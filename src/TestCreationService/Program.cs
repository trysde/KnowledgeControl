using FluentValidation;
using Serilog;
using Shared.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

CommonServiceExtensions.ConfigureSerilog(builder.Configuration, "TestCreationService");
builder.Host.UseSerilog();

builder.Services.AddCommonWebServices("Test Creation Service API");
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

app.UseCommonMiddleware(app.Environment);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();