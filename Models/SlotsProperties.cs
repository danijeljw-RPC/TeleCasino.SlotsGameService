namespace TeleCasino.SlotsGameService.Models;

public static class SlotsProperties
{
    public static readonly List<SlotSymbol> AllSymbols = new()
    {
        new("Cherry",     "🍒", "cherries.png",   SlotsTierType.Common,   50),
        new("Lemon",      "🍋", "lemon.png",      SlotsTierType.Common,   50),
        new("Coin",       "🪙", "coin.png",       SlotsTierType.Common,   40),
        new("Dice",       "🎲", "game_die.png",   SlotsTierType.Common,   40),
        new("Bell",       "🔔", "bell.png",       SlotsTierType.Uncommon, 30),
        new("Star",       "⭐", "star.png",       SlotsTierType.Uncommon, 30),
        new("Watermelon", "🍉", "watermelon.png", SlotsTierType.Uncommon, 20),
        new("Gem",        "💎", "gem.png",        SlotsTierType.Uncommon, 15),
        new("Crown",      "👑", "crown.png",      SlotsTierType.Rare,     10),
        new("MoneyBag",   "💰", "moneybag.png",   SlotsTierType.Rare,      5),
        new("Fire",       "🔥", "fire.png",       SlotsTierType.Rare,      3),
        new("7",          "7️⃣", "seven.png",      SlotsTierType.Jackpot,   1),
    };
}