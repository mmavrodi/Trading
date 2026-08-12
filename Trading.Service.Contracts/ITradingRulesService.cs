using Trading.Core.Models;
using Trading.DTO.Models;

namespace Trading.Service.Contracts
{
    public interface ITradingRulesService
    {
        ValidationResultDTO ValidateOrder(TradeOrder order, SymbolPriceState? currentPrice);
        TradingRulesConfigurations GetRules();
        void UpdateRules(TradingRulesConfigurations newRules);
    }
}
