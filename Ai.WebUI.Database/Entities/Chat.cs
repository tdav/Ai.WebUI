namespace Ai.WebUI.Database.Entities;

public class Chat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "Новый чат";
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public string ModelId { get; set; } = "llama3.2";
    public string ChatHistoryJson { get; set; } = "[]";
    public int TotalTokens { get; set; }
    public List<Document> Documents { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
