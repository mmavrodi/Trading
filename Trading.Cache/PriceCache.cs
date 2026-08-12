using System.Collections.Concurrent;
using Trading.Core.Models;
using Trading.DTO.Models;

namespace Trading.Cache
{
    public class PriceCache : IPriceCache
    {
        private readonly ConcurrentDictionary<string, SymbolPriceState> _cache = new();

        public (SymbolPriceState? Previous, SymbolPriceState Current) UpdateAndGetPrevious(PriceUpdateDTO update)
        {
            if (update.BidPrice >= update.AskPrice)
                throw new ArgumentException($"Invalid prices for {update.Symbol}: Bid ({update.BidPrice}) must be < Ask ({update.AskPrice})");

            decimal currentMarketPrice = (update.BidPrice + update.AskPrice) / 2m;
            decimal spread = update.AskPrice - update.BidPrice;
            decimal spreadPercent = (spread / currentMarketPrice) * 100m;

            var newState = new SymbolPriceState
            {
                Symbol = update.Symbol,
                BidPrice = update.BidPrice,
                AskPrice = update.AskPrice,
                CurrentMarketPrice = currentMarketPrice,
                Spread = spread,
                SpreadPercent = spreadPercent,
                Timestamp = update.Timestamp
            };

            SymbolPriceState? previousState = null;

            _cache.AddOrUpdate(
                update.Symbol,
                newState,
                (key, oldState) =>
                {
                    previousState = oldState;
                    return newState;
                });

            return (previousState, newState);
        }

        public SymbolPriceState? GetLatest(string symbol)
        {
            _cache.TryGetValue(symbol, out var state);
            return state;
        }
    }
}
