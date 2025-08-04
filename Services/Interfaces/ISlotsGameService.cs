using TeleCasino.SlotsGameService.Models;

namespace TeleCasino.SlotsGameService.Services.Interface;

public interface ISlotsGameService
{
    Task<SlotsResult> PlayGameAsync(decimal wager, int gameSessionId);
}