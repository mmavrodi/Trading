namespace Trading.Core.Models
{
    public class TradeOrder
    {
        public Guid Id { get; set; }
        public required string ClientOrderId { get; set; }
        public required string Symbol { get; set; }
        public required string Type { get; set; }
        public required string Side { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public required string Source { get; set; }
        public string? Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
