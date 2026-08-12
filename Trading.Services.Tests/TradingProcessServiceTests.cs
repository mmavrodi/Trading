using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Trading.Cache;
using Trading.Core.Models;
using Trading.DataAccess;
using Trading.DTO.Models;
using Trading.DTO.Models.Enums;
using Trading.Repository.Contracts;
using Trading.Service.Contracts;

namespace Trading.Services.Tests
{
    public class TradingProcessServiceTests
    {
        private readonly ITradingRulesService _rulesServiceMock;
        private readonly IPriceCache _priceCacheMock;
        private readonly ITradingRulesRepository _rulesRepoMock;
        private readonly TradingDbContext _dbContext;
        private readonly TradingProcessService _processService;

        public TradingProcessServiceTests()
        {
            _rulesServiceMock = A.Fake<ITradingRulesService>();
            _priceCacheMock = A.Fake<IPriceCache>();
            _rulesRepoMock = A.Fake<ITradingRulesRepository>();

            var options = new DbContextOptionsBuilder<TradingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new TradingDbContext(options);

            A.CallTo(() => _rulesRepoMock.GetRules()).Returns(new TradingRulesConfigurations
            {
                AutoTradeSpreadThresholdPercent = 0.01m
            });

            // Винаги приемаме ордера за целите на теста
            A.CallTo(() => _rulesServiceMock.ValidateOrder(A<TradeOrder>._, A<SymbolPriceState>._))
                           .Returns(new ValidationResultDTO(true, null));

            _processService = new TradingProcessService(_rulesServiceMock, _priceCacheMock, _rulesRepoMock, _dbContext);
        }

        [Fact]
        public async Task EvaluateAutoTrade_ShouldCreateSellOrder_WhenPriceGoesUp()
        {
            // Arrange
            var previous = new SymbolPriceState { Symbol = "AAAA", CurrentMarketPrice = 100m };
            var current = new SymbolPriceState
            {
                Symbol = "AAAA",
                CurrentMarketPrice = 110m,
                AskPrice = 111m,
                BidPrice = 109m,
                SpreadPercent = 1.0m
            };

            // Act
            await _processService.EvaluateAndExecuteAutoTradeAsync(previous, current, default);

            // Assert
            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Source == OrderSource.Auto.ToString());
            order.Should().NotBeNull();
            order.Side.Should().Be(OrderSide.Sell.ToString());
            order.Price.Should().Be(110.9667m);
        }

        [Fact]
        public async Task EvaluateAutoTrade_ShouldCreateBuyOrder_WhenPriceGoesDown()
        {
            // Arrange
            var previous = new SymbolPriceState { Symbol = "AAAA", CurrentMarketPrice = 100m };
            var current = new SymbolPriceState
            {
                Symbol = "AAAA",
                CurrentMarketPrice = 90m,
                AskPrice = 91m,
                BidPrice = 89m,
                SpreadPercent = 1.0m
            };

            // Act
            await _processService.EvaluateAndExecuteAutoTradeAsync(previous, current, default);

            // Assert
            var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Source == OrderSource.Auto.ToString());
            order.Should().NotBeNull();
            order.Side.Should().Be(OrderSide.Buy.ToString());
            order.Price.Should().Be(89.0267m);
        }

        [Fact]
        public async Task EvaluateAutoTrade_ShouldNotCreateOrder_WhenSpreadIsTooLow()
        {
            // Arrange
            A.CallTo(() => _rulesRepoMock.GetRules()).Returns(new TradingRulesConfigurations
            {
                AutoTradeSpreadThresholdPercent = 0.01m
            });

            var previous = new SymbolPriceState { Symbol = "AAAA", CurrentMarketPrice = 100m, AskPrice = 1.5m, BidPrice = 1m };
            var current = new SymbolPriceState { Symbol = "AAAA", CurrentMarketPrice = 105m, SpreadPercent = 0.005m, AskPrice = 1.5m, BidPrice = 1m };

            // Act
            await _processService.EvaluateAndExecuteAutoTradeAsync(previous, current, default);

            // Assert
            var ordersCount = await _dbContext.Orders.CountAsync();
            ordersCount.Should().Be(0);
        }
    }
}
