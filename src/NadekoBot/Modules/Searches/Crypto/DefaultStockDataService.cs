using CsvHelper;
using CsvHelper.Configuration;
using Google.Protobuf.WellKnownTypes;
using NadekoBot.Services.Database.Models;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NadekoBot.Modules.Searches;

public sealed class DefaultStockDataService : IStockDataService, INService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DefaultStockDataService(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    public async Task<StockData?> GetStockDataAsync(string query)
    {
        try
        {
            if (!query.IsAlphaNumeric())
                return default;

            using var http = _httpClientFactory.CreateClient();
            var data = await http.GetFromJsonAsync<YahooQueryModel>(
                $"https://query2.finance.yahoo.com/v7/finance/quote?symbols={query}");

            if (data is null)
                return default;

            var symbol = data.QuoteResponse.Result.FirstOrDefault();

            if (symbol is null)
                return default;

            return new()
            {
                Name = symbol.LongName,
                Symbol = symbol.Symbol,
                Price = symbol.RegularMarketPrice,
                Close = symbol.RegularMarketPreviousClose,
                MarketCap = symbol.MarketCap,
                Change50d = symbol.FiftyDayAverageChangePercent,
                Change200d = symbol.TwoHundredDayAverageChangePercent,
                DailyVolume = symbol.AverageDailyVolume10Day,
                Exchange = symbol.FullExchangeName
            };
        }
        catch (Exception)
        {
            // Log.Warning(ex, "Error getting stock data: {ErrorMessage}", ex.Message);
            return default;
        }
    }

    public async Task<IReadOnlyCollection<SymbolData>> SearchSymbolAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentNullException(nameof(query));

        query = Uri.EscapeDataString(query);

        using var http = _httpClientFactory.CreateClient();

        var res = await http.GetStringAsync(
            $"https://finance.yahoo.com/_finance_doubledown/api/resource/searchassist;searchTerm={query}?device=console");

        var data = JsonSerializer.Deserialize<YahooFinanceSearchResponse>(res);

        if (data is null or { Items: null })
            return Array.Empty<SymbolData>();

        return data.Items
                   .Where(x => x.Type == "S")
                   .Select(x => new SymbolData(x.Symbol, x.Name))
                   .ToList();
    }

    private static CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
    {
        PrepareHeaderForMatch = args => args.Header.Humanize(LetterCasing.Title)
    };

    public async Task<IReadOnlyCollection<CandleData>> GetCandleDataAsync(string query)
    {
        using var http = _httpClientFactory.CreateClient();
// https://query1.finance.yahoo.com/v8/finance/chart?symbol=aapl&interval=1d
        var resData = await http.GetFromJsonAsync<YahooChartData>(
            $"https://query1.finance.yahoo.com/v8/finance/chart/{query}"
            + $"?period1={DateTime.UtcNow.Subtract(30.Days()).ToTimestamp().Seconds}"
            + $"&period2={DateTime.UtcNow.ToTimestamp().Seconds}"
            + "&interval=1d");

        var quote =
            resData
                .Chart.Result[0]
                .Indicators
                .Quote
                .First();

        var output = new CandleData[quote.Close.Count];
        for (var i = 0; i < output.Length; i++)
        {
            var open = quote.Open[i];
            var close = quote.Close[i];
            var high = quote.High[i];
            var low = quote.Low[i];
            var vol = quote.Volume[i];

            output[i] = new CandleData(open, close, high, low, vol);
        }

        return output;
    }
}

public class Chart
{
    [JsonPropertyName("result")] public List<YahooResult> Result { get; set; }

    [JsonPropertyName("error")] public object Error { get; set; }
}

public class Indicators
{
    [JsonPropertyName("quote")] public List<YahooQuote> Quote { get; set; }
}

public class YahooQuote
{
    [JsonPropertyName("close")] public List<decimal> Close { get; set; }

    [JsonPropertyName("low")] public List<decimal> Low { get; set; }

    [JsonPropertyName("open")] public List<decimal> Open { get; set; }

    [JsonPropertyName("volume")] public List<int> Volume { get; set; }

    [JsonPropertyName("high")] public List<decimal> High { get; set; }
}

public class YahooResult
{
    [JsonPropertyName("indicators")] public Indicators Indicators { get; set; }
}

public class YahooChartData
{
    [JsonPropertyName("chart")] public Chart Chart { get; set; }
}