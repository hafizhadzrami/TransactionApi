using log4net;
using Microsoft.AspNetCore.Mvc;
using TransactionApi.Services;

namespace TransactionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmitTrxController : ControllerBase
    {
        private readonly SignatureService _sigService;
        private readonly PartnerAuthService _authService;
        private readonly DiscountService _discountService;
        private readonly ILog _log;

        public SubmitTrxController(SignatureService sigService, PartnerAuthService authService, DiscountService discountService)
        {
            _sigService = sigService;
            _authService = authService;
            _discountService = discountService;
            _log = LogManager.GetLogger(typeof(SubmitTrxController));
        }

        [HttpPost("SubmitTrxMessage")]
        public IActionResult SubmitTrxMessage([FromBody] SubmitTrxRequest req)
        {
            _log.Info($"📥 Incoming trx request: {req.PartnerRefNo}");

            // Validate partner
            if (!_authService.ValidatePartner(req.PartnerKey, req.PartnerPassword))
            {
                _log.Warn("❌ Invalid partner credentials");
                return Unauthorized(new { message = "Invalid partner credentials" });
            }

            // Expiry check (±5 min)
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (Math.Abs(now - req.Timestamp) > 300)
            {
                _log.Warn("⏰ Expired timestamp");
                return BadRequest(new { message = "Timestamp expired" });
            }

            // Signature check
            var expectedSig = _sigService.GenerateSignature(
                req.PartnerKey,
                req.PartnerRefNo,
                req.PartnerPassword,
                req.Timestamp,
                req.TotalAmount.ToString()
            );

            if (req.Sig != expectedSig)
            {
                _log.Warn("⚠️ Invalid signature");
                return BadRequest(new { message = "Invalid signature" });
            }

            // ✅ Calculate discount
            var (discount, finalAmount) = _discountService.CalculateDiscount(req.TotalAmount);

            var response = new SubmitTrxResponse
            {
                Result = 1,
                TotalAmount = req.TotalAmount,
                TotalDiscount = discount,
                FinalAmount = finalAmount,
                ResultMessage = "✅ Transaction received successfully",
                Timestamp = now,
                Signature = _sigService.GenerateSignature(req.PartnerKey, req.PartnerRefNo, req.PartnerPassword, now, req.TotalAmount.ToString())
            };

            _log.Info($"📤 Outgoing Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
            return Ok(response);
        }
    }

    public class SubmitTrxRequest
    {
        public string PartnerKey { get; set; }
        public string PartnerRefNo { get; set; }
        public string PartnerPassword { get; set; }
        public long TotalAmount { get; set; } // in cents
        public List<Item> Items { get; set; }
        public long Timestamp { get; set; }
        public string Sig { get; set; }
    }

    public class Item
    {
        public string PartnerItemRef { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        public long UnitPrice { get; set; }
    }

    public class SubmitTrxResponse
    {
        public int Result { get; set; }
        public long TotalAmount { get; set; }
        public long TotalDiscount { get; set; }
        public long FinalAmount { get; set; }
        public string ResultMessage { get; set; }
        public long Timestamp { get; set; }
        public string Signature { get; set; }
    }
}
