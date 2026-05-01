using Microsoft.AspNetCore.Identity;

namespace Ai.WebUI.Database.Entities;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Settings { get; set; } = """{"theme":"dark","defaultModel":"llama3.2"}""";
}
