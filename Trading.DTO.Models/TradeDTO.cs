using Trading.DTO.Models.Enums;

namespace Trading.DTO.Models
{
    public record TradeDTO(
        string ClientOrderId,
        string Symbol,
        OrderType Type,
        OrderSide Side,
        decimal Price,
        decimal Quantity
    );
}
