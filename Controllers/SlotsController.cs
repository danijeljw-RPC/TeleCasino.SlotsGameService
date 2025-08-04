using Microsoft.AspNetCore.Mvc;
using TeleCasino.SlotsGameService.Models;
using TeleCasino.SlotsGameService.Services.Interface;

namespace TeleCasino.SlotsGameService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlotsController : ControllerBase
    {
        private readonly ISlotsGameService _slotsGameService;

        public SlotsController(ISlotsGameService slotsGameService)
        {
            _slotsGameService = slotsGameService;
        }

        /// <summary>
        /// Plays a Slots game and returns the result with a generated video file path.
        /// </summary>
        /// <param name="wager">Amount wagered (allowed values: 0.05, 0.10, 0.50, 1.0, 2.0, 5.0).</param>
        /// <param name="gameSessionId">Game session identifier.</param>
        [HttpPost("play")]
        [ProducesResponseType(typeof(SlotsResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PlayGame(
            [FromQuery] decimal wager,
            [FromQuery] int gameSessionId)
        {
            if (wager <= 0)
                return BadRequest("Wager must be greater than zero.");

            var allowedWagers = new[] { 0.05m, 0.10m, 0.50m, 1.0m, 2.0m, 5.0m };
            if (!allowedWagers.Contains(wager))
                return BadRequest("Invalid wager amount. Allowed: 0.05, 0.10, 0.50, 1.0, 2.0, 5.0");

            var result = await _slotsGameService.PlayGameAsync(wager, gameSessionId);

            return Ok(result);
        }
    }
}
