namespace NadekoBot.Modules.Utility.Todo;

public interface ITodoService
{
    Task AddTodoItemAsync(ulong userId, string task);
    Task<bool> RemoveTodoItemAsync(ulong userId, int index);
    Task GetTodoItemsAsync(ulong userId, int page);
}

public sealed class TodoService : ITodoService
{
    private readonly DbService _db;

    public TodoService(DbService db)
    {
        _db = db;
    }

    public async Task AddTodoItemAsync(ulong userId, string task)
    {
        await using var ctx = _db.GetDbContext();
    }

    public async Task<bool> RemoveTodoItemAsync(ulong userId, int index)
    {
        await using var ctx = _db.GetDbContext();
    }

    public async Task GetTodoItemsAsync(ulong userId, int page)
    {
        if (page < 0)
            throw new ArgumentOutOfRangeException(nameof(page));

        await using var ctx = _db.GetDbContext();
    }
}

public partial class Utility
{
    [Group("Todo")]
    [Name("todo")]
    public partial class TodoCommands : NadekoModule
    {
        private readonly ITodoService _service;

        public TodoCommands(ITodoService service)
        {
            _service = service;
        }

        public async Task TodoAdd([Leftover] string task)
        {
            var result = await _service.AddTodoItemAsync(ctx.User.Id, task);

            // todo disallow duplicate items

            // todo max count?

            //todo show todos button
            await ctx.OkAsync();
        }

        public async Task TodoRemove(int index)
        {
            var ok = await _service.RemoveTodoItemAsync(ctx.User.Id, index);
            if (ok)
                await ReplyConfirmLocalizedAsync(strs.todo_removed);
            else
                await ReplyConfirmLocalizedAsync(strs.todo_remove_fail);
        }

        public async Task TodoList(int page = 1)
        {
            if (--page < 0)
                return;

            var items = await _service.GetTodoItemsAsync(ctx.User.Id, page);
            if (items.Count == 0)
            {
                await ReplyPendingLocalizedAsync(strs.todo_none);
                return;
            }

            await ctx.SendPaginatedConfirmAsync(page, (curPage) =>
            {
                var eb = _eb.Create(ctx)
                    .WithOkColor();

                items.Skip(9 * curPage).Take(9);
            }, items.Count, 9);
        }
    }
}