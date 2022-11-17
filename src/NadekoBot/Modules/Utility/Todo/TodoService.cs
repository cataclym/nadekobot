using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using NadekoBot.Db.Models;

namespace NadekoBot.Modules.Utility.Todo;

public sealed class TodoService : ITodoService
{
    private readonly DbService _db;

    public TodoService(DbService db)
    {
        _db = db;
    }

    public async Task<bool> AddTodoItemAsync(ulong userId, string task)
    {
        await using var ctx = _db.GetDbContext();

        var count = await ctx.GetTable<TodoItem>()
            .Where(x => x.UserId == userId)
            .CountAsyncLinqToDB();

        if (count >= 100)
            return false;
        
        await ctx.GetTable<TodoItem>()
            .InsertAsync(() => new()
            {
                Text = task,
                DateAdded = DateTime.UtcNow,
                UserId = userId
            });
        
        return true;
    }

    public async Task<bool> RemoveTodoItemAsync(ulong userId, int index)
    {
        await using var ctx = _db.GetDbContext();
        
        var rows = await ctx.GetTable<TodoItem>()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Id)
            .Skip(index)
            .Take(1)
            .DeleteAsync();

        return rows > 0;
    }

    public async Task<IReadOnlyCollection<TodoItem>> GetTodoItemsAsync(ulong userId)
    {
        await using var ctx = _db.GetDbContext();
        var data = await ctx.GetTable<TodoItem>()
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return data;
    }
}