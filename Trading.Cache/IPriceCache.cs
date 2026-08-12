using Trading.Core.Models;
using Trading.DTO.Models;

namespace Trading.Cache
{
    public interface IPriceCache
    {
        (SymbolPriceState? Previous, SymbolPriceState Current) UpdateAndGetPrevious(PriceUpdateDTO update);
        SymbolPriceState? GetLatest(string symbol);
    }
}
