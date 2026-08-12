using Microsoft.AspNetCore.Mvc;
using Trading.DTO.Models;
using Trading.Service.Contracts;

namespace Trading.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ITradingProcessService _tradingProcessService;

        public OrdersController(ITradingProcessService tradingProcessService)
        {
            _tradingProcessService = tradingProcessService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTrade([FromBody] TradeDTO trade)
        {
            var result = await _tradingProcessService.ProcessManualOrderAsync(trade, default);
            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetTradeOrders([FromQuery] TradeFilterDTO filter)
        {
            var res = await _tradingProcessService.GetTradeOrdersAsync(filter, default);
            return Ok(res);
        }

        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetOrdersBySymbol(string symbol)
        {
            var res = await _tradingProcessService.GetTradeOrdersAsync(new TradeFilterDTO(symbol, null, null, null), default);
            return Ok(res);
        }
    }
}
