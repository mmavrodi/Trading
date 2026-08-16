using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using Trading.Api.Configurations;
using Trading.Background.Services;
using Trading.Cache;
using Trading.DataAccess;
using Trading.DTO.Models;
using Trading.Repository;
using Trading.Repository.Contracts;
using Trading.Service.Contracts;
using Trading.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var settings = builder.Configuration.GetSection("ConfigurationSettings").Get<ConfigurationSettings>()!;

builder.Services.Configure<ConfigurationSettings>(
    builder.Configuration.GetSection("ConfigurationSettings"));

builder.Services.AddDbContext<TradingDbContext>(options =>
    options
        .UseSqlServer(settings.DbConnectionString)
   );

builder.Services.AddScoped<ITradingDbContext>(provider => provider.GetService<TradingDbContext>()!);

builder.Services.AddSingleton<IPriceCache, PriceCache>();

builder.Services.AddSingleton<ITradingRulesRepository, TradingRulesRepository>();

builder.Services.AddScoped<ITradingRulesService, TradingRulesService>();
builder.Services.AddScoped<ITradingProcessService, TradingProcessService>();

var priceChannel = Channel.CreateUnbounded<PriceUpdateDTO>(new UnboundedChannelOptions
{
    SingleReader = true
});
builder.Services.AddSingleton(priceChannel);

builder.Services.AddHostedService<MarketDataSimulatorBackgroundService>();
builder.Services.AddHostedService<PriceProcessorBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
