namespace Trading.DataAccess
{
    public static class DbTableNames
    {
        public const string Orders = "Orders";
        public const string Prices = "SymbolPrices";
    }

    public static class DbSchemas
    {
        public const string Dbo = "dbo";
    }

    public static class DbColumnTypes
    {
        public const string PriceDecimal = "decimal(18, 4)";
        public const string QuantityDecimal = "decimal(22, 6)";
    }
}
