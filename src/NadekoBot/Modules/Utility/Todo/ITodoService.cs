using NadekoBot.Db.Models;

namespace NadekoBot.Modules.Utility.Todo;

public interface ITodoService
{
    Task AddTodoItemAsync(ulong userId, string task);
    Task<bool> RemoveTodoItemAsync(ulong userId, int index);
    Task<IReadOnlyCollection<TodoItem>> GetTodoItemsAsync(ulong userId);
}