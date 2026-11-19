using System.Text;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Shared.Common.Extensions;
using TestTakingService.Infrastructure.Caching;
using TestTakingService.Infrastructure.HttpClients;
using TestTakingService.Infrastructure.Messaging;
using TestTakingService.Infrastructure.Persistence;
using TestTakingService.Services.Implementations;
using TestTakingService.Services.Interfaces;
using UserService.Options;

var builder = WebApplication.CreateBuilder(args);

CommonServiceExtensions.ConfigureSerilog(builder.Configuration, "TestTakingService");
builder.Host.UseSerilog();

builder.Services.AddCommonWebServices("Test Taking Service API");
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<TestTakingDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", TestTakingDbContext.SchemaName)));

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

builder.Services.AddAuthorization();

// Redis кэш снапшотов попыток
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "KnowledgeControl:";
});

// Типизированный HttpClient к TestCreationService
builder.Services.AddHttpClient<ITestCreationClient, TestCreationHttpClient>(client =>
{
    var baseUrl = builder.Configuration["Services:TestCreationService:BaseUrl"]
        ?? throw new InvalidOperationException("Services:TestCreationService:BaseUrl is not configured.");
    var internalApiKey = builder.Configuration["Services:TestCreationService:InternalApiKey"]
        ?? throw new InvalidOperationException("Services:TestCreationService:InternalApiKey is not configured.");

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("X-Internal-Api-Key", internalApiKey);
});

// RabbitMQ + MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitSection = builder.Configuration.GetSection("RabbitMq");
        var host = rabbitSection["Host"] ?? "localhost";
        var port = rabbitSection.GetValue<ushort?>("Port") ?? 5672;
        var virtualHost = rabbitSection["VirtualHost"] ?? "/";
        var username = rabbitSection["Username"] ?? "guest";
        var password = rabbitSection["Password"] ?? "guest";

        cfg.Host(host, port, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
        });
    });
});

builder.Services.AddScoped<IAttemptSnapshotCache, RedisAttemptSnapshotCache>();
builder.Services.AddScoped<IMessagePublisher, RabbitMqMessagePublisher>();
builder.Services.AddSingleton<GradingService>();
builder.Services.AddScoped<IAttemptService, AttemptService>();

var app = builder.Build();

await app.ApplyMigrationsAsync<TestTakingDbContext>();

app.UseCommonMiddleware(app.Environment);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();