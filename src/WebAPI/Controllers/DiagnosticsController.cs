using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Contracts.Services;

namespace OnlineShop.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly ISmsService _smsService;

    public DiagnosticsController(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public record SmsIrVerifyRequest(string mobile, string code);

    [HttpPost("smsir/verify")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SmsIrVerify([FromBody] SmsIrVerifyRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.mobile) || string.IsNullOrWhiteSpace(request.code))
            return BadRequest(new { isSuccess = false, message = "mobile and code are required" });

        var success = await _smsService.SendOtpAsync(request.mobile, request.code, ct);
        if (success)
            return Ok(new { isSuccess = true, message = "SmsIr Verify sent (sandbox)" });

        return StatusCode(500, new { isSuccess = false, message = "SmsIr Verify failed" });
    }
}


