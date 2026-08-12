using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trading.Core.Models;

namespace Trading.DataAccess.Configurations
{
    public class SymbolPriceStateConfigurations : IEntityTypeConfiguration<SymbolPriceState>
    {
        public void Configure(EntityTypeBuilder<SymbolPriceState> builder)
        {
            builder.ToTable(DbTableNames.Prices, DbSchemas.Dbo);

            builder.HasKey(p => p.Symbol);

            builder.Property(p => p.Symbol)
                .HasColumnName(nameof(SymbolPriceState.Symbol))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.BidPrice)
                .HasColumnName(nameof(SymbolPriceState.BidPrice))
                .HasColumnType(DbColumnTypes.PriceDecimal)
                .IsRequired();

            builder.Property(p => p.AskPrice)
                .HasColumnName(nameof(SymbolPriceState.AskPrice))
                .HasColumnType(DbColumnTypes.PriceDecimal)
                .IsRequired();

            builder.Property(p => p.Spread)
                .HasColumnName(nameof(SymbolPriceState.Spread))
                .HasColumnType(DbColumnTypes.PriceDecimal)
                .IsRequired();

            builder.Property(p => p.CurrentMarketPrice)
                .HasColumnName(nameof(SymbolPriceState.CurrentMarketPrice))
                .HasColumnType(DbColumnTypes.PriceDecimal)
                .IsRequired();

            builder.Property(p => p.SpreadPercent)
                .HasColumnName(nameof(SymbolPriceState.SpreadPercent))
                .HasColumnType(DbColumnTypes.PriceDecimal)
                .IsRequired();

            builder.Property(p => p.Timestamp)
                .HasColumnName(nameof(SymbolPriceState.Timestamp))
                .IsRequired();
        }
    }
}
