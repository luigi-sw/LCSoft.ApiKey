using LCSoft.ApiKey.Attribute;
using Microsoft.AspNetCore.Mvc;

namespace LCSoft.ApiKey.Debugger.Controllers;

[ApiController]
[CustomAuthorization(AuthorizationHeader = "X-Auth", ApiKeyHeader = "X-API-Key")]
public class TestController : ControllerBase
{
    [HttpGet("test")]
    [CustomApiKey]
    [ApiKey]
    public IActionResult GetTest()
    {
        return Ok("Api Key is valid and working!");
    }
}
