using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;
using Trading.Cache;
using Trading.DataAccess;
using Trading.DTO.Models;
using Trading.Service.Contracts;

namespace Trading.Background.Services
{
    public class PriceProcessorBackgroundService : BackgroundService
    {
        private readonly Channel<PriceUpdateDTO> _channel;
        private readonly IPriceCache _priceCache;
        private readonly IServiceScopeFactory _scopeFactory;

        public PriceProcessorBackgroundService(
            Channel<PriceUpdateDTO> channel,
            IPriceCache priceCache,
            IServiceScopeFactory scopeFactory)
        {
            _channel = channel;
            _priceCache = priceCache;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancelationToken)
        {
            await foreach (var priceUpdate in _channel.Reader.ReadAllAsync(cancelationToken))
            {
                try
                {
                    var (previousState, currentState) = _priceCache.UpdateAndGetPrevious(priceUpdate);

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ITradingDbContext>();
                    var tradeService = scope.ServiceProvider.GetRequiredService<ITradingProcessService>();

                    var existingPrice = await dbContext.SymbolPrices.FindAsync(new object[] { currentState.Symbol }, cancelationToken);
                    if (existingPrice == null)
                    {
                        dbContext.SymbolPrices.Add(currentState);
                    }
                    else
                    {
                        existingPrice.BidPrice = currentState.BidPrice;
                        existingPrice.AskPrice = currentState.AskPrice;
                        existingPrice.CurrentMarketPrice = currentState.CurrentMarketPrice;
                        existingPrice.Spread = currentState.Spread;
                        existingPrice.SpreadPercent = currentState.SpreadPercent;
                        existingPrice.Timestamp = currentState.Timestamp;
                    }
                    await dbContext.SaveAsync(cancelationToken);

                    await tradeService.EvaluateAndExecuteAutoTradeAsync(previousState, currentState, cancelationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing price update: {ex.Message}");
                }
            }
        }
    }
}
