using Microsoft.AspNetCore.Mvc;
using TransactionApi.Services;

namespace TransactionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilsController : ControllerBase
    {
        private readonly SignatureService _sigService;

        public UtilsController(SignatureService sigService)
        {
            _sigService = sigService;
        }

        [HttpGet("GetTimestamp")]
        public IActionResult GetTimestamp()
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Ok(new { timestamp = ts });
        }

        [HttpGet("GetSignature")]
        public IActionResult GetSignature(string partnerKey, string partnerRefNo, string partnerPassword, decimal totalAmount)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var sig = _sigService.GenerateSignature(partnerKey, partnerRefNo, partnerPassword, ts, totalAmount.ToString());
            return Ok(new { timestamp = ts, signature = sig });
        }
    }
}
