using System.IO;
using Newtonsoft.Json;

namespace NadekoBot.Core.Modules.Gambling.Common;

public sealed class Conf
{
    public static string PATH = "data/conf.json";

    public static Conf GetConfig()
        => JsonConvert.DeserializeObject<Conf>(File.ReadAllText(PATH));
    
    [JsonProperty("shop_refund_percent")]
    public float ShopRefundPercent { get; set; } = 0.5f;
    
}