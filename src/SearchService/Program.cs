using FluentValidation;
using Serilog;
using Shared.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

CommonServiceExtensions.ConfigureSerilog(builder.Configuration, "SearchService");
builder.Host.UseSerilog();

builder.Services.AddCommonWebServices("Search Service API");
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

app.UseCommonMiddleware(app.Environment);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();