using System.Diagnostics;
using System.Security.Cryptography;
using NanoidDotNet;
using SkiaSharp;
using Svg.Skia;
using TeleCasino.SlotsGameService.Models;
using TeleCasino.SlotsGameService.Services.Interface;

namespace TeleCasino.SlotsGameService.Services;

public class SlotsGameService : ISlotsGameService
{
    private readonly string _sharedDir;
    private readonly string _htmlDir;
    public const int FrameCount = 20;
    private const int Width = 600;
    private const int Height = 300;
    private static readonly string _framesSubDir = "frames";
    private static readonly string _videosSubDir = "videos";
    private static readonly string _imagesSubDir = "images";
    private static readonly string _soundsSubDir = "sounds";

    public SlotsGameService(IConfiguration config)
    {
        _sharedDir = config["SharedDirectory"] ?? "/shared";
        _htmlDir = config["HtmlDir"] ?? "/app/wwwroot";
    }

    public async Task<SlotsResult> PlayGameAsync(decimal wager, int gameSessionId)
    {
        var slotsResultId = await Nanoid.GenerateAsync();
        var slotsSharedRootPath = Path.Combine(_sharedDir, "Slots");
        var videoDir = Path.Combine(slotsSharedRootPath, slotsResultId, _videosSubDir);
        var videoFile = Path.Combine(videoDir, $"{slotsResultId}.mp4");
        var framesDir = Path.Combine(slotsSharedRootPath, slotsResultId, _framesSubDir);
        var imagesDir = Path.Combine(slotsSharedRootPath, _imagesSubDir);
        var soundsDir = Path.Combine(slotsSharedRootPath, _soundsSubDir);

        PrepareDirectory(framesDir);
        DeleteThisFile(videoFile);
        PrepareDirectory(videoDir);

        // load assets
        var assets = LoadBitmaps(imagesDir);
        var soundFile = Path.Combine(soundsDir, "Ij76x_px8jo.mp3");

        // spin and payout
        var (reels, payout) = Spin(wager);
        var netGain = payout - wager;

        // video generation
        await RenderAnimationAsync(assets, reels, framesDir);
        RunFfmpeg(framesDir, videoFile);

        // move video file to public access url
        if (File.Exists(videoFile))
            File.Move(videoFile, Path.Combine(_htmlDir, Path.GetFileName(videoFile)));

        // cleanup
        Directory.Delete(Path.Combine(slotsSharedRootPath, slotsResultId), true);

        return new SlotsResult
        {
            Id = slotsResultId,
            Wager = wager,
            Payout = payout,
            NetGain = netGain,
            VideoFile = Path.GetFileName(videoFile),
            Win = netGain > 0,
            Reels = reels.Select(r => r.Name).ToList(),
            ReelsEmoji = reels.Select(r => r.Emoji).ToList(),
            GameSessionId = gameSessionId
        };
    }

    private static void PrepareDirectory(string dir)
    {
        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);
    }

    private static void DeleteThisFile(string filePath)
    {
        if (File.Exists(filePath)) File.Delete(filePath);
    }

    private static Dictionary<SlotSymbol, SKBitmap> LoadBitmaps(string rootPath)
    {
        var dict = new Dictionary<SlotSymbol, SKBitmap>();
        foreach (var sym in SlotsProperties.AllSymbols)
        {
            var symImageFullPath = Path.Combine(rootPath, sym.ImagePath);
            if (!File.Exists(symImageFullPath))
                throw new FileNotFoundException($"Missing image for {sym.Name}", symImageFullPath);
            dict[sym] = SKBitmap.Decode(symImageFullPath)
                ?? throw new Exception($"Failed to decode {symImageFullPath}");
        }
        return dict;
    }

    // 🔐 Secure RNG helpers
    private static double SecureNextDouble()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        ulong value = BitConverter.ToUInt64(bytes);
        return value / (double)ulong.MaxValue;
    }

    private static int SecureNext(int minValue, int maxValue)
    {
        if (minValue >= maxValue)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be less than maxValue");

        var diff = maxValue - minValue;
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);
        int value = BitConverter.ToInt32(bytes) & int.MaxValue;
        return (value % diff) + minValue;
    }

    private static (SlotSymbol[] Reels, decimal Payout) Spin(decimal bet)
    {
        decimal modifier = bet switch
        {
            0.05m => 1.0m,
            0.10m => 1.0m,
            0.50m => 1.0m,
            1.0m  => 1.0m,
            2.0m  => 1.2m,
            5.0m  => 1.5m,
            _     => 1.0m
        };

        var weighted = SlotsProperties.AllSymbols
            .Select(s => (sym: s, w: (decimal)s.BaseWeight * (s.Rarity >= SlotsTierType.Rare ? modifier : 1.0m)))
            .ToList();

        SlotSymbol PickOne()
        {
            decimal total = weighted.Sum(x => x.w);
            decimal r = (decimal)SecureNextDouble() * total;
            foreach (var (sym, w) in weighted)
                if (r < w) return sym;
                else r -= w;
            return weighted.Last().sym;
        }

        var reels = new[] { PickOne(), PickOne(), PickOne() };

        decimal payout = 0;
        if (reels.All(r => r.Name == reels[0].Name))
        {
            payout = reels[0].Rarity switch
            {
                SlotsTierType.Jackpot  => bet * 100,
                SlotsTierType.Rare     => bet * 50,
                SlotsTierType.Uncommon => bet * 20,
                SlotsTierType.Common   => bet * 10,
                _                      => 0
            };
        }
        else if (reels.GroupBy(r => r.Name).Any(g => g.Count() == 2))
        {
            payout = bet * 2;
        }

        return (reels, payout);
    }

    private static Task RenderAnimationAsync(
        Dictionary<SlotSymbol, SKBitmap> assets,
        SlotSymbol[] finalReels,
        string framesDir)
    {
        return Task.Run(async () =>
        {
            for (int i = 0; i < FrameCount; i++)
            {
                var reels = i < FrameCount - 1
                    ? SlotsProperties.AllSymbols.OrderBy(_ => SecureNext(0, int.MaxValue)).Take(3).ToArray()
                    : finalReels;
                await DrawFrameAsync(assets, reels, i, framesDir);
            }
        });
    }

    private static async Task DrawFrameAsync(
        Dictionary<SlotSymbol, SKBitmap> assets,
        SlotSymbol[] reels,
        int idx,
        string dir)
    {
        using var bmp = new SKBitmap(Width, Height);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.DarkGreen);

        int slotW = Width / 3, slotH = Height;
        for (int i = 0; i < 3; i++)
        {
            var img = assets[reels[i]];
            float x = i * slotW + (slotW - img.Width) / 2;
            float y = (slotH - img.Height) / 2;
            canvas.DrawBitmap(img, x, y);
        }

        var path = Path.Combine(dir, $"frame_{idx:D3}.png");
        using var data = SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Png, 100);

        using var ms = new MemoryStream();
        data.SaveTo(ms);
        ms.Seek(0, SeekOrigin.Begin);

        using var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await ms.CopyToAsync(fs);
    }

    private static void RunFfmpeg(string framesDir, string outputVideo)
    {
        var ffArgs = $"-y -framerate 10 -i {framesDir}/frame_%03d.png " +
                     $"-i {soundFile} " +
                     "-shortest " +
                     "-r 30 " +
                     "-c:v libx264 -preset fast -pix_fmt yuv420p " +
                     "-c:a aac -b:a 128k " +
                     "-movflags +faststart " +
                     "-f mp4 " +
                     outputVideo;

        var ffmpegPath = Environment.GetEnvironmentVariable("FFMPEG_PATH") ?? "ffmpeg";
        var psi = new ProcessStartInfo(ffmpegPath, ffArgs)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false
        };

        using var proc = Process.Start(psi)!;
        proc.WaitForExit();
    }
}
