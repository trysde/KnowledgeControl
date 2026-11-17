using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Common.Extensions;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Настраиваем Serilog
CommonServiceExtensions.ConfigureSerilog(builder.Configuration, "UserService");
builder.Host.UseSerilog();

// 2. Добавляем общие веб-сервисы (Controllers, Swagger, ApiExplorer)
builder.Services.AddCommonWebServices("User Service API");

// 3. Подключаем FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// 4. Подключаем PostgreSQL (UserDbContext) со своей таблицей истории миграций в user_schema
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "user_schema")
    ));

var app = builder.Build();

// 5. Подключаем общий Middleware (ошибки + Swagger в Development)
app.UseCommonMiddleware(app.Environment);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// 6. Автоматически применяем миграции в Development
await app.ApplyMigrationsAsync<UserDbContext>();

app.Run();