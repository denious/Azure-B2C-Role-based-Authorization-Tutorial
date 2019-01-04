using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return NoContent();
        }

        [HttpGet("Extra")]
        [Authorize(Roles = "Extra")]
        public IActionResult GetExtra()
        {
            return NoContent();
        }

        [HttpGet("Missing")]
        [Authorize(Roles = "Missing")]
        public IActionResult GetMissing()
        {
            return NoContent();
        }
    }
}
