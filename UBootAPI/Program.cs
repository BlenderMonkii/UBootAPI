using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using UBootAPI;
using UBootAPI.Factory;
using UBootAPI.Service;
using UBootAPI.Wrapper;

var builder = WebApplication.CreateBuilder(args);

// CORS-Policy hinzufügen
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = null; // Kein Limit für die Anfragegröße
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = null; // Kein Limit für IIS
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddControllers();
builder.Services.AddScoped<IHeightmapService, HeightmapService>();
builder.Services.AddScoped<IConverterFactory, ConverterFactory>();
builder.Services.AddSingleton<GDALWrapper>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

// CORS aktivieren
//app.UseCors("AllowSpecific");
app.UseCors("AllowAll");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.RoutePrefix = string.Empty; // Swagger direkt unter https://localhost:7277 aufrufen
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
