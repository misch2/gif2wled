using Microsoft.Extensions.Caching.Memory;
using WLEDControlApi.Business;
using WLEDControlApi.Interfaces;
using WLEDControlApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

// Add logging to the application
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

// setup DI
builder.Services.AddScoped<IWLEDService, WLEDService>();
builder.Services.AddScoped<IChannelMapper, ChannelMapperWLED>();

builder.Services.AddSingleton<IDNSCacheService>(sp => new DNSCacheService(sp.GetRequiredService<IMemoryCache>(), TimeSpan.FromMinutes(60)));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseRouting();  // Map requests to endpoints
app.MapControllers();  // Map controller routes

app.Run();
