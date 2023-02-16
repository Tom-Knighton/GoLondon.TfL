using Microsoft.AspNetCore.Mvc;

namespace GoLondon.TfL.Controllers;

[ApiController]
[Route("")]
public class TflController : ControllerBase
{
    public TflController()
    {
        
    }

    [HttpGet("HelloWorld")]
    public async Task<IActionResult> HelloWorld()
    {
        return Ok("Hello world");
    }
}