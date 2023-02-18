using Microsoft.OpenApi.Models;
using GoLondon.TfL.Services.Domain.ServiceCollections.TfL;
using GoLondon.TfL.Services.Domain.TfL;
using GoLondon.TfL.Services.TfL;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

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

app.Run();