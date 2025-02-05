using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KanbamApi.Controllers;

[ApiController]
[Route("api/")]
[EnableRateLimiting("FixedWindow")]
public class DocController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(
            new Dictionary<string, string>()
            {
                { "Title", "Doc" },
                { "Description", "Hi, welcome to Kanbam!" }
            }
        );
    }
}
