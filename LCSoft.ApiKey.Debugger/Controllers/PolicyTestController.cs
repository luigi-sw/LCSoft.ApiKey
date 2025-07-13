using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LCSoft.ApiKey.Debugger.Controllers;

[ApiController]
[Authorize]
public class PolicyTestController : ControllerBase
{
    [HttpGet("PolicyAuthorization")]
    public IActionResult PolicyAuthorization()
    {
        return Ok("Api Key is valid and working!");
    }
}
