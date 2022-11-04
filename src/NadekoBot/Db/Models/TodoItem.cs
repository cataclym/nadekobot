using NadekoBot.Services.Database.Models;

namespace NadekoBot.Db.Models;

public class TodoItem : DbEntity
{
   public ulong UserId { get; set; } 
   public string Text { get; set; }
}