﻿#nullable disable warnings
using NadekoBot.Common.Yml;
using NadekoBot.Db;
using NadekoBot.Services.Database.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NadekoBot.Modules.Utility;

public partial class Utility
{
    [Group]
    public partial class QuoteCommands : NadekoModule
    {
        private const string PREPEND_EXPORT =
            """
                # Keys are keywords, Each key has a LIST of quotes in the following format:
                # - id: Alphanumeric id used for commands related to the quote. (Note, when using .quotesimport, a new id will be generated.) 
                #   an: Author name 
                #   aid: Author id 
                #   txt: Quote text

                """;

        private static readonly ISerializer _exportSerializer = new SerializerBuilder()
            .WithEventEmitter(args
                => new MultilineScalarFlowStyleEmitter(args))
            .WithNamingConvention(
                CamelCaseNamingConvention.Instance)
            .WithIndentedSequences()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling
                .OmitDefaults)
            .DisableAliases()
            .Build();

        private readonly DbService _db;
        private readonly IHttpClientFactory _http;
        private readonly IQuoteService _qs;

        public QuoteCommands(DbService db, IQuoteService qs, IHttpClientFactory http)
        {
            _db = db;
            _http = http;
            _qs = qs;
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [Priority(1)]
        public Task ListQuotes(OrderType order = OrderType.Keyword)
            => ListQuotes(1, order);

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [Priority(0)]
        public async Task ListQuotes(int page = 1, OrderType order = OrderType.Keyword)
        {
            page -= 1;
            if (page < 0)
                return;

            IEnumerable<Quote> quotes;
            await using (var uow = _db.GetDbContext())
            {
                quotes = uow.Set<Quote>().GetGroup(ctx.Guild.Id, page, order);
            }

            if (quotes.Any())
            {
                await SendConfirmAsync(GetText(strs.quotes_page(page + 1)),
                    string.Join("\n",
                        quotes.Select(q
                            => $"`#{q.Id}` {Format.Bold(q.Keyword.SanitizeAllMentions()),-20} by {q.AuthorName.SanitizeAllMentions()}")));
            }
            else
                await ReplyErrorLocalizedAsync(strs.quotes_page_none);
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task QuotePrint([Leftover] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return;

            keyword = keyword.ToUpperInvariant();

            Quote quote;
            await using (var uow = _db.GetDbContext())
            {
                quote = await uow.Set<Quote>().GetRandomQuoteByKeywordAsync(ctx.Guild.Id, keyword);
                //if (quote is not null)
                //{
                //    quote.UseCount += 1;
                //    uow.Complete();
                //}
            }

            if (quote is null)
                return;

            var rep = new ReplacementBuilder().WithDefault(Context).Build();

            var text = SmartText.CreateFrom(quote.Text);
            text = rep.Replace(text);

            await ctx.Channel.SendAsync($"`#{quote.Id}` 📣 " + text, true);
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task QuoteShow(int id)
        {
            Quote? quote;
            await using (var uow = _db.GetDbContext())
            {
                quote = uow.Set<Quote>().GetById(id);
                if (quote?.GuildId != ctx.Guild.Id)
                    quote = null;
            }

            if (quote is null)
            {
                await ReplyErrorLocalizedAsync(strs.quotes_notfound);
                return;
            }

            await ShowQuoteData(quote);
        }

        private async Task ShowQuoteData(Quote data)
            => await ctx.Channel.EmbedAsync(_eb.Create(ctx)
                .WithOkColor()
                .WithTitle(GetText(strs.quote_id($"#{data.Id}")))
                .AddField(GetText(strs.trigger), data.Keyword)
                .AddField(GetText(strs.response),
                    Format.Sanitize(data.Text).Replace("](", "]\\("))
                .WithFooter(
                    GetText(strs.created_by($"{data.AuthorName} ({data.AuthorId})"))));

        private async Task QuoteSearchinternalAsync(string? keyword, string textOrAuthor)
        {
            if (string.IsNullOrWhiteSpace(textOrAuthor))
                return;

            keyword = keyword?.ToUpperInvariant();

            Quote quote;
            await using (var uow = _db.GetDbContext())
            {
                quote = await uow.Set<Quote>().SearchQuoteKeywordTextAsync(ctx.Guild.Id, keyword, textOrAuthor);
            }

            if (quote is null)
                return;

            await ctx.Channel.SendMessageAsync($"`#{quote.Id}` 💬 "
                                               + quote.Keyword.ToLowerInvariant()
                                               + ":  "
                                               + quote.Text.SanitizeAllMentions());
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [Priority(0)]
        public Task QuoteSearch(string textOrAuthor)
            => QuoteSearchinternalAsync(null, textOrAuthor);
        
        [Cmd]
        [RequireContext(ContextType.Guild)]
        [Priority(1)]
        public Task QuoteSearch(string keyword, [Leftover] string textOrAuthor)
            => QuoteSearchinternalAsync(keyword, textOrAuthor);

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task QuoteId(int id)
        {
            if (id < 0)
                return;

            Quote quote;

            var rep = new ReplacementBuilder().WithDefault(Context).Build();

            await using (var uow = _db.GetDbContext())
            {
                quote = uow.Set<Quote>().GetById(id);
            }

            if (quote is null || quote.GuildId != ctx.Guild.Id)
            {
                await SendErrorAsync(GetText(strs.quotes_notfound));
                return;
            }

            var infoText = $"`#{quote.Id} added by {quote.AuthorName.SanitizeAllMentions()}` 🗯️ "
                           + quote.Keyword.ToLowerInvariant().SanitizeAllMentions()
                           + ":\n";


            var text = SmartText.CreateFrom(quote.Text);
            text = rep.Replace(text);
            await ctx.Channel.SendAsync(infoText + text, true);
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task QuoteAdd(string keyword, [Leftover] string text)
        {
            if (string.IsNullOrWhiteSpace(keyword) || string.IsNullOrWhiteSpace(text))
                return;

            keyword = keyword.ToUpperInvariant();

            Quote q;
            await using (var uow = _db.GetDbContext())
            {
                uow.Set<Quote>().Add(q = new()
                {
                    AuthorId = ctx.Message.Author.Id,
                    AuthorName = ctx.Message.Author.Username,
                    GuildId = ctx.Guild.Id,
                    Keyword = keyword,
                    Text = text
                });
                await uow.SaveChangesAsync();
            }

            await ReplyConfirmLocalizedAsync(strs.quote_added_new(Format.Code(q.Id.ToString())));
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task QuoteDelete(int id)
        {
            var hasManageMessages = ((IGuildUser)ctx.Message.Author).GuildPermissions.ManageMessages;

            var success = false;
            string response;
            await using (var uow = _db.GetDbContext())
            {
                var q = uow.Set<Quote>().GetById(id);

                if (q?.GuildId != ctx.Guild.Id || (!hasManageMessages && q.AuthorId != ctx.Message.Author.Id))
                    response = GetText(strs.quotes_remove_none);
                else
                {
                    uow.Set<Quote>().Remove(q);
                    await uow.SaveChangesAsync();
                    success = true;
                    response = GetText(strs.quote_deleted(id));
                }
            }

            if (success)
                await SendConfirmAsync(response);
            else
                await SendErrorAsync(response);
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        public Task QuoteDeleteAuthor(IUser user)
            => QuoteDeleteAuthor(user.Id);
        
        [Cmd]
        [RequireContext(ContextType.Guild)]
        public async Task QuoteDeleteAuthor(ulong userId)
        {
            var hasManageMessages = ((IGuildUser)ctx.Message.Author).GuildPermissions.ManageMessages;

            if (userId == ctx.User.Id || hasManageMessages)
            {
                var deleted = await _qs.DeleteAllAuthorQuotesAsync(ctx.Guild.Id, userId);
                await ReplyConfirmLocalizedAsync(strs.quotes_deleted_count(deleted));
            }
            else
            {
                await ReplyErrorLocalizedAsync(strs.insuf_perms_u);
            }
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [UserPerm(GuildPerm.ManageMessages)]
        public async Task DelAllQuotes([Leftover] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return;

            keyword = keyword.ToUpperInvariant();

            await using (var uow = _db.GetDbContext())
            {
                uow.Set<Quote>().RemoveAllByKeyword(ctx.Guild.Id, keyword.ToUpperInvariant());

                await uow.SaveChangesAsync();
            }

            await ReplyConfirmLocalizedAsync(strs.quotes_deleted(Format.Bold(keyword.SanitizeAllMentions())));
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [UserPerm(GuildPerm.Administrator)]
        public async Task QuotesExport()
        {
            IEnumerable<Quote> quotes;
            await using (var uow = _db.GetDbContext())
            {
                quotes = uow.Set<Quote>().GetForGuild(ctx.Guild.Id).ToList();
            }

            var exprsDict = quotes.GroupBy(x => x.Keyword)
                .ToDictionary(x => x.Key, x => x.Select(ExportedQuote.FromModel));

            var text = PREPEND_EXPORT + _exportSerializer.Serialize(exprsDict).UnescapeUnicodeCodePoints();

            await using var stream = await text.ToStream();
            await ctx.Channel.SendFileAsync(stream, "quote-export.yml");
        }

        [Cmd]
        [RequireContext(ContextType.Guild)]
        [UserPerm(GuildPerm.Administrator)]
        [Ratelimit(300)]
#if GLOBAL_NADEKO
            [OwnerOnly]
#endif
        public async Task QuotesImport([Leftover] string? input = null)
        {
            input = input?.Trim();

            _ = ctx.Channel.TriggerTypingAsync();

            if (input is null)
            {
                var attachment = ctx.Message.Attachments.FirstOrDefault();
                if (attachment is null)
                {
                    await ReplyErrorLocalizedAsync(strs.expr_import_no_input);
                    return;
                }

                using var client = _http.CreateClient();
                input = await client.GetStringAsync(attachment.Url);

                if (string.IsNullOrWhiteSpace(input))
                {
                    await ReplyErrorLocalizedAsync(strs.expr_import_no_input);
                    return;
                }
            }

            var succ = await ImportExprsAsync(ctx.Guild.Id, input);
            if (!succ)
            {
                await ReplyErrorLocalizedAsync(strs.expr_import_invalid_data);
                return;
            }

            await ctx.OkAsync();
        }

        private async Task<bool> ImportExprsAsync(ulong guildId, string input)
        {
            Dictionary<string, List<ExportedQuote>> data;
            try
            {
                data = Yaml.Deserializer.Deserialize<Dictionary<string, List<ExportedQuote>>>(input);
                if (data.Sum(x => x.Value.Count) == 0)
                    return false;
            }
            catch
            {
                return false;
            }

            await using var uow = _db.GetDbContext();
            foreach (var entry in data)
            {
                var keyword = entry.Key;
                await uow.Set<Quote>().AddRangeAsync(entry.Value.Where(quote => !string.IsNullOrWhiteSpace(quote.Txt))
                    .Select(quote => new Quote
                    {
                        GuildId = guildId,
                        Keyword = keyword,
                        Text = quote.Txt,
                        AuthorId = quote.Aid,
                        AuthorName = quote.An
                    }));
            }

            await uow.SaveChangesAsync();
            return true;
        }

        public class ExportedQuote
        {
            public string Id { get; set; }
            public string An { get; set; }
            public ulong Aid { get; set; }
            public string Txt { get; set; }

            public static ExportedQuote FromModel(Quote quote)
                => new()
                {
                    Id = ((kwum)quote.Id).ToString(),
                    An = quote.AuthorName,
                    Aid = quote.AuthorId,
                    Txt = quote.Text
                };
        }
    }
}