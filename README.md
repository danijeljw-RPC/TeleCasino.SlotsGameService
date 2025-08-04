# TeleCasino Slots Game

[![Build and Test SlotsGameService](https://github.com/repasscloud/TeleCasino.SlotsGameService/actions/workflows/test-slots-api.yml/badge.svg)](https://github.com/repasscloud/TeleCasino.SlotsGameService/actions/workflows/test-slots-api.yml)
[![🚀 Publish TeleCasino.KenoGameService (linux-x64)](https://github.com/repasscloud/TeleCasino.SlotsGameService/actions/workflows/docker-image.yml/badge.svg)](https://github.com/repasscloud/TeleCasino.SlotsGameService/actions/workflows/docker-image.yml)
![GitHub tag (latest SemVer)](https://img.shields.io/github/v/tag/repasscloud/TeleCasino.SlotsGameService?label=version)

A command-line Slots game animation and result generator built with .NET.  
Players place a wager and receive an animated video of the slot machine spin, plus a JSON summary of the result.

## Features

- **Animated spin**:
  1. **Spinning reels**: Shows 3 reels spinning with randomized symbols
  2. **Final reveal**: Stops on the final winning (or losing) combination
  3. **Video output**: Generates an MP4 animation of the entire spin

- **JSON output**: Detailed result including wager, reels outcome, payout, net gain, video file.

- **Cryptographic RNG**: All spins use secure cryptographic randomness for fair results.

## Installation

1. Ensure [.NET 9.0 SDK](https://dotnet.microsoft.com/download) is installed.  
2. Clone or download this repository.  
3. Add dependencies:

   ```bash
   dotnet add package SkiaSharp
   dotnet add package Svg.Skia
   dotnet add package NanoidDotNet
   ```

4. Place your symbol images (`cherry.png`, `bar.png`, `seven.png`, etc.) in the `images/` directory.

## Build & Publish

```bash
# Clean and build
rm -rf bin obj
dotnet clean
dotnet restore
dotnet publish -c Release

# The single-file, self-contained binary will be in:
#   bin/Release/net9.0/<RID>/publish/TeleCasino.SlotsGame
```

## Usage

```bash
TeleCasino.SlotsGame <Wager> <GameSessionId> [--json]
```

- `<Wager>`: Must be 0.05, 0.10, 0.50, 1.0, 2.0, or 5.0  
- `<GameSessionId>`: Numeric identifier for the current game session.  
- `--json` (optional): Print the JSON summary to console.

### Example

```bash
TeleCasino.SlotsGame 1.0 221 --json
```

- Generates `abc123.mp4` (video) and `abc123.json`:

```json
{
  "Id": "abc123",
  "Wager": 1.0,
  "Payout": 2.0,
  "NetGain": 1.0,
  "VideoFile": "abc123.mp4",
  "Win": true,
  "Reels": ["Cherry", "Cherry", "Cherry"],
  "ReelsEmoji": ["🍒","🍒","🍒"],
  "GameSessionId": 221
}
```

## Rules & Parameters

- **3-reel slot machine** with weighted symbol rarity.  
- **Two of a kind** = 2× payout.  
- **Three of a kind** payouts depend on rarity:  
  - Common → 10× wager  
  - Uncommon → 20× wager  
  - Rare → 50× wager  
  - Jackpot → 100× wager  
- **No matching symbols** = loss.  

## License

This project is released under the MIT License.
