using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Common.Extensions;
using TestTakingService.Infrastructure.Caching;
using TestTakingService.Infrastructure.HttpClients;
using TestTakingService.Infrastructure.Messaging;
using TestTakingService.Infrastructure.Persistence;
using TestTakingService.Services.Implementations;
using TestTakingService.Services.Interfaces;

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

var app = builder.Build();

await app.ApplyMigrationsAsync<TestTakingDbContext>();

app.UseCommonMiddleware(app.Environment);
app.UseAuthorization();
app.MapControllers();

app.Run();