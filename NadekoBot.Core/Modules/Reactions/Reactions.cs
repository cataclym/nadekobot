using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NadekoBot.Common.Attributes;
using NadekoBot.Core.Services;
using NadekoBot.Extensions;
using NadekoBot.Modules.Reactions.Common;
using Newtonsoft.Json;

namespace NadekoBot.Modules.Reactions
{
    public partial class Reactions : NadekoModule
    {
        private readonly IHttpClientFactory _httpFactory;

        public Reactions(IHttpClientFactory factory)
        {
            _httpFactory = factory;
        }

        [NadekoCommand, Usage, Description, Aliases]
        public async Task hug([Leftover] string text = null)
        {
            EmbedBuilder emb = new EmbedBuilder()
                .WithOkColor();
           
            await ctx.Channel.TriggerTypingAsync().ConfigureAwait(false);

            using (var http = _httpFactory.CreateClient())
            {
                var img = await http.GetAsync("https://waifu.pics/api/sfw/hug").ConfigureAwait(false);
                var content = await img.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<WaifuData>(content);
                if (data != null) emb.WithImageUrl(data.URL);
            }
            
            if (!string.IsNullOrWhiteSpace(text))
            {
                emb.WithDescription($"{ctx.User.Mention} hugged {text}");
            }

            await ctx.Channel.EmbedAsync(emb);
        }
        
        [NadekoCommand, Usage, Description, Aliases]
        public async Task pat([Leftover] string text = null)
        {
            EmbedBuilder emb = new EmbedBuilder()
                .WithOkColor();
           
            await ctx.Channel.TriggerTypingAsync().ConfigureAwait(false);

            using (var http = _httpFactory.CreateClient())
            {
                var img = await http.GetAsync("https://waifu.pics/api/sfw/pat").ConfigureAwait(false);
                var content = await img.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<WaifuData>(content);
                if (data != null) emb.WithImageUrl(data.URL);
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                emb.WithDescription($"{ctx.User.Mention} patted {text}");
            }

            await ctx.Channel.EmbedAsync(emb);
        }
        
        [NadekoCommand, Usage, Description, Aliases]
        public async Task kiss([Leftover] string text = null)
        {
            EmbedBuilder emb = new EmbedBuilder()
                .WithOkColor();
           
            await ctx.Channel.TriggerTypingAsync().ConfigureAwait(false);

            using (var http = _httpFactory.CreateClient())
            {
                var img = await http.GetAsync("https://waifu.pics/api/sfw/kiss").ConfigureAwait(false);
                var content = await img.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<WaifuData>(content);
                if (data != null) emb.WithImageUrl(data.URL);
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                emb.WithDescription($"{ctx.User.Mention} kissed {text}");
            }

            await ctx.Channel.EmbedAsync(emb);
        }
    }
}