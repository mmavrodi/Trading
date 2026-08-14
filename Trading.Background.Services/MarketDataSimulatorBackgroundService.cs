using Microsoft.Extensions.Hosting;
using System.Threading.Channels;
using Trading.DTO.Models;

namespace Trading.Background.Services
{
    public class MarketDataSimulatorBackgroundService : BackgroundService
    {
        private readonly Channel<PriceUpdateDTO> _channel;
        private readonly string[] _symbols = { "AAAA", "BBBB", "CCCC", "DDDD", "EEEE", "HHHH", "IIII", "JJJJ", "KKKK", "LLLL" };
        private readonly Random _random = new();

        public MarketDataSimulatorBackgroundService(Channel<PriceUpdateDTO> channel)
        {
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = _symbols.Select(symbol => SimulateSymbolAsync(symbol, stoppingToken));
            await Task.WhenAll(tasks);
        }

        private async Task SimulateSymbolAsync(string symbol, CancellationToken stoppingToken)
        {
            decimal basePrice = _random.Next(50, 500);

            while (!stoppingToken.IsCancellationRequested)
            {
                decimal delta = (decimal)(_random.NextDouble() - 0.49) * 2m;
                basePrice = Math.Max(10m, basePrice + delta);

                decimal spread = Math.Round((decimal)(_random.NextDouble() * 0.5 + 0.01), 2);
                decimal bid = Math.Round(basePrice, 2);
                decimal ask = bid + spread;

                var update = new PriceUpdateDTO(symbol, bid, ask, DateTime.UtcNow);

                await _channel.Writer.WriteAsync(update, stoppingToken);
                await Task.Delay(_random.Next(200, 1000), stoppingToken);
            }
        }
    }
}
