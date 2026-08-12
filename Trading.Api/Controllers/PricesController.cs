using Microsoft.AspNetCore.Mvc;
using Trading.Cache;

namespace Trading.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricesController : ControllerBase
    {
        private readonly IPriceCache _priceCache;

        public PricesController(IPriceCache priceCache)
        {
            _priceCache = priceCache;
        }

        [HttpGet("{symbol}")]
        public IActionResult GetLatestPrice(string symbol)
        {
            var price = _priceCache.GetLatest(symbol);
            if (price == null) return NotFound($"No price state available for symbol '{symbol}'.");
            return Ok(price);
        }
    }
}
