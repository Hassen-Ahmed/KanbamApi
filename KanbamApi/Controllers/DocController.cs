using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;

[ApiController]
[Route("api/")]
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
