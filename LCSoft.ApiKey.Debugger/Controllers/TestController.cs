using LCSoft.ApiKey.Attribute;
using Microsoft.AspNetCore.Mvc;

namespace LCSoft.ApiKey.Debugger.Controllers;

[ApiController]
//[CustomAuthorization(AuthorizationHeader = "X-Auth", ApiKeyHeader = "X-API-Key")]
public class TestController : ControllerBase
{
    [HttpGet("CustomAuthorization")]
    public IActionResult GetCustomAuthorization()
    {
        return Ok("Api Key is valid and working!");
    }

    [HttpGet("CustomApiKey")]
    [CustomApiKey]
    public IActionResult GetCustomApiKey()
    {
        return Ok("Api Key is valid and working!");
    }

    [HttpGet("ApiKey")]
    [ApiKey]
    public IActionResult GetApiKey()
    {
        return Ok("Api Key is valid and working!");
    }
}
