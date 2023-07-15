﻿namespace NadekoBot.Services.Database.Models;

public class AntiSpamSetting : DbEntity
{
    public int GuildConfigId { get; set; }
    public GuildConfig GuildConfig { get; set; }

    public PunishmentAction Action { get; set; }
    public int MessageThreshold { get; set; } = 3;
    public int MuteTime { get; set; }
    public ulong? RoleId { get; set; }
    public HashSet<AntiSpamIgnore> IgnoredChannels { get; set; } = new();
}