using Microsoft.EntityFrameworkCore;
using Trading.Core.Models;

namespace Trading.DataAccess
{
    public class TradingDbContext : DbContext, ITradingDbContext
    {
        public TradingDbContext(DbContextOptions<TradingDbContext> options)
            : base(options)
        {
           
        }

        public DbSet<SymbolPriceState> SymbolPrices { get; set; }
        public DbSet<TradeOrder> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }

        public async Task<int> SaveAsync(CancellationToken cancellationToken)
        {
            return await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
