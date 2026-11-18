using System.Text;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Shared.Common.Extensions;
using UserService.Data;
using UserService.Options;
using UserService.Services.Implementations;
using UserService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

CommonServiceExtensions.ConfigureSerilog(builder.Configuration, "UserService");
builder.Host.UseSerilog();

// Базовые веб-сервисы, Swagger и ValidationFilter
builder.Services.AddCommonWebServices("UserService API");
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// PostgreSQL (схема user_schema)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "user_schema")));

// Настройки и аутентификация JWT
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);

var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration section is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireModerator", policy =>
        policy.RequireClaim("is_moderator", "true"));

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        var rabbitUser = builder.Configuration["RabbitMq:Username"] ?? "guest";
        var rabbitPass = builder.Configuration["RabbitMq:Password"] ?? "guest";

        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });
    });
});

// Регистрация сервисов приложения
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserProfileService>();
builder.Services.AddScoped<IBanService, BanService>();
builder.Services.AddScoped<IMessagePublisher, RabbitMqMessagePublisher>();

var app = builder.Build();

await app.ApplyMigrationsAsync<UserDbContext>();

app.UseCommonMiddleware(app.Environment);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();