using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SearchService.Infrastructure.Persistence;
using Serilog;
using Shared.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

CommonServiceExtensions.ConfigureSerilog(builder.Configuration, "SearchService");
builder.Host.UseSerilog();

builder.Services.AddCommonWebServices("Search Service API");
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<SearchDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "search_schema")));

var app = builder.Build();

await app.ApplyMigrationsAsync<SearchDbContext>();

app.UseCommonMiddleware(app.Environment);

app.UseAuthorization();

app.MapControllers();

app.Run();