namespace Trading.Core.Models
{
    public class SymbolPriceState
    {
        public required string Symbol { get; set; }
        public decimal BidPrice { get; set; }
        public decimal AskPrice { get; set; }
        public decimal CurrentMarketPrice { get; set; }
        public decimal Spread { get; set; }
        public decimal SpreadPercent { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
