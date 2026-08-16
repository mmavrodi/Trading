using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using Trading.Api.Configurations;
using Trading.Api.Middlewares;
using Trading.Background.Services;
using Trading.Cache;
using Trading.DataAccess;
using Trading.DTO.Models;
using Trading.Repository;
using Trading.Repository.Contracts;
using Trading.Service.Contracts;
using Trading.Services;
using Trading.Services.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<TradeDTOValidator>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.HttpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" }
        };
    };
});


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

app.UseMiddleware<CustomErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
