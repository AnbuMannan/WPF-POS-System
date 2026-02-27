using Microsoft.AspNetCore.Mvc;
using POS.LicenseServer.Services;

namespace POS.LicenseServer.Controllers
{
    [ApiController]
    [Route("api/license")]
    public class LicenseController : ControllerBase
    {
        private readonly LicenseService _service;

        public LicenseController(LicenseService service)
        {
            _service = service;
        }

        [HttpPost("activate")]
        public IActionResult Activate(ActivateRequest req)
        {
            var result = _service.ActivateSigned(req.LicenseKey, req.MachineId);

            if (!result.success)
                return BadRequest(result.message);

            return Ok(new
            {
                LicenseKey = req.LicenseKey,
                ExpiryDate = result.expiryDate,
                Payload = result.payload,
                Signature = Convert.ToBase64String(result.signature)
            });
        }


        public class ActivateRequest
        {
            public string LicenseKey { get; set; }
            public string MachineId { get; set; }
        }
    }

}
