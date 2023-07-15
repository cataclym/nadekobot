#nullable disable
namespace NadekoBot.Services.Database.Models;


// todo db required, nullable?
public class AntiRaidSetting : DbEntity
{
    public int GuildConfigId { get; set; }
    public GuildConfig GuildConfig { get; set; }

    public int UserThreshold { get; set; }
    public int Seconds { get; set; }
    public PunishmentAction Action { get; set; }

    /// <summary>
    ///     Duration of the punishment, in minutes. This works only for supported Actions, like:
    ///     Mute, Chatmute, Voicemute, etc...
    /// </summary>
    public int PunishDuration { get; set; }
}