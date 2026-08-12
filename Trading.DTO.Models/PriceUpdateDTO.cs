namespace Trading.DTO.Models
{
    public record PriceUpdateDTO(
        string Symbol,
        decimal BidPrice,
        decimal AskPrice,
        DateTime Timestamp
    );

}
