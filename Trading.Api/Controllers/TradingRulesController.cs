using Microsoft.AspNetCore.Mvc;
using Trading.Core.Models;
using Trading.Service.Contracts;

namespace Trading.Api.Controllers
{
    [ApiController]
    [Route("api/trading-rules")]
    public class TradingRulesController : ControllerBase
    {
        private readonly ITradingRulesService _rulesService;

        public TradingRulesController(ITradingRulesService rulesService)
        {
            _rulesService = rulesService;
        }

        [HttpGet]
        public IActionResult GetRules()
        {
            return Ok(_rulesService.GetRules());
        }

        [HttpPut]
        public IActionResult UpdateRules([FromBody] TradingRulesConfigurations rules)
        {
            _rulesService.UpdateRules(rules);
            return Ok(new { Message = "Trading rules updated successfully at runtime.", CurrentRules = _rulesService.GetRules() });
        }
    }
}
