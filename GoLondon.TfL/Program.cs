using System.Reflection;
using Microsoft.OpenApi.Models;
using GoLondon.TfL.Services.Domain.ServiceCollections.TfL;
using GoLondon.TfL.Services.Domain.TfL;
using GoLondon.TfL.Services.TfL;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environmentName}.json", false)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
}

builder.Services.AddControllers().AddNewtonsoftJson(o =>
{
    o.AllowInputFormatterExceptionMessages = true;
});

builder
    .Services
    .AddTfLApi(builder.Configuration.GetSection("TflAPI"))
    .AddScoped<IStopPointService, StopPointService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(o =>
    {
        o.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Go London TfL API",
            Description = "An API to interact with the open TfL API",
            Contact = new OpenApiContact
            {
                Email = "tomknighton@icloud.com",
                Name = "Tom Knighton"
            }
        });
    });
}

builder.Host.UseSerilog();

var app = builder.Build();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        o.RoutePrefix = string.Empty;
    });
}


var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
    .AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.AppSettings()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

Log.Information("Startup!");

app.Run();