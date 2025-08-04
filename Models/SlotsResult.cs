namespace TeleCasino.SlotsGameService.Models;

public class SlotsResult
{
    public required string Id { get; set; }
    public decimal Wager { get; set; }
    public decimal Payout { get; set; }
    public decimal NetGain { get; set; }
    public required string VideoFile { get; set; }
    public bool Win { get; set; }

    // game mechanics
    public required List<string> Reels { get; set; }
    public required List<string> ReelsEmoji { get; set; }
    public int GameSessionId { get; set; }
}