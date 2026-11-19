using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Common.Extensions;
using TestTakingService.Infrastructure.Persistence;

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

var app = builder.Build();

await app.ApplyMigrationsAsync<TestTakingDbContext>();

app.UseCommonMiddleware(app.Environment);
app.UseAuthorization();
app.MapControllers();

app.Run();