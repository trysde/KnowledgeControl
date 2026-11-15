using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Serilog;
using Shared.Common.Middleware;

namespace Shared.Common.Extensions;

public static class CommonServiceExtensions
{
    // Метод для настройки Сервера логирования (Serilog)
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

    // Метод для добавления общих веб-сервисов (Controllers, Swagger, ApiExplorer)
    public static IServiceCollection AddCommonWebServices(this IServiceCollection services, string serviceTitle)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // Настройка Swagger/OpenAPI (в v3 классах пространство имен Models больше не нужно)
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = serviceTitle,
                Version = "v1"
            });
        });

        return services;
    }

    // Метод для подключения нашего кастомного middleware обработки ошибок
    public static IApplicationBuilder UseCommonMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Глобальный перехватчик исключений ставим самым первым в пайплайне
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Swagger включаем только в режиме Development
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