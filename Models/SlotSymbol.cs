namespace TeleCasino.SlotsGameService.Models;

public class SlotSymbol
{
    public string Name { get; }
    public string Emoji { get; }
    public string ImagePath { get; }
    public SlotsTierType Rarity { get; }
    public double BaseWeight { get; }

    public SlotSymbol(string name, string emoji, string imagePath, SlotsTierType rarity, double baseWeight) =>
        (Name, Emoji, ImagePath, Rarity, BaseWeight) = (name, emoji, imagePath, rarity, baseWeight);
}
