using Trading.Core.Models;
using Trading.DTO.Models;

namespace Trading.Service.Contracts
{
    public interface ITradingProcessService
    {
        Task<IEnumerable<TradeOrder>> GetTradeOrdersAsync(TradeFilterDTO filter, CancellationToken cancellationToken);
        Task<TradeOrder> ProcessManualOrderAsync(TradeDTO trade, CancellationToken cancellationToken);
        Task EvaluateAndExecuteAutoTradeAsync(SymbolPriceState? previousState, SymbolPriceState currentState, CancellationToken cancellationToken);
    }
}
