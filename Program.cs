using System.Text;
using KanbamApi.Repo;
using KanbamApi.Services;
using KanbamApi.Services.AuthServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// load .env file variables
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<AuthService>();
builder.Services.AddScoped<IAuthControllerService, AuthControllerService>();

builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<ListsService>();
builder.Services.AddSingleton<CardsService>();
builder.Services.AddSingleton<KanbamDbRepository>();

builder.Services.AddControllers();

builder.Services.AddCors((options) =>
{
    options.AddPolicy("DevCors", (corsBuilder) =>
    { 
        corsBuilder.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:8000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        ;
    });

    options.AddPolicy("ProdCors", (corsBuilder) =>
    { 
        corsBuilder.WithOrigins( "https://kanbam.netlify.app","http://localhost:5173")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        ;
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = false, // true ==>  later after hosted
            ValidateAudience = false, // true ==>  later after hosted
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    DotNetEnv.Env.GetString("TOKEN_KEY")
                )
            ),
            // ValidIssuer = DotNetEnv.Env.GetString("VALID_ISSUER"), 
            // ValidAudience = DotNetEnv.Env.GetString("VALID_AUDIENCE"),
            
        };
    }
);

var app = builder.Build();

  app.Use(async (context, next) =>
    {
        string userAgent = context.Request.Headers["User-Agent"].ToString();
        // Check if user-agent contains Chrome, Firefox or Safari
        if (userAgent.Contains("Chrome") || userAgent.Contains("Firefox") || userAgent.Contains("Safari"))
        {    
            // Allow request to proceed
            await next(); 
        }
        else
        {
            // Forbidden
            context.Response.StatusCode = 403; 
        }
    });


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
} else {
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
