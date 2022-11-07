using NadekoBot.Db.Models;

namespace NadekoBot.Modules.Utility.Todo;


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
        
        [Cmd]
        public async Task TodoAdd([Leftover] string task)
        {
            await _service.AddTodoItemAsync(ctx.User.Id, task);

            // todo max count?

            // todo show todos button
            await ctx.OkAsync();
        }

        [Cmd]
        public async Task TodoRemove(int index)
        {
            if (--index < 0)
                return;
            
            var ok = await _service.RemoveTodoItemAsync(ctx.User.Id, index);
            if (ok)
                await ReplyConfirmLocalizedAsync(strs.todo_removed(index + 1));
            else
                await ReplyConfirmLocalizedAsync(strs.todo_index_out_of_range(index + 1));
        }

        [Cmd]
        public async Task TodoList(string group = "", int page = 1)
        {
            if (--page < 0)
                return;

            var items = await _service.GetTodoItemsAsync(ctx.User.Id);
            if (items.Count == 0)
            {
                await ReplyPendingLocalizedAsync(strs.todo_none);
                return;
            }

            await ctx.SendPaginatedConfirmAsync(page, (curPage) =>
            {
                var eb = _eb.Create(ctx)
                    .WithOkColor()
                    .WithTitle(GetText(strs.todo_list));

                var desc = items
                    .Skip(9 * curPage)
                    .Take(9)
                    .Select((x, index) => $"`{index:N2}.`[{GetIcon(x)}] {x.Text}")
                    .Join('\n');

                eb.WithDescription(desc);

                return eb;
            }, items.Count, 9);
        }

        private string GetIcon(TodoItem item)
        {
            if (item.State == TodoState.Completed)
                return "✅";

            if (item.State == TodoState.Expired)
                return "⌛";

            if (item.State == TodoState.InProgress)
                return " ";

            return "?";
        }
    }
}