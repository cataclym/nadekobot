using Discord;
using Discord.Commands;
using NadekoBot.Common;
using NadekoBot.Common.Attributes;
using NadekoBot.Extensions;
using System;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Utility
{
    public partial class Utility
    {
        public class BotConfigCommands : NadekoSubmodule
        {
            [NadekoCommand, Usage, Description, Aliases]
            [OwnerOnly]
            public async Task BotConfigEdit()
            {
                var names = Enum.GetNames(typeof(BotConfigEditType));
                var data = Bc.GetValues();
                var values = "";
                foreach(var name in names)
                {
                    data.TryGetValue(name, out var value);
                    if(value?.Length > 30 && name != "CurrencySign")
                        value = value.Substring(0, 30) + "...";
                    values += $"{value.Replace('\n',' ')}\n";
                }
                
                var embed = new EmbedBuilder();
                embed.WithTitle("Bot Config");
                embed.WithOkColor();
                embed.AddField(fb => fb.WithName("Names").WithValue(string.Join("\n", names)).WithIsInline(true));
                embed.AddField(fb => fb.WithName("Values").WithValue(values).WithIsInline(true));
                await ctx.Channel.EmbedAsync(embed: embed).ConfigureAwait(false);
            }

            [NadekoCommand, Usage, Description, Aliases]
            [OwnerOnly]
            public async Task BotConfigEdit(BotConfigEditType type, [Leftover]string newValue = null)
            {
                if (string.IsNullOrWhiteSpace(newValue))
                    newValue = null;

                var success = Bc.Edit(type, newValue);

                if (!success)
                    await ReplyErrorLocalizedAsync("bot_config_edit_fail", Format.Bold(type.ToString()), Format.Bold(newValue ?? "NULL")).ConfigureAwait(false);
                else
                    await ReplyConfirmLocalizedAsync("bot_config_edit_success", Format.Bold(type.ToString()), Format.Bold(newValue ?? "NULL")).ConfigureAwait(false);
            }
        }
    }
}
