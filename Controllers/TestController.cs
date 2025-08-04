using Microsoft.AspNetCore.Mvc;

namespace TeleCasino.SlotsGameService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "pong" });
        }
    }
}
