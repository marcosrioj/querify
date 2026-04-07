using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Common.Infrastructure.Swagger.Controller;

[ApiController]
[Route("version")]
public class VersionController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        try
        {
            var json = System.IO.File.ReadAllText("build_info.json");
            var jsonObject = JsonDocument.Parse(json).RootElement;
            return Ok(jsonObject);
        }
        catch (Exception)
        {
            return NotFound("No build version set");
        }
    }
}
