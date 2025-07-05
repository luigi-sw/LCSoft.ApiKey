using LCSoft.ApiKey.Attribute;
using Microsoft.AspNetCore.Mvc;

namespace LCSoft.ApiKey.Debugger.Controllers;

[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("test")]
    [CustomApiKey]
    public IActionResult GetTest()
    {
        return Ok("Api Key is valid and working!");
    }
}
