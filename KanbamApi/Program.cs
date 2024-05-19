using System.Text;
using FluentValidation;
using KanbamApi.Data;
using KanbamApi.Data.Interfaces;
using KanbamApi.Models.AuthModels;
using KanbamApi.Repositories;
using KanbamApi.Repositories.Interfaces;
using KanbamApi.Services.AuthServices;
using KanbamApi.Validators.AuthValidators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// load .env file variables
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IKanbamDbContext, KanbamDbContext>();

builder.Services.AddScoped<IValidator<UserLogin>, UserLoginValidator>();
builder.Services.AddScoped<IValidator<UserRegistration>, UserRegistrationValidator>();

builder.Services.AddScoped<IAuthControllerService, AuthControllerService>();

builder.Services.AddScoped<IAuthRepo, AuthRepo>();
builder.Services.AddScoped<IUsersRepo, UsersRepo>();
builder.Services.AddScoped<IListsRepo, ListsRepo>();
builder.Services.AddScoped<ICardsRepo, CardsRepo>();

builder.Services.AddControllers();

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
                    .WithOrigins("https://kanbam.netlify.app", "http://localhost:5173") // vite localHost address
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        );
    }
);

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = false, // true ==>  later after hosted
            ValidateAudience = false, // true ==>  later after hosted
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(DotNetEnv.Env.GetString("TOKEN_KEY"))
            ),
            // ValidIssuer = DotNetEnv.Env.GetString("VALID_ISSUER"),
            // ValidAudience = DotNetEnv.Env.GetString("VALID_AUDIENCE"),
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
app.Use(
    async (context, next) =>
    {
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
        await next();
    }
);

// Configure the HTTP request pipeline.
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

app.MapControllers();

app.Run();
