using Trading.DTO.Models.Enums;

namespace Trading.DTO.Models
{
    public record TradeFilterDTO(
        string? Symbol,
        OrderStatus? Status,
        DateTime? FromDate,
        DateTime? ToDate
    );
}
