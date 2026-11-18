using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Serilog;
using Shared.Common.Middleware;

namespace Shared.Common.Extensions;

public static class CommonServiceExtensions
{
    public static void ConfigureSerilog(IConfiguration configuration, string serviceName)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .WriteTo.Console()
            .WriteTo.File($"logs/service-{serviceName.ToLower()}.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public static IServiceCollection AddCommonWebServices(this IServiceCollection services, string serviceTitle)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<Shared.Common.Filters.ValidationFilter>();
        });
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = serviceTitle,
                Version = "v1"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Введите JWT-токен (без слова Bearer — оно подставится автоматически)."
            });

            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        return services;
    }

    public static async Task ApplyMigrationsAsync<TContext>(this WebApplication app) where TContext : DbContext
    {
        var autoMigrate = app.Environment.IsDevelopment()
                          || string.Equals(app.Configuration["AUTO_MIGRATE"], "true", StringComparison.OrdinalIgnoreCase);

        if (!autoMigrate)
            return;

        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            logger.LogInformation("Applying database migrations for {DbContext}...", typeof(TContext).Name);
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations for {DbContext} applied successfully.", typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error occurred while applying migrations for {DbContext}.", typeof(TContext).Name);
            throw;
        }
    }

    public static IApplicationBuilder UseCommonMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{env.ApplicationName} V1");
            });
        }

        return app;
    }
}