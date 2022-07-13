#nullable disable
using Nadeko.Common;
using NadekoBot.Modules.Gambling.Common;
using NadekoBot.Modules.Gambling.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace NadekoBot.Modules.Gambling;

public partial class Gambling
{
    [Group]
    public partial class FlipCoinCommands : GamblingSubmodule<IGamblingService>
    {
        public enum BetFlipGuess : byte
        {
            H = 0,
            Head = 0,
            Heads = 0,
            T = 1,
            Tail = 1,
            Tails = 1
        }

        private static readonly NadekoRandom _rng = new();
        private readonly IImageCache _images;
        private readonly ICurrencyService _cs;
        private readonly ImagesConfig _ic;

        public FlipCoinCommands(
            IImageCache images,
            ImagesConfig ic,
            ICurrencyService cs,
            GamblingConfigService gss)
            : base(gss)
        {
            _ic = ic;
            _images = images;
            _cs = cs;
        }

        [Cmd]
        public async partial Task Flip(int count = 1)
        {
            if (count is > 10 or < 1)
            {
                await ReplyErrorLocalizedAsync(strs.flip_invalid(10));
                return;
            }

            var headCount = 0;
            var tailCount = 0;
            var imgs = new Image<Rgba32>[count];
            var headsArr = await _images.GetHeadsImageAsync();
            var tailsArr = await _images.GetTailsImageAsync();

            var result = await _service.FlipAsync(count);
            
            for (var i = 0; i < result.Length; i++)
            {
                if (result[i].Side == 0)
                {
                    imgs[i] = Image.Load(headsArr);
                    headCount++;
                }
                else
                {
                    imgs[i] = Image.Load(tailsArr);
                    tailCount++;
                }
            }

            using var img = imgs.Merge(out var format);
            await using var stream = await img.ToStreamAsync(format);
            foreach (var i in imgs)
                i.Dispose();

            var msg = count != 1
                ? Format.Bold(ctx.User.ToString())
                  + " "
                  + GetText(strs.flip_results(count, headCount, tailCount))
                : Format.Bold(ctx.User.ToString())
                  + " "
                  + GetText(strs.flipped(headCount > 0
                      ? Format.Bold(GetText(strs.heads))
                      : Format.Bold(GetText(strs.tails))));

            await ctx.Channel.SendFileAsync(stream, $"{count} coins.{format.FileExtensions.First()}", msg);
        }

        [Cmd]
        public async partial Task Betflip(ShmartNumber amount, BetFlipGuess guess)
        {
            if (!await CheckBetMandatory(amount) || amount == 1)
                return;

            var res = await _service.BetFlipAsync(ctx.User.Id, amount, (byte)guess);
            if (!res.TryPickT0(out var result, out _))
            {
                await ReplyErrorLocalizedAsync(strs.not_enough(CurrencySign));
                return;
            }

            Uri imageToSend;
            var coins = _ic.Data.Coins;
            if (result.Side == 0)
            {
                imageToSend = coins.Heads[_rng.Next(0, coins.Heads.Length)];
            }
            else
            {
                imageToSend = coins.Tails[_rng.Next(0, coins.Tails.Length)];
            }

            string str;
            var won = (long)result.Won;
            if (won > 0)
            {
                str = Format.Bold(GetText(strs.flip_guess(N(won))));
            }
            else
            {
                str = Format.Bold(GetText(strs.better_luck));
            }

            await ctx.Channel.EmbedAsync(_eb.Create()
                .WithAuthor(ctx.User)
                                            .WithDescription(str)
                                            .WithOkColor()
                                            .WithImageUrl(imageToSend.ToString()));
        }
    }
}