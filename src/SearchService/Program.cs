using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Consumers;
using SearchService.Infrastructure.Caching;
using SearchService.Infrastructure.Persistence;
using SearchService.Services.Implementations;
using SearchService.Services.Interfaces;
using Serilog;
using Shared.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

CommonServiceExtensions.ConfigureSerilog(builder.Configuration, "SearchService");
builder.Host.UseSerilog();

var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "KnowledgeControl:Search:";
});

builder.Services.AddCommonWebServices("Search Service API");
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<SearchDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "search_schema")));

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumersFromNamespaceContaining<TestPublishedConsumer>();

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

        cfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15)));

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<ISearchCache, RedisSearchCache>();
builder.Services.AddScoped<ISearchService, SearchService.Services.Implementations.SearchService>();
builder.Services.AddScoped<IIndexService, IndexService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

var app = builder.Build();

await app.ApplyMigrationsAsync<SearchDbContext>();

app.UseCommonMiddleware(app.Environment);

app.UseAuthorization();

app.MapControllers();

app.Run();