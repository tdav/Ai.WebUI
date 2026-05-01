namespace Ai.WebUI.Database.Entities;

public class Document
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty;
    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
