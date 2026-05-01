# AI Backend + EF Core + Chat History Reducer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Добавить AI-бекенд Ollama через Semantic Kernel, SSE-стриминг, Chat History Reducer, EF Core (PostgreSQL) с сущностями Project/Chat/User/Document и Blazor UI по дизайн-системе DESIGN.md.

**Architecture:** Три проекта в решении: `Ai.WebUI.Database` (EF Core, Identity, сущности), `Ai.WebUI.DataFormats` (существующий, обработка форматов), `Ai.WebUI` (Blazor Server, сервисы, UI). Semantic Kernel подключается к Ollama через `OllamaChatCompletionService`, стриминг реализован через `IAsyncEnumerable<string>` + `InvokeAsync(StateHasChanged)` в Blazor.

**Tech Stack:** .NET 10, Blazor Server, Semantic Kernel 1.x, `Microsoft.SemanticKernel.Connectors.Ollama`, Npgsql EF Core 9, ASP.NET Identity, xUnit.

---

## Карта файлов

| Файл | Действие |
|------|----------|
| `Ai.WebUI.Database/Ai.WebUI.Database.csproj` | Изменить — добавить NuGet |
| `Ai.WebUI.Database/Class1.cs` | Удалить |
| `Ai.WebUI.Database/Entities/AppUser.cs` | Создать |
| `Ai.WebUI.Database/Entities/Project.cs` | Создать |
| `Ai.WebUI.Database/Entities/Chat.cs` | Создать |
| `Ai.WebUI.Database/Entities/Document.cs` | Создать |
| `Ai.WebUI.Database/AppDbContext.cs` | Создать |
| `Ai.WebUI.Database/AppDbContextFactory.cs` | Создать (design-time) |
| `Ai.WebUI/Ai.WebUI.csproj` | Изменить — добавить refs + NuGet |
| `Ai.WebUI/appsettings.json` | Изменить |
| `Ai.WebUI/Infrastructure/ServiceExtensions.cs` | Создать |
| `Ai.WebUI/Program.cs` | Изменить |
| `Ai.WebUI/Services/AI/OllamaChatService.cs` | Создать |
| `Ai.WebUI/Services/AI/ChatHistoryExtensions.cs` | Создать |
| `Ai.WebUI/Services/AI/ChatHistoryReducer.cs` | Создать |
| `Ai.WebUI/Services/DocumentService.cs` | Создать |
| `Ai.WebUI/wwwroot/app.css` | Изменить — CSS-переменные + neumorphic |
| `Ai.WebUI/Components/App.razor` | Изменить — `data-theme`, убрать Bootstrap |
| `Ai.WebUI/Components/Routes.razor` | Изменить — `AuthorizeRouteView` |
| `Ai.WebUI/Components/_Imports.razor` | Изменить — добавить using |
| `Ai.WebUI/Components/Layout/MainLayout.razor` | Изменить — sidebar layout |
| `Ai.WebUI/Components/Layout/NavMenu.razor` | Изменить — проекты + чаты |
| `Ai.WebUI/Components/Pages/Auth/Login.razor` | Создать |
| `Ai.WebUI/Components/Pages/Auth/Register.razor` | Создать |
| `Ai.WebUI/Components/Pages/Projects/ProjectList.razor` | Создать |
| `Ai.WebUI/Components/Pages/Projects/ProjectDetail.razor` | Создать |
| `Ai.WebUI/Components/Pages/Chat/ChatPage.razor` | Создать |
| `Ai.WebUI/Components/Shared/MessageBubble.razor` | Создать |
| `Ai.WebUI/Components/Shared/ChatInput.razor` | Создать |
| `Ai.WebUI/Components/Shared/ModelSelector.razor` | Создать |
| `Ai.WebUI/Components/Shared/DocumentUpload.razor` | Создать |

---

## Task 1: Ai.WebUI.Database — NuGet пакеты

**Files:**
- Modify: `Ai.WebUI.Database/Ai.WebUI.Database.csproj`
- Delete: `Ai.WebUI.Database/Class1.cs`

- [ ] **Шаг 1: Обновить csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.0" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

- [ ] **Шаг 2: Удалить Class1.cs**

```bash
rm Ai.WebUI.Database/Class1.cs
```

- [ ] **Шаг 3: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI.Database/Ai.WebUI.Database.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 4: Commit**

```bash
git add Ai.WebUI.Database/Ai.WebUI.Database.csproj
git rm Ai.WebUI.Database/Class1.cs
git commit -m "chore: setup Ai.WebUI.Database NuGet packages"
```

---

## Task 2: Ai.WebUI.Database — Сущности EF Core

**Files:**
- Create: `Ai.WebUI.Database/Entities/AppUser.cs`
- Create: `Ai.WebUI.Database/Entities/Project.cs`
- Create: `Ai.WebUI.Database/Entities/Chat.cs`
- Create: `Ai.WebUI.Database/Entities/Document.cs`

- [ ] **Шаг 1: Создать AppUser.cs**

```csharp
// Ai.WebUI.Database/Entities/AppUser.cs
using Microsoft.AspNetCore.Identity;

namespace Ai.WebUI.Database.Entities;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Settings { get; set; } = """{"theme":"dark","defaultModel":"llama3.2"}""";
}
```

- [ ] **Шаг 2: Создать Project.cs**

```csharp
// Ai.WebUI.Database/Entities/Project.cs
namespace Ai.WebUI.Database.Entities;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public List<Chat> Chats { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Шаг 3: Создать Chat.cs**

```csharp
// Ai.WebUI.Database/Entities/Chat.cs
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Шаг 4: Создать Document.cs**

```csharp
// Ai.WebUI.Database/Entities/Document.cs
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Шаг 5: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI.Database/Ai.WebUI.Database.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 6: Commit**

```bash
git add Ai.WebUI.Database/Entities/
git commit -m "feat(db): add EF Core entities AppUser, Project, Chat, Document"
```

---

## Task 3: Ai.WebUI.Database — AppDbContext + DesignTimeFactory

**Files:**
- Create: `Ai.WebUI.Database/AppDbContext.cs`
- Create: `Ai.WebUI.Database/AppDbContextFactory.cs`

- [ ] **Шаг 1: Создать AppDbContext.cs**

```csharp
// Ai.WebUI.Database/AppDbContext.cs
using Ai.WebUI.Database.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ai.WebUI.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(p => p.Chats)
                .WithOne(c => c.Project)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Chat>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            e.Property(c => c.ChatHistoryJson).HasColumnType("text");
        });

        builder.Entity<Document>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasOne(d => d.Chat)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            e.Property(d => d.ExtractedText).HasColumnType("text");
        });
    }
}
```

- [ ] **Шаг 2: Создать AppDbContextFactory.cs** (нужен для `dotnet ef migrations`)

```csharp
// Ai.WebUI.Database/AppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ai.WebUI.Database;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=webui_ai_log;Username=postgres;Password=postgres")
            .Options;
        return new AppDbContext(options);
    }
}
```

- [ ] **Шаг 3: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI.Database/Ai.WebUI.Database.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 4: Commit**

```bash
git add Ai.WebUI.Database/AppDbContext.cs Ai.WebUI.Database/AppDbContextFactory.cs
git commit -m "feat(db): add AppDbContext with Identity and EF Core configuration"
```

---

## Task 4: Ai.WebUI — NuGet + Project References + appsettings

**Files:**
- Modify: `Ai.WebUI/Ai.WebUI.csproj`
- Modify: `Ai.WebUI/appsettings.json`

- [ ] **Шаг 1: Обновить Ai.WebUI.csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.SemanticKernel" Version="1.30.0" />
    <PackageReference Include="Microsoft.SemanticKernel.Connectors.Ollama" Version="1.30.0-preview" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Ai.WebUI.Database\Ai.WebUI.Database.csproj" />
    <ProjectReference Include="..\Ai.WebUI.DataFormats\Ai.WebUI.DataFormats.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Шаг 2: Обновить appsettings.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=webui_ai_log;Username=postgres;Password=postgres"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:8000/",
    "DefaultModel": "llama3.2"
  },
  "ChatHistory": {
    "MaxTokens": 4096,
    "ReduceThreshold": 3000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Шаг 3: Восстановить пакеты и убедиться в сборке**

```bash
dotnet restore Ai.WebUI/Ai.WebUI.csproj
dotnet build Ai.WebUI/Ai.WebUI.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 4: Commit**

```bash
git add Ai.WebUI/Ai.WebUI.csproj Ai.WebUI/appsettings.json
git commit -m "chore: add SK, Ollama connector, project references, appsettings"
```

---

## Task 5: Ai.WebUI — ServiceExtensions

**Files:**
- Create: `Ai.WebUI/Infrastructure/ServiceExtensions.cs`

- [ ] **Шаг 1: Создать ServiceExtensions.cs**

```csharp
// Ai.WebUI/Infrastructure/ServiceExtensions.cs
using Ai.WebUI.Database;
using Ai.WebUI.Database.Entities;
using Ai.WebUI.DataFormats;
using Ai.WebUI.Services;
using Ai.WebUI.Services.AI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ai.WebUI.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:8000/";
        var defaultModel = configuration["Ollama:DefaultModel"] ?? "llama3.2";

#pragma warning disable SKEXP0070
        services.AddKernel()
            .AddOllamaChatCompletion(modelId: defaultModel, endpoint: new Uri(baseUrl));
#pragma warning restore SKEXP0070

        services.AddHttpClient("ollama", client =>
            client.BaseAddress = new Uri(baseUrl));

        services.AddScoped<OllamaChatService>();
        services.AddScoped<IChatHistoryReducer, ChatHistoryReducer>();

        return services;
    }

    public static IServiceCollection AddDocumentServices(this IServiceCollection services)
    {
        services.AddDefaultContentDecoders();
        services.AddScoped<DocumentService>();
        return services;
    }
}
```

- [ ] **Шаг 2: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI/Ai.WebUI.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 3: Commit**

```bash
git add Ai.WebUI/Infrastructure/ServiceExtensions.cs
git commit -m "feat: add ServiceExtensions for DB, AI, and document services"
```

---

## Task 6: Ai.WebUI — Program.cs

**Files:**
- Modify: `Ai.WebUI/Program.cs`

- [ ] **Шаг 1: Заменить Program.cs полностью**

```csharp
// Ai.WebUI/Program.cs
using Ai.WebUI.Components;
using Ai.WebUI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddAiServices(builder.Configuration);
builder.Services.AddDocumentServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

- [ ] **Шаг 2: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI/Ai.WebUI.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 3: Commit**

```bash
git add Ai.WebUI/Program.cs
git commit -m "feat: wire up authentication, AI, and document services in Program.cs"
```

---

## Task 7: EF Core — Initial Migration

**Files:**
- Create: `Ai.WebUI.Database/Migrations/` (генерируется автоматически)

- [ ] **Шаг 1: Установить EF Core CLI (если не установлен)**

```bash
dotnet tool install --global dotnet-ef
```

- [ ] **Шаг 2: Создать первую миграцию из папки Ai.WebUI.Database**

```bash
cd Ai.WebUI.Database
dotnet ef migrations add InitialCreate
```
Ожидаем: `Done. To undo this action, use 'ef migrations remove'`

- [ ] **Шаг 3: Применить миграцию к PostgreSQL**

```bash
dotnet ef database update
```
Ожидаем: `Done.`

- [ ] **Шаг 4: Commit**

```bash
cd ..
git add Ai.WebUI.Database/Migrations/
git commit -m "feat(db): add initial EF Core migration with Identity + Project/Chat/Document"
```

---

## Task 8: Ai.WebUI — OllamaChatService + ChatHistoryExtensions

**Files:**
- Create: `Ai.WebUI/Services/AI/ChatHistoryExtensions.cs`
- Create: `Ai.WebUI/Services/AI/OllamaChatService.cs`

- [ ] **Шаг 1: Создать ChatHistoryExtensions.cs**

```csharp
// Ai.WebUI/Services/AI/ChatHistoryExtensions.cs
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ai.WebUI.Services.AI;

public record ChatMessageDto(string Role, string Content);

public static class ChatHistoryExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static string ToJson(this ChatHistory history) =>
        JsonSerializer.Serialize(
            history.Select(m => new ChatMessageDto(m.Role.Label, m.Content ?? string.Empty)));

    public static ChatHistory ToChatHistory(this string json)
    {
        var dtos = JsonSerializer.Deserialize<List<ChatMessageDto>>(json, JsonOptions) ?? [];
        var history = new ChatHistory();
        foreach (var dto in dtos)
        {
            switch (dto.Role.ToLowerInvariant())
            {
                case "user": history.AddUserMessage(dto.Content); break;
                case "assistant": history.AddAssistantMessage(dto.Content); break;
                case "system": history.AddSystemMessage(dto.Content); break;
            }
        }
        return history;
    }

    public static int EstimateTokens(this ChatHistory history) =>
        history.Sum(m => (m.Content?.Length ?? 0) / 4);
}
```

- [ ] **Шаг 2: Создать OllamaChatService.cs**

```csharp
// Ai.WebUI/Services/AI/OllamaChatService.cs
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ai.WebUI.Services.AI;

public class OllamaChatService(Kernel kernel, IHttpClientFactory httpClientFactory)
{
    public async IAsyncEnumerable<string> StreamAsync(
        ChatHistory history,
        string modelId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var settings = new PromptExecutionSettings { ModelId = modelId };

        await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(
            history, settings, kernel, cancellationToken))
        {
            if (chunk.Content is not null)
                yield return chunk.Content;
        }
    }

    public async Task<string> CompleteAsync(
        ChatHistory history,
        string modelId,
        CancellationToken cancellationToken = default)
    {
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var settings = new PromptExecutionSettings { ModelId = modelId };
        var result = await chatService.GetChatMessageContentAsync(history, settings, kernel, cancellationToken);
        return result.Content ?? string.Empty;
    }

    public async Task<List<string>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("ollama");
        var response = await client.GetFromJsonAsync<OllamaTagsResponse>("api/tags", cancellationToken);
        return response?.Models.Select(m => m.Name).ToList() ?? [];
    }

    private record OllamaTagsResponse(
        [property: JsonPropertyName("models")] List<OllamaModel> Models);

    private record OllamaModel(
        [property: JsonPropertyName("name")] string Name);
}
```

- [ ] **Шаг 3: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI/Ai.WebUI.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 4: Commit**

```bash
git add Ai.WebUI/Services/AI/
git commit -m "feat: add OllamaChatService with SSE streaming and ChatHistoryExtensions"
```

---

## Task 9: Ai.WebUI — ChatHistoryReducer (с тестами)

**Files:**
- Create: `Ai.WebUI/Services/AI/ChatHistoryReducer.cs`
- Create: `Ai.WebUI.Tests/Services/AI/ChatHistoryReducerTests.cs` (новый тестовый проект)

- [ ] **Шаг 1: Создать тестовый проект**

```bash
dotnet new xunit -n Ai.WebUI.Tests -o Ai.WebUI.Tests
dotnet add Ai.WebUI.Tests/Ai.WebUI.Tests.csproj reference Ai.WebUI/Ai.WebUI.csproj
dotnet add Ai.WebUI.Tests package Moq
dotnet sln Ai.WebUI.slnx add Ai.WebUI.Tests/Ai.WebUI.Tests.csproj
```

- [ ] **Шаг 2: Написать падающие тесты**

```csharp
// Ai.WebUI.Tests/Services/AI/ChatHistoryReducerTests.cs
using Ai.WebUI.Services.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;

namespace Ai.WebUI.Tests.Services.AI;

public class ChatHistoryReducerTests
{
    private static IConfiguration BuildConfig(int threshold = 100) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ChatHistory:ReduceThreshold"] = threshold.ToString(),
                ["Ollama:DefaultModel"] = "llama3.2"
            })
            .Build();

    private static ChatHistory BuildHistory(int messageCount, int charsPerMessage = 10)
    {
        var h = new ChatHistory();
        h.AddSystemMessage("You are a helpful assistant.");
        for (var i = 0; i < messageCount; i++)
        {
            if (i % 2 == 0) h.AddUserMessage(new string('u', charsPerMessage));
            else h.AddAssistantMessage(new string('a', charsPerMessage));
        }
        return h;
    }

    [Fact]
    public async Task ReduceAsync_WhenUnderThreshold_ReturnsNull()
    {
        var mockChat = new Mock<OllamaChatService>();
        var reducer = new ChatHistoryReducer(mockChat.Object, BuildConfig(threshold: 10000));
        var history = BuildHistory(messageCount: 3, charsPerMessage: 10);

        var result = await reducer.ReduceAsync(history);

        Assert.Null(result);
    }

    [Fact]
    public async Task ReduceAsync_WhenOverThreshold_ReturnsShorterHistory()
    {
        var mockChat = new Mock<OllamaChatService>();
        mockChat
            .Setup(s => s.CompleteAsync(It.IsAny<ChatHistory>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Summary of earlier messages.");

        var reducer = new ChatHistoryReducer(mockChat.Object, BuildConfig(threshold: 1));
        var history = BuildHistory(messageCount: 20, charsPerMessage: 50);

        var result = await reducer.ReduceAsync(history);

        Assert.NotNull(result);
        var list = result.ToList();
        Assert.True(list.Count < history.Count);
        Assert.Contains(list, m => m.Content!.Contains("[Summary"));
    }

    [Fact]
    public async Task ReduceAsync_WhenOverThreshold_PreservesSystemMessage()
    {
        var mockChat = new Mock<OllamaChatService>();
        mockChat
            .Setup(s => s.CompleteAsync(It.IsAny<ChatHistory>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Brief summary.");

        var reducer = new ChatHistoryReducer(mockChat.Object, BuildConfig(threshold: 1));
        var history = BuildHistory(messageCount: 20, charsPerMessage: 50);

        var result = await reducer.ReduceAsync(history);

        Assert.NotNull(result);
        Assert.Equal(AuthorRole.System, result.First().Role);
    }
}
```

- [ ] **Шаг 3: Убедиться что тесты падают**

```bash
dotnet test Ai.WebUI.Tests/Ai.WebUI.Tests.csproj
```
Ожидаем: `Error: The type or namespace 'ChatHistoryReducer' could not be found`

- [ ] **Шаг 4: Создать ChatHistoryReducer.cs**

```csharp
// Ai.WebUI/Services/AI/ChatHistoryReducer.cs
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Ai.WebUI.Services.AI;

public class ChatHistoryReducer(OllamaChatService chatService, IConfiguration configuration)
    : IChatHistoryReducer
{
    private readonly int _reduceThreshold =
        configuration.GetValue<int>("ChatHistory:ReduceThreshold", 3000);
    private readonly string _defaultModel =
        configuration["Ollama:DefaultModel"] ?? "llama3.2";
    private const int KeepLastMessages = 6;

    public async Task<IEnumerable<ChatMessageContent>?> ReduceAsync(
        IReadOnlyList<ChatMessageContent> chatHistory,
        CancellationToken cancellationToken = default)
    {
        if (chatHistory.EstimateTokens() < _reduceThreshold)
            return null;

        var systemMessages = chatHistory.Where(m => m.Role == AuthorRole.System).ToList();
        var nonSystem = chatHistory.Where(m => m.Role != AuthorRole.System).ToList();

        if (nonSystem.Count <= KeepLastMessages)
            return null;

        var toSummarize = nonSystem.Take(nonSystem.Count - KeepLastMessages).ToList();
        var toKeep = nonSystem.TakeLast(KeepLastMessages).ToList();

        var summaryRequest = new ChatHistory();
        summaryRequest.AddUserMessage(
            "Кратко summarize the following conversation in 2-3 sentences:\n" +
            string.Join("\n", toSummarize.Select(m => $"{m.Role.Label}: {m.Content}")));

        var summary = await chatService.CompleteAsync(summaryRequest, _defaultModel, cancellationToken);

        var reduced = new List<ChatMessageContent>();
        reduced.AddRange(systemMessages);
        reduced.Add(new ChatMessageContent(AuthorRole.Assistant,
            $"[Summary of earlier conversation] {summary}"));
        reduced.AddRange(toKeep);

        return reduced;
    }
}

file static class Extensions
{
    public static int EstimateTokens(this IReadOnlyList<ChatMessageContent> history) =>
        history.Sum(m => (m.Content?.Length ?? 0) / 4);
}
```

- [ ] **Шаг 5: Убедиться что тесты проходят**

```bash
dotnet test Ai.WebUI.Tests/Ai.WebUI.Tests.csproj
```
Ожидаем: `Passed! - Failed: 0, Passed: 3`

- [ ] **Шаг 6: Commit**

```bash
git add Ai.WebUI/Services/AI/ChatHistoryReducer.cs Ai.WebUI.Tests/
git commit -m "feat: add ChatHistoryReducer with summarization strategy and unit tests"
```

---

## Task 10: Ai.WebUI — DocumentService

**Files:**
- Create: `Ai.WebUI/Services/DocumentService.cs`

- [ ] **Шаг 1: Создать DocumentService.cs**

```csharp
// Ai.WebUI/Services/DocumentService.cs
using Ai.WebUI.DataFormats;
using Microsoft.AspNetCore.Components.Forms;

namespace Ai.WebUI.Services;

public class DocumentService(IEnumerable<IContentDecoder> decoders)
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public async Task<string> ExtractTextAsync(
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        var decoder = decoders.FirstOrDefault(d => d.SupportsMimeType(file.ContentType));
        if (decoder is null)
            return string.Empty;

        await using var stream = file.OpenReadStream(MaxFileSizeBytes, cancellationToken);
        var content = await decoder.DecodeAsync(stream, cancellationToken);
        return string.Join("\n\n", content.Sections.Select(s => s.Content));
    }

    public async Task<string> ExtractTextAsync(
        string filePath,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        var decoder = decoders.FirstOrDefault(d => d.SupportsMimeType(mimeType));
        if (decoder is null)
            return string.Empty;

        var content = await decoder.DecodeAsync(filePath, cancellationToken);
        return string.Join("\n\n", content.Sections.Select(s => s.Content));
    }

    public bool IsSupported(string mimeType) =>
        decoders.Any(d => d.SupportsMimeType(mimeType));
}
```

- [ ] **Шаг 2: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI/Ai.WebUI.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 3: Commit**

```bash
git add Ai.WebUI/Services/DocumentService.cs
git commit -m "feat: add DocumentService using Ai.WebUI.DataFormats decoders"
```

---

## Task 11: CSS — Дизайн-система (app.css + App.razor + Routes.razor + _Imports.razor)

**Files:**
- Modify: `Ai.WebUI/wwwroot/app.css`
- Modify: `Ai.WebUI/Components/App.razor`
- Modify: `Ai.WebUI/Components/Routes.razor`
- Modify: `Ai.WebUI/Components/_Imports.razor`

- [ ] **Шаг 1: Заменить app.css**

```css
/* Ai.WebUI/wwwroot/app.css */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&family=JetBrains+Mono:wght@400&display=swap');

/* ── Shared variables ─────────────────────────────────────── */
:root {
    --color-accent:        #FF6B2C;
    --color-accent-hover:  #FF8C42;
    --color-accent-glow:   rgba(255,107,44,0.3);
    --color-accent-bg:     rgba(255,107,44,0.10);
    --color-log-error:     #FF4D4D;
    --color-log-warn:      #FFB02E;
    --color-log-info:      #4DA6FF;
    --color-log-debug:     #48C25C;
    --radius-sm:  4px;
    --radius-md:  8px;
    --radius-lg:  16px;
    --radius-xl:  24px;
    --font-ui:   Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    --font-mono: 'JetBrains Mono', 'Fira Code', 'Cascadia Code', monospace;
}

/* ── Dark theme ──────────────────────────────────────────── */
:root, [data-theme="dark"] {
    --color-bg:             #2D2D35;
    --color-panel:          #363640;
    --color-inset:          #1E1E26;
    --color-surface:        #3A3A45;
    --color-text-primary:   #FFFFFF;
    --color-text-secondary: #8A8A9A;
    --color-text-muted:     #7D7D8A;
    --shadow-raised:    6px 6px 12px rgba(0,0,0,0.4), -6px -6px 12px rgba(255,255,255,0.05);
    --shadow-raised-sm: 3px 3px 6px  rgba(0,0,0,0.4), -3px -3px 6px  rgba(255,255,255,0.05);
    --shadow-flat:      2px 2px 5px  rgba(0,0,0,0.4), -2px -2px 5px  rgba(255,255,255,0.05);
    --shadow-inset:    inset 4px 4px 8px rgba(0,0,0,0.4), inset -4px -4px 8px rgba(255,255,255,0.05);
    --shadow-inset-sm: inset 2px 2px 4px rgba(0,0,0,0.4), inset -2px -2px 4px rgba(255,255,255,0.05);
    --shadow-pressed:  inset 3px 3px 6px rgba(0,0,0,0.4), inset -3px -3px 6px rgba(255,255,255,0.05);
}

/* ── Light theme ─────────────────────────────────────────── */
[data-theme="light"] {
    --color-bg:             #F0F0F5;
    --color-panel:          #FFFFFF;
    --color-inset:          #E4E4EC;
    --color-surface:        #FFFFFF;
    --color-text-primary:   #1A1A2E;
    --color-text-secondary: #8A8A9A;
    --color-text-muted:     #7D7D8A;
    --shadow-raised:    6px 6px 12px rgba(0,0,0,0.15), -6px -6px 12px rgba(255,255,255,0.8);
    --shadow-raised-sm: 3px 3px 6px  rgba(0,0,0,0.15), -3px -3px 6px  rgba(255,255,255,0.8);
    --shadow-flat:      2px 2px 5px  rgba(0,0,0,0.15), -2px -2px 5px  rgba(255,255,255,0.8);
    --shadow-inset:    inset 4px 4px 8px rgba(0,0,0,0.15), inset -4px -4px 8px rgba(255,255,255,0.8);
    --shadow-inset-sm: inset 2px 2px 4px rgba(0,0,0,0.15), inset -2px -2px 4px rgba(255,255,255,0.8);
    --shadow-pressed:  inset 3px 3px 6px rgba(0,0,0,0.15), inset -3px -3px 6px rgba(255,255,255,0.8);
}

/* ── Base reset ──────────────────────────────────────────── */
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

html, body {
    height: 100%;
    background: var(--color-bg);
    color: var(--color-text-primary);
    font-family: var(--font-ui);
    font-size: 0.875rem;
    line-height: 1.5;
}

/* ── Buttons ─────────────────────────────────────────────── */
.btn {
    display: inline-flex; align-items: center; gap: 6px;
    padding: 8px 16px; border: none; border-radius: var(--radius-md);
    font-family: var(--font-ui); font-size: 0.875rem; font-weight: 500;
    cursor: pointer; transition: box-shadow 150ms ease, opacity 150ms ease;
}

.btn-accent {
    background: linear-gradient(135deg, #FF6B2C, #FF8C42);
    color: #fff;
    box-shadow: 0 2px 8px var(--color-accent-glow), var(--shadow-raised-sm);
}
.btn-accent:hover  { box-shadow: 0 4px 12px rgba(255,107,44,0.4), var(--shadow-flat); }
.btn-accent:active { box-shadow: var(--shadow-pressed); }

.btn-default {
    background: var(--color-panel);
    color: var(--color-text-primary);
    box-shadow: var(--shadow-raised-sm);
}
.btn-default:hover  { box-shadow: var(--shadow-flat); }
.btn-default:active { box-shadow: var(--shadow-pressed); }

.btn-ghost {
    background: transparent;
    color: var(--color-text-secondary);
}
.btn-ghost:hover { background: var(--color-accent-bg); color: var(--color-text-primary); }

.btn:disabled { opacity: 0.5; cursor: not-allowed; }
.btn-full { width: 100%; justify-content: center; }

/* ── Inputs ──────────────────────────────────────────────── */
.field-label {
    display: block;
    font-size: 0.75rem; font-weight: 500; letter-spacing: 0.02em;
    color: var(--color-text-secondary);
    margin-bottom: 6px;
}

.field-input {
    width: 100%;
    background: var(--color-inset);
    color: var(--color-text-primary);
    border: 1px solid transparent;
    border-radius: var(--radius-md);
    padding: 0 16px;
    height: 36px;
    font-family: var(--font-ui);
    font-size: 0.875rem;
    box-shadow: var(--shadow-inset-sm);
    outline: none;
    transition: border-color 150ms ease;
}
.field-input::placeholder { color: var(--color-text-muted); }
.field-input:focus { border-color: var(--color-accent); }

/* ── Cards ───────────────────────────────────────────────── */
.card {
    background: var(--color-panel);
    border-radius: var(--radius-lg);
    padding: 24px;
    box-shadow: var(--shadow-raised);
}

/* ── Validation ──────────────────────────────────────────── */
.validation-message { color: var(--color-log-error); font-size: 0.75rem; margin-top: 4px; }

/* ── Scrollbar ───────────────────────────────────────────── */
::-webkit-scrollbar { width: 6px; }
::-webkit-scrollbar-track { background: transparent; }
::-webkit-scrollbar-thumb { background: var(--color-surface); border-radius: 3px; }

@media (prefers-reduced-motion: reduce) {
    *, *::before, *::after { transition: none !important; animation: none !important; }
}
```

- [ ] **Шаг 2: Обновить App.razor** — убрать Bootstrap, добавить `data-theme`, `CascadingAuthenticationState`

```razor
<!DOCTYPE html>
<html lang="ru" data-theme="dark">

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <ResourcePreloader />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link rel="stylesheet" href="@Assets["app.css"]" />
    <link rel="stylesheet" href="@Assets["Ai.WebUI.styles.css"]" />
    <ImportMap />
    <link rel="icon" type="image/png" href="favicon.png" />
    <HeadOutlet />
</head>

<body>
    <CascadingAuthenticationState>
        <Routes />
    </CascadingAuthenticationState>
    <ReconnectModal />
    <script src="@Assets["_framework/blazor.web.js"]"></script>
</body>

</html>
```

- [ ] **Шаг 3: Обновить Routes.razor** — добавить `AuthorizeRouteView`

```razor
@using Microsoft.AspNetCore.Authorization

<Router AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)">
            <NotAuthorized>
                <RedirectToLogin />
            </NotAuthorized>
        </AuthorizeRouteView>
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

- [ ] **Шаг 4: Создать RedirectToLogin.razor**

```razor
@* Ai.WebUI/Components/Shared/RedirectToLogin.razor *@
@inject NavigationManager Nav

@code {
    protected override void OnInitialized() =>
        Nav.NavigateTo($"/login?returnUrl={Uri.EscapeDataString(Nav.Uri)}", forceLoad: true);
}
```

- [ ] **Шаг 5: Обновить _Imports.razor**

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using Ai.WebUI
@using Ai.WebUI.Components
@using Ai.WebUI.Components.Layout
@using Ai.WebUI.Components.Shared
@using Ai.WebUI.Database.Entities
```

- [ ] **Шаг 6: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI/Ai.WebUI.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 7: Commit**

```bash
git add Ai.WebUI/wwwroot/app.css Ai.WebUI/Components/App.razor Ai.WebUI/Components/Routes.razor Ai.WebUI/Components/_Imports.razor Ai.WebUI/Components/Shared/RedirectToLogin.razor
git commit -m "feat: add DESIGN.md CSS system, auth routing, and component imports"
```

---

## Task 12: Blazor — MainLayout + NavMenu

**Files:**
- Modify: `Ai.WebUI/Components/Layout/MainLayout.razor`
- Modify: `Ai.WebUI/Components/Layout/NavMenu.razor`

- [ ] **Шаг 1: Заменить MainLayout.razor**

```razor
@* Ai.WebUI/Components/Layout/MainLayout.razor *@
@inherits LayoutComponentBase

<div class="app-shell">
    <NavMenu />
    <main class="main-content">
        @Body
    </main>
</div>

<style>
    .app-shell {
        display: flex;
        height: 100vh;
        overflow: hidden;
        background: var(--color-bg);
    }
    .main-content {
        flex: 1;
        overflow-y: auto;
        padding: 24px;
    }
</style>
```

- [ ] **Шаг 2: Заменить NavMenu.razor**

```razor
@* Ai.WebUI/Components/Layout/NavMenu.razor *@
@using Ai.WebUI.Database
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.EntityFrameworkCore
@inject AppDbContext Db
@inject AuthenticationStateProvider AuthState
@inject NavigationManager Nav

<nav class="sidebar @(collapsed ? "sidebar--collapsed" : "")">
    <div class="sidebar-header">
        @if (!collapsed)
        {
            <span class="sidebar-title">Ai.WebUI</span>
        }
        <button class="btn btn-ghost sidebar-toggle" @onclick="ToggleCollapse">
            @(collapsed ? "›" : "‹")
        </button>
    </div>

    @if (!collapsed)
    {
        <a href="/" class="btn btn-accent sidebar-new-project" style="margin: 12px 16px;">
            + Проект
        </a>

        @foreach (var project in projects)
        {
            <div class="nav-project @(currentProjectId == project.Id ? "nav-project--active" : "")">
                <a href="/projects/@project.Id" class="nav-project-name">
                    @project.Name
                    <span class="nav-badge">@project.Chats.Count</span>
                </a>
                @if (currentProjectId == project.Id)
                {
                    @foreach (var chat in project.Chats.OrderByDescending(c => c.UpdatedAt))
                    {
                        <a href="/chat/@chat.Id"
                           class="nav-chat-item @(currentChatId == chat.Id ? "nav-chat-item--active" : "")">
                            @chat.Title
                        </a>
                    }
                }
            </div>
        }
    }

    <div class="sidebar-footer">
        <a href="/logout" class="btn btn-ghost">Выйти</a>
    </div>
</nav>

<style>
    .sidebar {
        width: 260px; min-width: 260px;
        background: var(--color-panel);
        box-shadow: var(--shadow-raised);
        display: flex; flex-direction: column;
        transition: width 250ms ease, min-width 250ms ease;
        overflow: hidden;
    }
    .sidebar--collapsed { width: 64px; min-width: 64px; }
    .sidebar-header {
        display: flex; align-items: center; justify-content: space-between;
        padding: 16px; border-bottom: 1px solid rgba(255,255,255,0.05);
    }
    .sidebar-title { font-weight: 600; color: var(--color-text-primary); }
    .nav-project { padding: 4px 8px; }
    .nav-project-name {
        display: flex; align-items: center; justify-content: space-between;
        padding: 8px; border-radius: var(--radius-md);
        color: var(--color-text-secondary); text-decoration: none;
        font-weight: 500; transition: background 150ms;
    }
    .nav-project--active .nav-project-name {
        background: var(--color-accent-bg);
        color: var(--color-accent);
    }
    .nav-badge {
        font-size: 0.7rem; background: var(--color-surface);
        padding: 2px 6px; border-radius: 999px;
        color: var(--color-text-muted);
    }
    .nav-chat-item {
        display: block; padding: 6px 8px 6px 20px;
        border-radius: var(--radius-sm);
        color: var(--color-text-secondary); text-decoration: none;
        font-size: 0.8rem; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
        transition: background 150ms;
    }
    .nav-chat-item:hover { background: var(--color-surface); }
    .nav-chat-item--active { color: var(--color-accent); background: var(--color-accent-bg); }
    .sidebar-footer { margin-top: auto; padding: 16px; }
</style>

@code {
    private bool collapsed;
    private List<Project> projects = [];
    private Guid? currentProjectId;
    private Guid? currentChatId;

    protected override async Task OnInitializedAsync()
    {
        var auth = await AuthState.GetAuthenticationStateAsync();
        var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return;

        projects = await Db.Projects
            .Include(p => p.Chats)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }

    private void ToggleCollapse() => collapsed = !collapsed;
}
```

- [ ] **Шаг 3: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI/Ai.WebUI.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 4: Commit**

```bash
git add Ai.WebUI/Components/Layout/
git commit -m "feat: redesign MainLayout and NavMenu with neumorphic sidebar"
```

---

## Task 13: Blazor — Auth Pages (Login, Register)

**Files:**
- Create: `Ai.WebUI/Components/Pages/Auth/Login.razor`
- Create: `Ai.WebUI/Components/Pages/Auth/Register.razor`

- [ ] **Шаг 1: Создать Login.razor**

```razor
@* Ai.WebUI/Components/Pages/Auth/Login.razor *@
@page "/login"
@attribute [AllowAnonymous]
@using Ai.WebUI.Database.Entities
@using Microsoft.AspNetCore.Identity
@inject SignInManager<AppUser> SignInManager
@inject NavigationManager Nav

<PageTitle>Войти — Ai.WebUI</PageTitle>

<div class="auth-wrapper">
    <div class="card auth-card">
        <h1 class="auth-title">Войти</h1>

        <EditForm Model="model" OnValidSubmit="HandleLogin" FormName="login">
            <DataAnnotationsValidator />
            <div class="field" style="margin-bottom:16px">
                <label class="field-label">Email</label>
                <InputText @bind-Value="model.Email" class="field-input" placeholder="you@example.com" autocomplete="email" />
                <ValidationMessage For="() => model.Email" />
            </div>
            <div class="field" style="margin-bottom:24px">
                <label class="field-label">Пароль</label>
                <InputText @bind-Value="model.Password" type="password" class="field-input" placeholder="••••••" autocomplete="current-password" />
                <ValidationMessage For="() => model.Password" />
            </div>

            @if (error is not null)
            {
                <p class="validation-message" style="margin-bottom:12px">@error</p>
            }

            <button type="submit" class="btn btn-accent btn-full">Войти</button>
        </EditForm>

        <p class="auth-link">Нет аккаунта? <a href="/register">Зарегистрироваться</a></p>
    </div>
</div>

<style>
    .auth-wrapper { display: flex; align-items: center; justify-content: center; min-height: 100vh; padding: 24px; }
    .auth-card { width: 100%; max-width: 400px; }
    .auth-title { font-size: 1.5rem; font-weight: 600; margin-bottom: 24px; }
    .auth-link { margin-top: 16px; font-size: 0.8rem; color: var(--color-text-secondary); text-align: center; }
    .auth-link a { color: var(--color-accent); text-decoration: none; }
</style>

@code {
    [SupplyParameterFromForm] private LoginModel model { get; set; } = new();
    [SupplyParameterFromQuery] private string? returnUrl { get; set; }
    private string? error;

    private async Task HandleLogin()
    {
        error = null;
        var result = await SignInManager.PasswordSignInAsync(
            model.Email, model.Password, isPersistent: true, lockoutOnFailure: false);

        if (result.Succeeded)
            Nav.NavigateTo(returnUrl ?? "/", forceLoad: true);
        else
            error = "Неверный email или пароль.";
    }

    private sealed class LoginModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = string.Empty;
    }
}
```

- [ ] **Шаг 2: Создать Register.razor**

```razor
@* Ai.WebUI/Components/Pages/Auth/Register.razor *@
@page "/register"
@attribute [AllowAnonymous]
@using Ai.WebUI.Database.Entities
@using Microsoft.AspNetCore.Identity
@inject UserManager<AppUser> UserManager
@inject SignInManager<AppUser> SignInManager
@inject NavigationManager Nav

<PageTitle>Регистрация — Ai.WebUI</PageTitle>

<div class="auth-wrapper">
    <div class="card auth-card">
        <h1 class="auth-title">Регистрация</h1>

        <EditForm Model="model" OnValidSubmit="HandleRegister" FormName="register">
            <DataAnnotationsValidator />
            <div class="field" style="margin-bottom:16px">
                <label class="field-label">Имя</label>
                <InputText @bind-Value="model.DisplayName" class="field-input" placeholder="Имя" />
                <ValidationMessage For="() => model.DisplayName" />
            </div>
            <div class="field" style="margin-bottom:16px">
                <label class="field-label">Email</label>
                <InputText @bind-Value="model.Email" class="field-input" placeholder="you@example.com" autocomplete="email" />
                <ValidationMessage For="() => model.Email" />
            </div>
            <div class="field" style="margin-bottom:24px">
                <label class="field-label">Пароль</label>
                <InputText @bind-Value="model.Password" type="password" class="field-input" placeholder="Минимум 6 символов" autocomplete="new-password" />
                <ValidationMessage For="() => model.Password" />
            </div>

            @if (error is not null)
            {
                <p class="validation-message" style="margin-bottom:12px">@error</p>
            }

            <button type="submit" class="btn btn-accent btn-full">Создать аккаунт</button>
        </EditForm>

        <p class="auth-link">Уже есть аккаунт? <a href="/login">Войти</a></p>
    </div>
</div>

<style>
    .auth-wrapper { display: flex; align-items: center; justify-content: center; min-height: 100vh; padding: 24px; }
    .auth-card { width: 100%; max-width: 400px; }
    .auth-title { font-size: 1.5rem; font-weight: 600; margin-bottom: 24px; }
    .auth-link { margin-top: 16px; font-size: 0.8rem; color: var(--color-text-secondary); text-align: center; }
    .auth-link a { color: var(--color-accent); text-decoration: none; }
</style>

@code {
    [SupplyParameterFromForm] private RegisterModel model { get; set; } = new();
    private string? error;

    private async Task HandleRegister()
    {
        error = null;
        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.DisplayName
        };
        var result = await UserManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            error = string.Join(" ", result.Errors.Select(e => e.Description));
            return;
        }
        await SignInManager.SignInAsync(user, isPersistent: true);
        Nav.NavigateTo("/", forceLoad: true);
    }

    private sealed class RegisterModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string DisplayName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
```

- [ ] **Шаг 3: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI/Ai.WebUI.csproj
```

- [ ] **Шаг 4: Проверить вручную** — открыть `/login`, убедиться что форма отображается, neumorphic-карточка видна.

- [ ] **Шаг 5: Commit**

```bash
git add Ai.WebUI/Components/Pages/Auth/
git commit -m "feat: add Login and Register pages with Identity"
```

---

## Task 14: Blazor — Projects Pages

**Files:**
- Create: `Ai.WebUI/Components/Pages/Projects/ProjectList.razor`
- Create: `Ai.WebUI/Components/Pages/Projects/ProjectDetail.razor`

- [ ] **Шаг 1: Создать ProjectList.razor**

```razor
@* Ai.WebUI/Components/Pages/Projects/ProjectList.razor *@
@page "/"
@attribute [Authorize]
@using Ai.WebUI.Database
@using Microsoft.EntityFrameworkCore
@inject AppDbContext Db
@inject AuthenticationStateProvider AuthState
@inject NavigationManager Nav

<PageTitle>Проекты — Ai.WebUI</PageTitle>

<div style="max-width:800px">
    <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:24px">
        <h1 style="font-size:1.5rem;font-weight:600">Проекты</h1>
        <button class="btn btn-accent" @onclick="CreateProject">+ Новый проект</button>
    </div>

    @if (projects.Count == 0)
    {
        <div class="card" style="text-align:center;color:var(--color-text-secondary)">
            <p>Проектов пока нет. Создайте первый.</p>
        </div>
    }
    else
    {
        <div style="display:grid;gap:16px">
            @foreach (var project in projects)
            {
                <div class="card project-card" @onclick="() => Nav.NavigateTo($"/projects/{project.Id}")">
                    <div style="display:flex;justify-content:space-between;align-items:start">
                        <div>
                            <h2 style="font-size:1rem;font-weight:500;margin-bottom:4px">@project.Name</h2>
                            @if (project.Description is not null)
                            {
                                <p style="color:var(--color-text-secondary);font-size:0.8rem">@project.Description</p>
                            }
                        </div>
                        <span style="font-size:0.75rem;color:var(--color-text-muted)">
                            @project.Chats.Count чат(ов)
                        </span>
                    </div>
                </div>
            }
        </div>
    }
</div>

<style>
    .project-card { cursor: pointer; transition: box-shadow 150ms; }
    .project-card:hover { box-shadow: var(--shadow-flat); }
</style>

@code {
    private List<Project> projects = [];

    protected override async Task OnInitializedAsync()
    {
        var auth = await AuthState.GetAuthenticationStateAsync();
        var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return;

        projects = await Db.Projects
            .Include(p => p.Chats)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }

    private async Task CreateProject()
    {
        var auth = await AuthState.GetAuthenticationStateAsync();
        var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return;

        var project = new Project { Name = "Новый проект", UserId = userId };
        Db.Projects.Add(project);
        await Db.SaveChangesAsync();
        Nav.NavigateTo($"/projects/{project.Id}");
    }
}
```

- [ ] **Шаг 2: Создать ProjectDetail.razor**

```razor
@* Ai.WebUI/Components/Pages/Projects/ProjectDetail.razor *@
@page "/projects/{projectId:guid}"
@attribute [Authorize]
@using Ai.WebUI.Database
@using Microsoft.EntityFrameworkCore
@inject AppDbContext Db
@inject AuthenticationStateProvider AuthState
@inject NavigationManager Nav

<PageTitle>@(project?.Name ?? "Проект") — Ai.WebUI</PageTitle>

@if (project is null)
{
    <p style="color:var(--color-text-secondary)">Загрузка...</p>
}
else
{
    <div style="max-width:800px">
        <div style="display:flex;align-items:center;gap:16px;margin-bottom:24px">
            @if (editingName)
            {
                <input class="field-input" style="font-size:1.25rem;max-width:320px"
                       @bind="project.Name" @onblur="SaveName" @onkeydown="HandleNameKey" />
            }
            else
            {
                <h1 style="font-size:1.5rem;font-weight:600;cursor:pointer" @onclick="() => editingName = true">
                    @project.Name
                </h1>
            }
            <button class="btn btn-accent" @onclick="CreateChat">+ Новый чат</button>
        </div>

        @if (project.Chats.Count == 0)
        {
            <div class="card" style="text-align:center;color:var(--color-text-secondary)">
                <p>Чатов нет. Создайте первый.</p>
            </div>
        }
        else
        {
            <div style="display:grid;gap:12px">
                @foreach (var chat in project.Chats.OrderByDescending(c => c.UpdatedAt))
                {
                    <div class="card chat-card" @onclick="() => Nav.NavigateTo($"/chat/{chat.Id}")">
                        <div style="display:flex;justify-content:space-between;align-items:center">
                            <span style="font-weight:500">@chat.Title</span>
                            <span style="font-size:0.75rem;color:var(--color-text-muted);font-family:var(--font-mono)">
                                @chat.ModelId
                            </span>
                        </div>
                    </div>
                }
            </div>
        }
    </div>
}

<style>
    .chat-card { cursor: pointer; transition: box-shadow 150ms; }
    .chat-card:hover { box-shadow: var(--shadow-flat); }
</style>

@code {
    [Parameter] public Guid ProjectId { get; set; }

    private Project? project;
    private bool editingName;

    protected override async Task OnInitializedAsync()
    {
        project = await Db.Projects
            .Include(p => p.Chats)
            .FirstOrDefaultAsync(p => p.Id == ProjectId);
    }

    private async Task CreateChat()
    {
        if (project is null) return;
        var auth = await AuthState.GetAuthenticationStateAsync();
        var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return;

        var chat = new Chat { Title = "Новый чат", ProjectId = project.Id, UserId = userId };
        Db.Chats.Add(chat);
        await Db.SaveChangesAsync();
        Nav.NavigateTo($"/chat/{chat.Id}");
    }

    private async Task SaveName()
    {
        editingName = false;
        if (project is null) return;
        project.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync();
    }

    private async Task HandleNameKey(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await SaveName();
        if (e.Key == "Escape") editingName = false;
    }
}
```

- [ ] **Шаг 3: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI/Ai.WebUI.csproj
```

- [ ] **Шаг 4: Commit**

```bash
git add Ai.WebUI/Components/Pages/Projects/
git commit -m "feat: add ProjectList and ProjectDetail pages"
```

---

## Task 15: Blazor — Shared компоненты

**Files:**
- Create: `Ai.WebUI/Components/Shared/MessageBubble.razor`
- Create: `Ai.WebUI/Components/Shared/ChatInput.razor`
- Create: `Ai.WebUI/Components/Shared/ModelSelector.razor`
- Create: `Ai.WebUI/Components/Shared/DocumentUpload.razor`

- [ ] **Шаг 1: Создать MessageBubble.razor**

```razor
@* Ai.WebUI/Components/Shared/MessageBubble.razor *@

<div class="bubble bubble--@Role">
    <div class="bubble-content @(IsStreaming ? "bubble-content--streaming" : "")">
        @((MarkupString)FormattedContent)
    </div>
</div>

<style>
    .bubble { display: flex; margin-bottom: 12px; }
    .bubble--user     { justify-content: flex-end; }
    .bubble--assistant { justify-content: flex-start; }

    .bubble-content {
        max-width: 72%;
        padding: 12px 16px;
        border-radius: var(--radius-lg);
        font-family: var(--font-mono);
        font-size: 0.875rem;
        line-height: 1.6;
        white-space: pre-wrap;
        word-break: break-word;
    }

    .bubble--user .bubble-content {
        background: var(--color-inset);
        box-shadow: var(--shadow-inset-sm);
        color: var(--color-text-primary);
        font-family: var(--font-ui);
    }

    .bubble--assistant .bubble-content {
        background: var(--color-panel);
        box-shadow: var(--shadow-raised-sm);
        color: var(--color-text-primary);
    }

    .bubble-content--streaming::after {
        content: '▋';
        display: inline-block;
        animation: blink 1s step-end infinite;
        color: var(--color-accent);
    }

    @keyframes blink { 50% { opacity: 0; } }
</style>

@code {
    [Parameter, EditorRequired] public string Role { get; set; } = "user";
    [Parameter, EditorRequired] public string Content { get; set; } = string.Empty;
    [Parameter] public bool IsStreaming { get; set; }

    private string FormattedContent =>
        System.Net.WebUtility.HtmlEncode(Content).Replace("\n", "<br />");
}
```

- [ ] **Шаг 2: Создать ChatInput.razor**

```razor
@* Ai.WebUI/Components/Shared/ChatInput.razor *@

<div class="chat-input-bar">
    <textarea class="chat-textarea field-input"
              @bind="text"
              @bind:event="oninput"
              @onkeydown="HandleKey"
              placeholder="Введите сообщение... (Enter — отправить, Shift+Enter — новая строка)"
              rows="1"
              disabled="@Disabled" />
    <button class="btn btn-accent" @onclick="Send" disabled="@(Disabled || string.IsNullOrWhiteSpace(text))">
        ↑
    </button>
</div>

<style>
    .chat-input-bar {
        display: flex; gap: 8px; align-items: flex-end;
        padding: 12px 0;
        border-top: 1px solid rgba(255,255,255,0.05);
    }
    .chat-textarea {
        flex: 1; resize: none; height: auto;
        min-height: 36px; max-height: 160px;
        padding: 8px 16px; line-height: 1.5;
        font-family: var(--font-ui);
        overflow-y: auto;
    }
</style>

@code {
    [Parameter] public EventCallback<string> OnSend { get; set; }
    [Parameter] public bool Disabled { get; set; }

    private string text = string.Empty;

    private async Task HandleKey(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
            await Send();
    }

    private async Task Send()
    {
        var msg = text.Trim();
        if (string.IsNullOrEmpty(msg)) return;
        text = string.Empty;
        await OnSend.InvokeAsync(msg);
    }
}
```

- [ ] **Шаг 3: Создать ModelSelector.razor**

```razor
@* Ai.WebUI/Components/Shared/ModelSelector.razor *@
@using Ai.WebUI.Services.AI
@inject OllamaChatService OllamaChat

<div class="model-selector">
    <label class="field-label">Модель</label>
    <select class="field-input" value="@SelectedModel" @onchange="OnChanged">
        @foreach (var m in models)
        {
            <option value="@m">@m</option>
        }
    </select>
</div>

@code {
    [Parameter] public string SelectedModel { get; set; } = "llama3.2";
    [Parameter] public EventCallback<string> SelectedModelChanged { get; set; }

    private List<string> models = [];

    protected override async Task OnInitializedAsync()
    {
        try { models = await OllamaChat.GetModelsAsync(); }
        catch { models = [SelectedModel]; }

        if (!models.Contains(SelectedModel) && models.Count > 0)
            SelectedModel = models[0];
    }

    private async Task OnChanged(ChangeEventArgs e) =>
        await SelectedModelChanged.InvokeAsync(e.Value?.ToString() ?? SelectedModel);
}
```

- [ ] **Шаг 4: Создать DocumentUpload.razor**

```razor
@* Ai.WebUI/Components/Shared/DocumentUpload.razor *@
@using Ai.WebUI.Database
@using Ai.WebUI.Services
@inject DocumentService DocService
@inject AppDbContext Db

<div class="doc-upload @(isDragOver ? "doc-upload--dragover" : "")"
     @ondragover:preventDefault
     @ondragover="() => isDragOver = true"
     @ondragleave="() => isDragOver = false"
     @ondrop:preventDefault
     @ondrop="HandleDrop">

    <InputFile OnChange="HandleFileSelected" multiple class="doc-upload-input" accept=".pdf,.txt,.md,.docx,.xlsx,.pptx" />

    <span style="color:var(--color-text-muted);font-size:0.8rem">
        Перетащите файлы или кликните для выбора
    </span>
</div>

@if (documents.Count > 0)
{
    <div class="doc-list">
        @foreach (var doc in documents)
        {
            <div class="doc-pill">
                <span>@doc.FileName</span>
                <button class="doc-remove" @onclick="() => RemoveDocument(doc)">×</button>
            </div>
        }
    </div>
}

<style>
    .doc-upload {
        position: relative; display: flex; align-items: center; justify-content: center;
        padding: 16px; border-radius: var(--radius-md);
        background: var(--color-inset); box-shadow: var(--shadow-inset-sm);
        border: 1px solid transparent; cursor: pointer;
        transition: border-color 150ms;
    }
    .doc-upload--dragover { border-color: var(--color-accent); }
    .doc-upload-input {
        position: absolute; inset: 0; opacity: 0; cursor: pointer;
    }
    .doc-list { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 8px; }
    .doc-pill {
        display: flex; align-items: center; gap: 6px;
        padding: 4px 10px; border-radius: 999px;
        background: var(--color-surface); font-size: 0.75rem;
        color: var(--color-text-secondary);
        box-shadow: var(--shadow-inset-sm);
    }
    .doc-remove {
        background: none; border: none; cursor: pointer;
        color: var(--color-text-muted); font-size: 1rem; line-height: 1;
    }
    .doc-remove:hover { color: var(--color-log-error); }
</style>

@code {
    [Parameter, EditorRequired] public Guid ChatId { get; set; }
    [Parameter, EditorRequired] public string UserId { get; set; } = string.Empty;
    [Parameter] public EventCallback<List<Database.Entities.Document>> OnDocumentsChanged { get; set; }

    private List<Database.Entities.Document> documents = [];
    private bool isDragOver;

    private async Task HandleFileSelected(InputFileChangeEventArgs e) =>
        await ProcessFiles(e.GetMultipleFiles());

    private async Task HandleDrop(DragEventArgs e)
    {
        isDragOver = false;
        // drag-drop файлы обрабатываются через InputFile — событие drop перехватывает браузер
    }

    private async Task ProcessFiles(IReadOnlyList<IBrowserFile> files)
    {
        foreach (var file in files)
        {
            if (!DocService.IsSupported(file.ContentType)) continue;

            var extractedText = await DocService.ExtractTextAsync(file);
            var doc = new Database.Entities.Document
            {
                FileName = file.Name,
                ContentType = file.ContentType,
                FilePath = string.Empty,
                ExtractedText = extractedText,
                ChatId = ChatId,
                UserId = UserId
            };
            Db.Documents.Add(doc);
            documents.Add(doc);
        }
        await Db.SaveChangesAsync();
        await OnDocumentsChanged.InvokeAsync(documents);
    }

    private async Task RemoveDocument(Database.Entities.Document doc)
    {
        Db.Documents.Remove(doc);
        documents.Remove(doc);
        await Db.SaveChangesAsync();
        await OnDocumentsChanged.InvokeAsync(documents);
    }
}
```

- [ ] **Шаг 5: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI/Ai.WebUI.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 6: Commit**

```bash
git add Ai.WebUI/Components/Shared/
git commit -m "feat: add MessageBubble, ChatInput, ModelSelector, DocumentUpload components"
```

---

## Task 16: Blazor — ChatPage с SSE-стримингом

**Files:**
- Create: `Ai.WebUI/Components/Pages/Chat/ChatPage.razor`

- [ ] **Шаг 1: Создать ChatPage.razor**

```razor
@* Ai.WebUI/Components/Pages/Chat/ChatPage.razor *@
@page "/chat/{chatId:guid}"
@attribute [Authorize]
@implements IDisposable
@using Ai.WebUI.Database
@using Ai.WebUI.Services.AI
@using Microsoft.EntityFrameworkCore
@using Microsoft.SemanticKernel.ChatCompletion
@inject AppDbContext Db
@inject OllamaChatService OllamaChat
@inject IChatHistoryReducer HistoryReducer
@inject AuthenticationStateProvider AuthState

<PageTitle>@(chat?.Title ?? "Чат") — Ai.WebUI</PageTitle>

@if (chat is null)
{
    <p style="color:var(--color-text-secondary)">Загрузка...</p>
}
else
{
    <div class="chat-page">
        <div class="chat-header">
            @if (editingTitle)
            {
                <input class="field-input" style="max-width:320px"
                       @bind="chat.Title" @onblur="SaveTitle" @onkeydown="HandleTitleKey" />
            }
            else
            {
                <h1 class="chat-title" @onclick="() => editingTitle = true">@chat.Title</h1>
            }
            <ModelSelector @bind-SelectedModel="chat.ModelId" />
        </div>

        <div class="messages-area" @ref="messagesRef">
            @foreach (var msg in history)
            {
                <MessageBubble Role="@msg.Role.Label" Content="@(msg.Content ?? string.Empty)" />
            }
            @if (isStreaming)
            {
                <MessageBubble Role="assistant" Content="@streamingText" IsStreaming="true" />
            }
        </div>

        <div class="chat-bottom">
            <DocumentUpload ChatId="chat.Id" UserId="@userId" OnDocumentsChanged="OnDocsChanged" />
            <ChatInput OnSend="HandleSend" Disabled="@isStreaming" />
        </div>
    </div>
}

<style>
    .chat-page {
        display: flex; flex-direction: column;
        height: calc(100vh - 48px);
    }
    .chat-header {
        display: flex; align-items: center; gap: 16px;
        padding-bottom: 16px; border-bottom: 1px solid rgba(255,255,255,0.05);
        margin-bottom: 8px;
    }
    .chat-title {
        font-size: 1.25rem; font-weight: 500; cursor: pointer;
        color: var(--color-text-primary);
    }
    .messages-area {
        flex: 1; overflow-y: auto; padding: 8px 0;
    }
    .chat-bottom { flex-shrink: 0; }
</style>

@code {
    [Parameter] public Guid ChatId { get; set; }

    private Database.Entities.Chat? chat;
    private ChatHistory history = new();
    private List<Database.Entities.Document> chatDocs = [];

    private string streamingText = string.Empty;
    private bool isStreaming;
    private bool editingTitle;
    private string userId = string.Empty;

    private ElementReference messagesRef;
    private CancellationTokenSource cts = new();

    protected override async Task OnInitializedAsync()
    {
        var auth = await AuthState.GetAuthenticationStateAsync();
        userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        chat = await Db.Chats
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == ChatId);

        if (chat is null) return;

        chatDocs = chat.Documents.ToList();
        history = chat.ChatHistoryJson.ToChatHistory();
    }

    private async Task HandleSend(string userMessage)
    {
        if (chat is null) return;

        isStreaming = true;
        streamingText = string.Empty;

        history.AddUserMessage(userMessage);

        // Добавить текст документов в контекст (если есть)
        if (chatDocs.Count > 0)
        {
            var docsContext = string.Join("\n\n---\n\n",
                chatDocs.Select(d => $"[{d.FileName}]\n{d.ExtractedText}"));
            var enriched = new ChatHistory();
            enriched.AddSystemMessage($"Контекст из прикреплённых документов:\n{docsContext}");
            foreach (var m in history) enriched.Add(m);

            await StreamResponse(enriched);
        }
        else
        {
            await StreamResponse(history);
        }
    }

    private async Task StreamResponse(ChatHistory activeHistory)
    {
        if (chat is null) return;

        // Применить reducer если нужно
        var reduced = await HistoryReducer.ReduceAsync(activeHistory, cts.Token);
        if (reduced is not null)
        {
            history = new ChatHistory();
            foreach (var m in reduced) history.Add(m);
            activeHistory = history;
        }

        try
        {
            await foreach (var token in OllamaChat.StreamAsync(activeHistory, chat.ModelId, cts.Token))
            {
                streamingText += token;
                await InvokeAsync(StateHasChanged);
            }

            history.AddAssistantMessage(streamingText);
            chat.ChatHistoryJson = history.ToJson();
            chat.TotalTokens = history.EstimateTokens();
            chat.UpdatedAt = DateTime.UtcNow;
            await Db.SaveChangesAsync();
        }
        catch (OperationCanceledException) { }
        finally
        {
            isStreaming = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SaveTitle()
    {
        editingTitle = false;
        if (chat is null) return;
        chat.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync();
    }

    private async Task HandleTitleKey(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await SaveTitle();
        if (e.Key == "Escape") editingTitle = false;
    }

    private void OnDocsChanged(List<Database.Entities.Document> docs) => chatDocs = docs;

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }
}
```

- [ ] **Шаг 2: Добавить метод EstimateTokens в ChatHistoryExtensions**

Открыть `Ai.WebUI/Services/AI/ChatHistoryExtensions.cs` и добавить:

```csharp
public static int EstimateTokens(this ChatHistory history) =>
    history.Sum(m => (m.Content?.Length ?? 0) / 4);
```

(Метод уже должен быть добавлен в Task 8 — убедиться что он есть.)

- [ ] **Шаг 3: Убедиться что проект собирается**

```bash
dotnet build Ai.WebUI/Ai.WebUI.csproj
```
Ожидаем: `Build succeeded`

- [ ] **Шаг 4: Запустить приложение и проверить вручную**

```bash
dotnet run --project Ai.WebUI/Ai.WebUI.csproj
```

Проверить:
1. `/register` → создать пользователя
2. `/login` → войти
3. `/` → создать проект
4. Открыть проект → создать чат
5. Написать сообщение → убедиться что токены стримируются в реальном времени (typewriter)
6. Прикрепить PDF → убедиться что текст извлекается и передаётся в контекст

- [ ] **Шаг 5: Commit**

```bash
git add Ai.WebUI/Components/Pages/Chat/
git commit -m "feat: add ChatPage with SSE streaming, Chat History Reducer, and document context"
```

---

## Self-Review

**Spec coverage:**
- ✅ Ollama backend (Task 8: OllamaChatService)
- ✅ SSE streaming (Task 16: `IAsyncEnumerable` + `InvokeAsync(StateHasChanged)`)
- ✅ Chat History Reducer (Task 9: `ChatHistoryReducer` + tests)
- ✅ EF Core — Project (Task 2, 14)
- ✅ EF Core — Chat (Task 2, 16)
- ✅ EF Core — User / Identity (Task 2, 13)
- ✅ EF Core — Document (Task 2, 15)
- ✅ Ai.WebUI.Database class library (Tasks 1-3)
- ✅ Ai.WebUI.DataFormats (Task 10)
- ✅ DESIGN.md — CSS переменные, neumorphic, Ember Orange (Task 11)
- ✅ Dark/light theme (Task 11: `[data-theme]`)
- ✅ PostgreSQL (Task 4, 7)
- ✅ ASP.NET Identity (Tasks 5, 13)

**Placeholders:** Отсутствуют. Весь код полный.

**Type consistency:**
- `Chat.ChatHistoryJson` → сериализуется через `ToJson()` / `ToChatHistory()` из `ChatHistoryExtensions`
- `OllamaChatService` → используется в `ChatHistoryReducer` (dependency), `ModelSelector`, `ChatPage`
- `IChatHistoryReducer` → реализован `ChatHistoryReducer`, инжектируется в `ChatPage`
- `IContentDecoder` → из `Ai.WebUI.DataFormats`, инжектируется в `DocumentService`
- `AppDbContext` → из `Ai.WebUI.Database`, инжектируется в страницы и `DocumentUpload`
