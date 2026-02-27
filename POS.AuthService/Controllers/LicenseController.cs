using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/local-license")]
public class LocalLicenseController : ControllerBase
{
    private readonly LicenseActivationService _activation;

    public LocalLicenseController(LicenseActivationService activation)
    {
        _activation = activation;
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate(ActivateLocalRequest req)
    {
        var result = await _activation.ActivateOnlineAsync(req.LicenseKey);

        if (!result.success)
            return BadRequest(result.message);

        return Ok(new { result.message, result.store });
    }
}

public class ActivateLocalRequest
{
    public string LicenseKey { get; set; } = string.Empty;
}
