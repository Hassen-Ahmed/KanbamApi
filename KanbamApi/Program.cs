using System.Collections.Concurrent;
using System.Text;
using System.Threading.RateLimiting;
using AspNetCore.Identity.Mongo;
using KanbamApi.Core;
using KanbamApi.Data;
using KanbamApi.Data.Interfaces;
using KanbamApi.Data.Seed;
using KanbamApi.Hubs;
using KanbamApi.Models.MongoDbIdentity;
using KanbamApi.Repositories;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services;
using KanbamApi.Services.Email;
using KanbamApi.Services.Interfaces;
using KanbamApi.Services.Interfaces.Email;
using KanbamApi.Util.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

// build environment path
var env = builder.Environment.EnvironmentName.ToLower();

if (env == "test" || env == "development")
{
    DotNetEnv.Env.Load($".env.{env}");
}
else
{
    DotNetEnv.Env.Load();
}

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSingleton<IKanbamDbContext, KanbamDbContext>();
builder.Services.AddTransient<IGeneralValidation, GeneralValidation>();

// Repos
builder.Services.AddScoped<IListsRepo, ListsRepo>();
builder.Services.AddScoped<ICardsRepo, CardsRepo>();
builder.Services.AddScoped<IBoardsRepo, BoardsRepo>();
builder.Services.AddScoped<IBoardMemberRepo, BoardMemberRepo>();
builder.Services.AddScoped<IWorkspacesRepo, WorkspacesRepo>();
builder.Services.AddScoped<IWorkspaceMembersRepo, WorkspaceMembersRepo>();

// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICardsService, CardsService>();
builder.Services.AddScoped<IListsService, ListsService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IBoardMemberService, BoardMemberService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IWorkspaceMemberService, WorkspaceMemberService>();

// Email service
builder.Services.AddTransient<SmtpEmailService>();
builder.Services.AddTransient<SendGridEmailService>();
builder.Services.AddTransient<IEmailService, FallbackEmailService>();
builder.Services.AddSingleton<IEmailServiceFactory, EmailServiceFactory>();

// Seeding for testing
builder.Services.AddScoped<IMongoDbSeeder, MongoDbSeeder>();

//
builder.Services.AddControllers();
builder.Services.AddSignalR();

// cors service section
builder.Services.AddCors(
    (options) =>
    {
        options.AddPolicy(
            "DevCors",
            (corsBuilder) =>
            {
                corsBuilder
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        );

        options.AddPolicy(
            "ProdCors",
            (corsBuilder) =>
            {
                corsBuilder
                    .WithOrigins("https://kanbam.netlify.app")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        );
    }
);

// Identity service
builder
    .Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        // Lockout
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        options.Lockout.MaxFailedAccessAttempts = 10;

        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@.-_";
    })
    .AddRoles<ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, ObjectId>(mongo =>
    {
        mongo.ConnectionString = DotNetEnv.Env.GetString("CONNECTION_STRING");
        mongo.UsersCollection = "Users";
        mongo.RolesCollection = "Roles";
    })
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
{
    opt.TokenLifespan = TimeSpan.FromMinutes(30);
});

// HttpClient
builder.Services.AddHttpClient();

builder.Services.AddSingleton<ICloudFlareTurnstileService>(ServiceProvider =>
{
    var secretKey = DotNetEnv.Env.GetString("CLOUDFLARE_TURNSTILE_SECRET_KEY");
    var httpClientFactory = ServiceProvider.GetRequiredService<IHttpClientFactory>();

    return new CloudFlareTurnstileService(httpClientFactory, secretKey);
});

// Authentication service
builder
    .Services.AddAuthentication(
        (options) =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    )
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(0),
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(DotNetEnv.Env.GetString("TOKEN_KEY"))
            ),
            ValidIssuer = DotNetEnv.Env.GetString("VALID_ISSUER"),
            ValidAudience = DotNetEnv.Env.GetString("VALID_AUDIENCE"),
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = (context) =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/kanbamHubs")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Thread Safety for rate limiting service (SlidingWindowStrictPolicy)
var violationTracker = new ConcurrentDictionary<string, double>();

// Rate limiting services
builder.Services.AddRateLimiter(opts =>
{
    opts.AddFixedWindowLimiter(
        "FixedWindow",
        limiterOptions =>
        {
            limiterOptions.PermitLimit = 100;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        }
    );

    opts.AddPolicy(
        "SlidingWindowStrictPolicy",
        context =>
        {
            // I am using the user id as the partition key, instead of the IP address if user is authenticated
            var clientId =
                context.User.Identity?.IsAuthenticated == true
                    ? context.User.FindFirst("userId")?.Value
                    : context.Connection.RemoteIpAddress?.ToString();

            var violations = violationTracker.GetOrAdd(clientId!, 0);
            var dynamicWindow = TimeSpan.FromMinutes(1 + Math.Ceiling(violations));

            return RateLimitPartition.GetSlidingWindowLimiter(
                clientId,
                _ =>
                    new()
                    {
                        PermitLimit = 5,
                        SegmentsPerWindow = 2,
                        Window = dynamicWindow,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    }
            );
        }
    );

    opts.OnRejected = (context, _) =>
    {
        var clientId =
            context.HttpContext.User.Identity?.IsAuthenticated == true
                ? context.HttpContext.User.FindFirst("userId")?.Value
                : context.HttpContext.Connection.RemoteIpAddress?.ToString();

        if (clientId != null)
        {
            // Increment violation count
            violationTracker.AddOrUpdate(clientId, 0.2, (_, count) => count + 0.2);
        }
        // default response for rate limiting was 503 and known is 429
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return ValueTask.CompletedTask;
    };
});

// Reset violation count after a period (e.g., 1 hour) to prevent memory leak from long-lived clients
var cleanupTimer = new Timer(
    _ =>
    {
        violationTracker.Clear();
    },
    null,
    TimeSpan.FromHours(1),
    TimeSpan.FromHours(1)
);

////////////////////////////////////////////////////////////////////////////////////////////
// build app

var app = builder.Build();

app.UseRateLimiter();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = ErrorMessages.ServerError });
    });
});

// Set stripe secret key for payment here to minimize the number of times it is configured
Stripe.StripeConfiguration.ApiKey = DotNetEnv.Env.GetString("STRIPE_SECRET_KEY");

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<BoardHub>("/kanbamHubs/boardHub");
app.MapHub<ListHub>("/kanbamHubs/listHub");
app.MapHub<CardHub>("/kanbamHubs/cardHub");

app.MapControllers();

app.Run();
