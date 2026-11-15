using FluentValidation;
using Serilog;
using Shared.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Настраиваем Serilog с именем сервиса "UserService"
CommonServiceExtensions.ConfigureSerilog(builder.Configuration, "UserService");
builder.Host.UseSerilog();

// 2. Добавляем общие веб-сервисы (Controllers, Swagger, ApiExplorer)
builder.Services.AddCommonWebServices("User Service API");

// 3. Подключаем FluentValidation (автоматически ищет все валидаторы в этой сборке)
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// 4. Подключаем наш общий Middleware (ошибки + Swagger в Development)
app.UseCommonMiddleware(app.Environment);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();