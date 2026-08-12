using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trading.Core.Models;

namespace Trading.DataAccess.Configurations
{
    public class TradeOrderConfigurations : IEntityTypeConfiguration<TradeOrder>
    {
        public void Configure(EntityTypeBuilder<TradeOrder> builder)
        {
            builder.ToTable(DbTableNames.Orders, DbSchemas.Dbo);

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName(nameof(TradeOrder.Id))
                .IsRequired();

            builder.Property(p => p.Symbol)
                .HasColumnName(nameof(TradeOrder.Symbol))
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.ClientOrderId)
                .HasColumnName(nameof(TradeOrder.ClientOrderId))
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.Side)
                .HasColumnName(nameof(TradeOrder.Side))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.Price)
                .HasColumnName(nameof(TradeOrder.Price))
                .HasColumnType(DbColumnTypes.PriceDecimal)
                .IsRequired();

            builder.Property(p => p.Quantity)
                .HasColumnName(nameof(TradeOrder.Quantity))
                .HasColumnType(DbColumnTypes.QuantityDecimal)
                .IsRequired();

            builder.Property(p => p.Type)
                .HasColumnName(nameof(TradeOrder.Type))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.Status)
                .HasColumnName(nameof(TradeOrder.Status))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.Source)
                .HasColumnName(nameof(TradeOrder.Source))
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.RejectionReason)
                .HasColumnName(nameof(TradeOrder.RejectionReason))
                .HasMaxLength(250);

            builder.Property(p => p.CreatedAt)
                .HasColumnName(nameof(TradeOrder.CreatedAt))
                .IsRequired();
        }
    }
}
