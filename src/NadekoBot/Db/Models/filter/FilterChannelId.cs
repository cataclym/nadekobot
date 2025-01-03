#nullable disable
namespace NadekoBot.Db.Models;

public class FilterChannelId : DbEntity
{
    public ulong ChannelId { get; set; }

    public bool Equals(FilterChannelId other)
        => ChannelId == other.ChannelId;

    public override bool Equals(object obj)
        => obj is FilterChannelId fci && Equals(fci);

    public override int GetHashCode()
        => ChannelId.GetHashCode();
}
