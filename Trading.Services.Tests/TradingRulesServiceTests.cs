using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Trading.Core.Models;
using Trading.DataAccess;
using Trading.DTO.Models.Enums;
using Trading.Repository.Contracts;
using Trading.Service.Contracts;

namespace Trading.Services.Tests
{
    public class TradingRulesServiceTests
    {
        private readonly ITradingRulesRepository _rulesRepoMock;
        private readonly TradingDbContext _dbContext;
        private readonly ITradingRulesService _rulesServiceMock;

        public TradingRulesServiceTests()
        {
            _rulesRepoMock = A.Fake<ITradingRulesRepository>();
            var options = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TradingDbContext(options);

            // Default Rules Setup
            A.CallTo(() => _rulesRepoMock.GetRules()).Returns(new TradingRulesConfigurations
            {
                MaxQuantity = 100,
                MaxNotionalAmount = 10000,
                PriceDeviationThresholdPercent = 1.0m,
                WhitelistedSymbols = new List<string> { "AAAA" },
                SymbolWhitelistEnabled = true,
                DuplicateOrderIdCheckEnabled = true
            });

            _rulesServiceMock = new TradingRulesService(_rulesRepoMock, _dbContext);
        }

        [Fact]
        public void ValidateOrder_ShouldReject_WhenQuantityExceedsLimit()
        {
            // Arrange
            var order = new TradeOrder() 
            {  
                Quantity = 150, 
                Symbol = "AAAA", 
                Price = 10,
                ClientOrderId = string.Empty,
                Side = string.Empty,
                Source = string.Empty,
                Type = string.Empty
            };

            // Act
            var result = _rulesServiceMock.ValidateOrder(order, null);

            // Assert
            result.IsValid.Should().Be(false);
            result.RejectionReason.Should().Contain("exceeds max allowed");
        }

        [Fact]
        public void ValidateOrder_ShouldReject_WhenPriceDeviatesTooMuch()
        {
            // Arrange
            var currentPrice = new SymbolPriceState { Symbol = "AAAA", CurrentMarketPrice = 100m };
            var order = new TradeOrder()
            {
                Quantity = 1,
                Symbol = "AAAA",
                Price = 105,
                ClientOrderId = string.Empty,
                Side = string.Empty,
                Source = string.Empty,
                Type = string.Empty
            };

            // Act
            var result = _rulesServiceMock.ValidateOrder(order, currentPrice);

            // Assert
            result.IsValid.Should().Be(false);
            result.RejectionReason.Should().Contain("Price deviation");
        }

        [Fact]
        public void ValidateOrder_ShouldReject_WhenNotionalAmountTooMuch()
        {
            // Arrange
            var currentPrice = new SymbolPriceState { Symbol = "AAAA" };
            var order = new TradeOrder()
            {
                Quantity = 10,
                Symbol = "AAAA",
                Price = 10500,
                ClientOrderId = string.Empty,
                Side = string.Empty,
                Source = string.Empty,
                Type = string.Empty
            };

            // Act
            var result = _rulesServiceMock.ValidateOrder(order, currentPrice);

            // Assert
            result.IsValid.Should().Be(false);
            result.RejectionReason.Should().Contain($"Notional amount {105000} exceeds");
        }

        [Fact]
        public void ValidateOrder_ShouldReject_WhenNoWhitelisted()
        {
            // Arrange
            var currentPrice = new SymbolPriceState { Symbol = "ALABALA", CurrentMarketPrice = 100m };
            var order = new TradeOrder()
            {
                Quantity = 1,
                Symbol = "ALABALA",
                Price = 90,
                ClientOrderId = string.Empty,
                Side = string.Empty,
                Source = string.Empty,
                Type = string.Empty
            };

            // Act
            var result = _rulesServiceMock.ValidateOrder(order, currentPrice);

            // Assert
            result.IsValid.Should().Be(false);
            result.RejectionReason.Should().Contain(" is not whitelisted");
        }

        [Fact]
        public async Task ValidateOrder_ShouldReject_WhenDuplicateOrder()
        {
            // Arrange
            var currentPrice = new SymbolPriceState { Symbol = "AAAA", CurrentMarketPrice = 100m };
            var order = new TradeOrder()
            {
                Quantity = 1,
                Symbol = "AAAA",
                Price = 100,
                ClientOrderId = Guid.NewGuid().ToString("N").Substring(0, 8),
                Side = OrderSide.Buy.ToString(),
                Source = OrderSource.Manual.ToString(),
                Type = OrderType.Limit.ToString()
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = _rulesServiceMock.ValidateOrder(order, currentPrice);

            // Assert
            result.IsValid.Should().Be(false);
            result.RejectionReason.Should().Contain("Duplicate ClientOrderId");
        }

        [Fact]
        public async Task ValidateOrder_ShouldSucceed()
        {
            // Arrange
            var currentPrice = new SymbolPriceState { Symbol = "AAAA", CurrentMarketPrice = 100m };
            var order = new TradeOrder()
            {
                Quantity = 1,
                Symbol = "AAAA",
                Price = 100,
                ClientOrderId = Guid.NewGuid().ToString("N").Substring(0, 8),
                Side = OrderSide.Buy.ToString(),
                Source = OrderSource.Manual.ToString(),
                Type = OrderType.Limit.ToString()
            };

            // Act
            var result = _rulesServiceMock.ValidateOrder(order, currentPrice);

            // Assert
            result.IsValid.Should().Be(true);
            result.RejectionReason.Should().BeNullOrEmpty();
        }
    }
}
