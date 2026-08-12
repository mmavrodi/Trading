using Trading.Core.Models;
using Trading.DataAccess;
using Trading.DTO.Models;
using Trading.Repository.Contracts;
using Trading.Service.Contracts;

namespace Trading.Services
{
    public class TradingRulesService : BaseService, ITradingRulesService
    {
        private readonly ITradingRulesRepository _rulesRepository;

        public TradingRulesService(ITradingRulesRepository rulesRepository, TradingDbContext dbContext) : base(dbContext)
        {
            _rulesRepository = rulesRepository;
        }

        public ValidationResultDTO ValidateOrder(TradeOrder order, SymbolPriceState? currentPrice)
        {
            var config = _rulesRepository.GetRules();

            if (order.Quantity > config.MaxQuantity)
            {
                return new ValidationResultDTO(false, $"Order quantity {order.Quantity} exceeds max allowed {config.MaxQuantity}.");
            }

            decimal notional = order.Price * order.Quantity;
            if (notional > config.MaxNotionalAmount)
            {
                return new ValidationResultDTO(false, $"Notional amount {notional} exceeds max limit {config.MaxNotionalAmount}.");
            }

            if (config.SymbolWhitelistEnabled && !config.WhitelistedSymbols.Contains(order.Symbol, StringComparer.OrdinalIgnoreCase))
            {
                return new ValidationResultDTO(false, $"Symbol '{order.Symbol}' is not whitelisted.");
            }

            if (currentPrice != null)
            {
                decimal deviationPercent = Math.Abs(order.Price - currentPrice.CurrentMarketPrice) / currentPrice.CurrentMarketPrice * 100m;
                if (deviationPercent > config.PriceDeviationThresholdPercent)
                {
                    return new ValidationResultDTO(false, $"Price deviation {deviationPercent:F2}% exceeds allowed threshold {config.PriceDeviationThresholdPercent}%.");
                }
            }

            if (config.DuplicateOrderIdCheckEnabled)
            {
                bool exists = _dbContext.Orders.Any(o => o.ClientOrderId == order.ClientOrderId);
                if (exists)
                {
                    return new ValidationResultDTO(false, $"Duplicate ClientOrderId '{order.ClientOrderId}'.");
                }
            }

            return new ValidationResultDTO(true, null);
        }

        public TradingRulesConfigurations GetRules()
        {
            return _rulesRepository.GetRules();
        }

        public void UpdateRules(TradingRulesConfigurations newRules)
        {
            _rulesRepository.UpdateRules(newRules);
        }
    }
}
