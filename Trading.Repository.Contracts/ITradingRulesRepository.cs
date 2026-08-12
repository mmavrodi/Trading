using Trading.Core.Models;

namespace Trading.Repository.Contracts
{
    public interface ITradingRulesRepository
    {
        TradingRulesConfigurations GetRules();
        void UpdateRules(TradingRulesConfigurations newRules);
    }
}
