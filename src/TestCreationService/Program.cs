using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Common.Extensions;
using TestCreationService.Data;

var builder = WebApplication.CreateBuilder(args);

CommonServiceExtensions.ConfigureSerilog(builder.Configuration, "TestCreationService");
builder.Host.UseSerilog();

builder.Services.AddCommonWebServices("Test Creation Service API");
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<TestCreationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "test_creation_schema")));

var app = builder.Build();

await app.ApplyMigrationsAsync<TestCreationDbContext>();

app.UseCommonMiddleware(app.Environment);
app.UseAuthorization();
app.MapControllers();

app.Run();