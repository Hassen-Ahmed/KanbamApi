using System.Text;
using FluentValidation;
using KanbamApi.Data;
using KanbamApi.Data.Interfaces;
using KanbamApi.Data.Seed;
using KanbamApi.Hubs;
using KanbamApi.Models;
using KanbamApi.Repositories;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services;
using KanbamApi.Services.Interfaces;
using KanbamApi.Util.Generators.SecureData;
using KanbamApi.Util.Generators.SecureData.Interfaces;
using KanbamApi.Util.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSingleton<IKanbamDbContext, KanbamDbContext>();

builder.Services.AddScoped<IValidator<RefreshToken>, RefreshTokenValidator>();
builder.Services.AddTransient<IGeneralValidation, GeneralValidation>();
builder.Services.AddScoped<IAuthData, AuthData>();

// Repos
builder.Services.AddScoped<IAuthRepo, AuthRepo>();
builder.Services.AddScoped<IUsersRepo, UsersRepo>();
builder.Services.AddScoped<IRefreshTokenRepo, RefreshTokenRepo>();
builder.Services.AddScoped<IListsRepo, ListsRepo>();
builder.Services.AddScoped<ICardsRepo, CardsRepo>();
builder.Services.AddScoped<IBoardsRepo, BoardsRepo>();
builder.Services.AddScoped<IBoardMemberRepo, BoardMemberRepo>();
builder.Services.AddScoped<IWorkspacesRepo, WorkspacesRepo>();
builder.Services.AddScoped<IWorkspaceMembersRepo, WorkspaceMembersRepo>();

// Services
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ICardsService, CardsService>();
builder.Services.AddScoped<IListsService, ListsService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IBoardMemberService, BoardMemberService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IWorkspaceMemberService, WorkspaceMemberService>();

// Seeding for testing
builder.Services.AddScoped<IMongoDbSeeder, MongoDbSeeder>();

//
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddCors(
    (options) =>
    {
        options.AddPolicy(
            "DevCors",
            (corsBuilder) =>
            {
                corsBuilder
                    .WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:3000",
                        "http://localhost:8000"
                    )
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
                    .WithOrigins("https://kanbam.netlify.app", "https://kanbam.onrender.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        );
    }
);

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

// if(OperatingSystem.IsWindows()) {
//     builder.Services.AddDataProtection().ProtectKeysWithDpapi();
// }



var app = builder.Build();

// Allow certain browsers
//   app.Use(async (context, next) =>
//     {
//         string userAgent = context.Request.Headers["User-Agent"].ToString();
//         // Check if user-agent contains Chrome, Firefox or Safari
//         if (userAgent.Contains("Chrome") || userAgent.Contains("Firefox") || userAgent.Contains("Safari"))
//         {
//             // Allow request to proceed
//             await next();
//         }
//         else
//         {
//             // Forbidden
//             context.Response.StatusCode = 403;
//         }
//     });

// Add Content Security Policy (CSP)
// app.Use(
//     async (context, next) =>
//     {
//         context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
//         await next();
//     }
// );

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // IKanbamDbContext kanbamDbContext = new KanbamDbContext();
    // MongoDbSeeder mongoDbSeeder = new(kanbamDbContext);
    // mongoDbSeeder.SeedAsync().Wait();

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
app.MapControllers();

app.Run();
