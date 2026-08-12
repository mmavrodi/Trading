using Trading.Core.Models;
using Trading.Repository.Contracts;

namespace Trading.Repository
{
    public class TradingRulesRepository : ITradingRulesRepository
    {
        private readonly object _lock = new();
        private TradingRulesConfigurations _config = new();

        public TradingRulesConfigurations GetRules()
        {
            lock (_lock)
            {
                return _config;
            }
        }

        public void UpdateRules(TradingRulesConfigurations rules)
        {
            lock (_lock)
            {
                _config = rules;
            }
        }
    }
}
