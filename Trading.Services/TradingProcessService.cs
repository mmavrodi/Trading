using Microsoft.EntityFrameworkCore;
using Trading.Cache;
using Trading.Core.Models;
using Trading.DataAccess;
using Trading.DTO.Models;
using Trading.DTO.Models.Enums;
using Trading.Repository.Contracts;
using Trading.Service.Contracts;

namespace Trading.Services
{
    public class TradingProcessService : BaseService, ITradingProcessService
    {
        private readonly ITradingRulesService _rulesService;
        private readonly IPriceCache _priceCache;
        private readonly ITradingRulesRepository _rulesRepository;

        public TradingProcessService(
            ITradingRulesService rulesService,
            IPriceCache priceCache,
            ITradingRulesRepository rulesRepository,
            TradingDbContext dbContext) : base(dbContext)
        {
            _rulesService = rulesService;
            _priceCache = priceCache;
            _rulesRepository = rulesRepository;
        }

        public async Task<IEnumerable<TradeOrder>> GetTradeOrdersAsync(TradeFilterDTO filter, CancellationToken cancellationToken)
        {
            var query = _dbContext.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Symbol))
                query = query.Where(o => o.Symbol == filter.Symbol);

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value.ToString());

            if (filter.FromDate.HasValue)
                query = query.Where(x => x.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(x => x.CreatedAt <= filter.ToDate.Value);

            return await query.OrderByDescending(o => o.CreatedAt).Take(100).ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task<TradeOrder> ProcessManualOrderAsync(TradeDTO trade, CancellationToken cancellationToken)
        {
            var latestPrice = _priceCache.GetLatest(trade.Symbol);

            var order = new TradeOrder
            {
                ClientOrderId = trade.ClientOrderId,
                Symbol = trade.Symbol,
                Type = trade.Type.ToString(),
                Side = trade.Side.ToString(),
                Price = trade.Price,
                Quantity = trade.Quantity,
                Source = OrderSource.Manual.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            var validation = _rulesService.ValidateOrder(order, latestPrice);

            order.Status = (validation.IsValid ? OrderStatus.Accepted : OrderStatus.Rejected).ToString();
            order.RejectionReason = validation.RejectionReason;

            _dbContext.Orders.Add(order);
            await _dbContext.SaveAsync(cancellationToken).ConfigureAwait(false);

            return order;
        }
        public async Task EvaluateAndExecuteAutoTradeAsync(SymbolPriceState? previousState, SymbolPriceState currentState, CancellationToken cancellationToken)
        {
            if (previousState == null) return;

            var config = _rulesRepository.GetRules();

            if (currentState.SpreadPercent <= config.AutoTradeSpreadThresholdPercent) return;

            if (currentState.CurrentMarketPrice == previousState.CurrentMarketPrice) return;

            OrderSide side;
            decimal targetPrice;

            if (currentState.CurrentMarketPrice > previousState.CurrentMarketPrice)
            {
                side = OrderSide.Sell;
                targetPrice = currentState.AskPrice - (currentState.AskPrice * 0.0003m);
            }
            else
            {
                side = OrderSide.Buy;
                targetPrice = currentState.BidPrice + (currentState.BidPrice * 0.0003m);
            }

            decimal calculatedQuantity = Math.Round(10_000m / targetPrice, 2);

            var autoOrder = new TradeOrder
            {
                ClientOrderId = $"AUTO-{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Symbol = currentState.Symbol,
                Type = OrderType.Limit.ToString(),
                Side = side.ToString(),
                Price = Math.Round(targetPrice, 4),
                Quantity = calculatedQuantity,
                Source = OrderSource.Auto.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            var validation = _rulesService.ValidateOrder(autoOrder, currentState);

            autoOrder.Status = (validation.IsValid ? OrderStatus.Accepted : OrderStatus.Rejected).ToString();
            autoOrder.RejectionReason = validation.RejectionReason;

            _dbContext.Orders.Add(autoOrder);
            await _dbContext.SaveAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
