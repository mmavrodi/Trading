namespace Trading.Core.Models
{
    public class TradingRulesConfigurations
    {
        public decimal MaxNotionalAmount { get; set; } = 100_000m;
        public decimal MaxQuantity { get; set; } = 1_000m;
        public decimal PriceDeviationThresholdPercent { get; set; } = 0.8m;
        public bool DuplicateOrderIdCheckEnabled { get; set; } = true;
        public bool SymbolWhitelistEnabled { get; set; } = true;
        public List<string> WhitelistedSymbols { get; set; } = new()
    {
        "AAAA", "BBBB", "CCCC", "DDDD", "EEEE",
        "HHHH", "IIII", "JJJJ", "KKKK", "LLLL"
    };
        public decimal AutoTradeSpreadThresholdPercent { get; set; } = 0.05m;
    }
}
