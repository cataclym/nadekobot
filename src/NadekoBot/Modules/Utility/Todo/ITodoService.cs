using NadekoBot.Db.Models;

namespace NadekoBot.Modules.Utility.Todo;

public interface ITodoService
{
    Task<bool> AddTodoItemAsync(ulong userId, string task);
    Task<bool> RemoveTodoItemAsync(ulong userId, int index);
    Task<IReadOnlyCollection<TodoItem>> GetTodoItemsAsync(ulong userId);
    Task<(TodoCompleteResult, TodoItem)> CompleteTodoAsync(ulong userId, int index);
}

public enum TodoCompleteResult
{
    Success,
    OutOfRange,
    AlreadyCompleted,
}