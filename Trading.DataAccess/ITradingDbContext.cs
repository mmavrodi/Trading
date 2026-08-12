using Microsoft.EntityFrameworkCore;
using Trading.Core.Models;

namespace Trading.DataAccess
{
    public interface ITradingDbContext
    {
        DbSet<SymbolPriceState> SymbolPrices { get; set; }
        DbSet<TradeOrder> Orders { get; set; }

        Task<int> SaveAsync(CancellationToken cancellationToken);
    }
}
