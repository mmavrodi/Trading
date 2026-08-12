using FluentAssertions;
using Trading.Core.Models;
using Trading.DTO.Models;

namespace Trading.Cache.Tests
{
    public class PriceCacheTests
    {
        private readonly IPriceCache _mockedPriceCacheService;

        public PriceCacheTests()
        {
            _mockedPriceCacheService = new PriceCache();
        }

        [Fact]
        public void UpdateAndGetPrevious_ShouldCalculateDerivedValuesCorrectly()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var update = new PriceUpdateDTO("AAAA", 100m, 102m, now);

            (SymbolPriceState?, SymbolPriceState?) expected = (null, new SymbolPriceState()
            {
                Symbol = "AAAA",
                BidPrice = 0,
                AskPrice = 0,
                CurrentMarketPrice = 101m,
                Spread = 2m,
                SpreadPercent = 1.98m,
                Timestamp = now
            });

            // Act
            var (previous, current) = _mockedPriceCacheService.UpdateAndGetPrevious(update);

            // Assert
            previous.Should().BeNull();
            current.Should().NotBeNull();
            current.CurrentMarketPrice.Should().Be(expected.Item2.CurrentMarketPrice);
            current.Spread.Should().Be(expected.Item2.Spread);
            Math.Round(current.SpreadPercent, 2, MidpointRounding.AwayFromZero).Should().Be(expected.Item2.SpreadPercent);
        }

        [Fact]
        public void UpdateAndGetPrevious_ShouldThrowException_WhenBidIsGreaterThanAsk()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var invalidUpdate = new PriceUpdateDTO("AAAA", 105m, 100m, now);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>  _mockedPriceCacheService.UpdateAndGetPrevious(invalidUpdate));
        }

        [Fact]
        public void UpdateAndGetPrevious_ShouldReturnCorrectPreviousState()
        {
            // Arrange
            var firstUpdate = new PriceUpdateDTO("AAAA", 100m, 101m, DateTime.UtcNow);
            var secondUpdate = new PriceUpdateDTO("AAAA", 102m, 103m, DateTime.UtcNow);

            // Act
            _mockedPriceCacheService.UpdateAndGetPrevious(firstUpdate);
            var (previous, current) = _mockedPriceCacheService.UpdateAndGetPrevious(secondUpdate);

            // Assert
            Assert.NotNull(previous);
            previous.CurrentMarketPrice.Should().Be(100.5m);
            current.CurrentMarketPrice.Should().Be(102.5m);
        }
    }
}
