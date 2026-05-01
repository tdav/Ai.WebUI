# UI/UX Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Переработать UX сайдбара, создания проектов/чатов и прикрепления файлов согласно спецификации `docs/superpowers/specs/2026-05-01-ui-ux-redesign.md`.

**Architecture:** Новый компонент `CreateProjectModal` инкапсулирует создание проекта + первого чата; `NavMenu` и `ProjectList` открывают его вместо прямого создания. Логика загрузки файлов переезжает из `DocumentUpload` напрямую в `ChatInput`, который становится единой точкой ввода текста и файлов.

**Tech Stack:** Blazor Server (.NET 10), MSTest, EF Core, `DocumentService` (`Ai.WebUI.Services`), `IDbContextFactory<MyDbContext>`.

---

## Структура файлов

| Файл | Действие |
|------|----------|
| `Ai.WebUI/Components/Shared/CreateProjectModal.razor` | Создать |
| `Ai.WebUI/Components/Layout/NavMenu.razor` | Изменить |
| `Ai.WebUI/Components/Pages/Projects/ProjectList.razor` | Изменить |
| `Ai.WebUI/Components/Shared/ChatInput.razor` | Изменить |
| `Ai.WebUI/Components/Pages/Chat/ChatPage.razor` | Изменить |
| `Ai.WebUI/Components/Shared/DocumentUpload.razor` | Удалить |

---

## Task 1: CreateProjectModal.razor

**Files:**
- Create: `Ai.WebUI/Components/Shared/CreateProjectModal.razor`

- [ ] **Шаг 1: Создать файл компонента**

```razor
@* Ai.WebUI/Components/Shared/CreateProjectModal.razor *@
@using Ai.WebUI.Database
@using Ai.WebUI.Database.Entities
@using Microsoft.EntityFrameworkCore
@inject IDbContextFactory<MyDbContext> DbFactory
@inject AuthenticationStateProvider AuthState
@inject NavigationManager Nav

@if (IsOpen)
{
    <div class="modal-overlay" @onclick="Close">
        <div class="modal" @onclick:stopPropagation>
            <h2 class="modal-title">Новый проект</h2>

            <div class="form-group">
                <label class="field-label">Название</label>
                <input class="field-input" @bind="name" @bind:event="oninput"
                       placeholder="Название проекта" maxlength="100" @onkeydown="HandleKey" />
                @if (nameError is not null)
                {
                    <span class="field-error">@nameError</span>
                }
            </div>

            <div class="form-group">
                <label class="field-label">
                    Описание <span style="color:var(--color-text-muted)">(необязательно)</span>
                </label>
                <textarea class="field-input" @bind="description" rows="2"
                          maxlength="500" placeholder="Краткое описание..." />
            </div>

            @if (submitError is not null)
            {
                <div class="error-banner">@submitError</div>
            }

            <div class="modal-actions">
                <button class="btn btn-accent" @onclick="Submit" disabled="@isLoading">
                    @(isLoading ? "Создаю..." : "Создать")
                </button>
                <button class="btn btn-ghost" @onclick="Close" disabled="@isLoading">Отмена</button>
            </div>
        </div>
    </div>
}

<style>
    .modal-overlay {
        position: fixed; inset: 0; z-index: 1000;
        background: rgba(0,0,0,0.6);
        display: flex; align-items: center; justify-content: center;
    }
    .modal {
        background: var(--color-panel);
        border-radius: var(--radius-lg);
        box-shadow: var(--shadow-raised);
        padding: 24px; width: 400px; max-width: 90vw;
        display: flex; flex-direction: column; gap: 16px;
    }
    .modal-title { font-size: 1.1rem; font-weight: 600; color: var(--color-text-primary); margin: 0; }
    .form-group { display: flex; flex-direction: column; gap: 6px; }
    .field-label { font-size: 0.8rem; color: var(--color-text-secondary); }
    .field-error { font-size: 0.75rem; color: var(--color-log-error); }
    .error-banner {
        padding: 8px 12px; border-radius: var(--radius-sm);
        background: rgba(239,68,68,0.1); color: var(--color-log-error);
        font-size: 0.8rem;
    }
    .modal-actions { display: flex; gap: 8px; justify-content: flex-end; margin-top: 4px; }
</style>

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnCreated { get; set; }

    private string name = string.Empty;
    private string? description;
    private string? nameError;
    private string? submitError;
    private bool isLoading;

    private void Close()
    {
        if (isLoading) return;
        name = string.Empty;
        description = null;
        nameError = null;
        submitError = null;
        OnClose.InvokeAsync();
    }

    private async Task HandleKey(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await Submit();
        if (e.Key == "Escape") Close();
    }

    private async Task Submit()
    {
        nameError = null;
        submitError = null;

        if (string.IsNullOrWhiteSpace(name))
        {
            nameError = "Название обязательно";
            return;
        }

        isLoading = true;
        try
        {
            var auth = await AuthState.GetAuthenticationStateAsync();
            var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
            {
                submitError = "Ошибка авторизации";
                return;
            }

            await using var db = DbFactory.CreateDbContext();
            var project = new Project
            {
                Name = name.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                UserId = userId
            };
            db.Projects.Add(project);
            var chat = new Chat { Title = "Новый чат", ProjectId = project.Id, UserId = userId };
            db.Chats.Add(chat);
            await db.SaveChangesAsync();

            await OnCreated.InvokeAsync();
            Nav.NavigateTo("/chat/" + chat.Id);
        }
        catch (Exception)
        {
            submitError = "Не удалось создать проект. Попробуй ещё раз.";
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

- [ ] **Шаг 2: Собрать проект — убедиться что компилируется**

```bash
dotnet build Ai.WebUI.slnx
```

Ожидаем: `Build succeeded.`

- [ ] **Шаг 3: Коммит**

```bash
git add Ai.WebUI/Components/Shared/CreateProjectModal.razor
git commit -m "feat: add CreateProjectModal component with project+chat creation"
```

---

## Task 2: NavMenu.razor

**Files:**
- Modify: `Ai.WebUI/Components/Layout/NavMenu.razor`

- [ ] **Шаг 1: Заменить содержимое файла**

Заменить всё содержимое `Ai.WebUI/Components/Layout/NavMenu.razor` на:

```razor
@* Ai.WebUI/Components/Layout/NavMenu.razor *@
@using Ai.WebUI.Database
@using Ai.WebUI.Database.Entities
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.EntityFrameworkCore
@implements IDisposable
@inject MyDbContext Db
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
        <button class="btn btn-accent sidebar-new-project" @onclick="OpenCreateModal">
            + Проект
        </button>

        <div class="nav-projects">
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
                        <button class="nav-new-chat" @onclick="() => CreateChat(project.Id)">
                            + чат
                        </button>
                    }
                </div>
            }
        </div>
    }
    else
    {
        <div class="nav-projects-collapsed">
            @foreach (var project in projects)
            {
                <a href="/projects/@project.Id"
                   class="nav-project-icon @(currentProjectId == project.Id ? "nav-project-icon--active" : "")"
                   title="@project.Name">
                    @project.Name[0].ToString().ToUpper()
                </a>
            }
        </div>
    }

    <div class="sidebar-footer">
        <form method="post" action="/logout">
            <AntiforgeryToken />
            <button type="submit" class="btn btn-ghost sidebar-logout">
                <span class="logout-icon">⎋</span>
                @if (!collapsed)
                {
                    <span>Выйти</span>
                }
            </button>
        </form>
    </div>
</nav>

<CreateProjectModal IsOpen="showCreateModal"
                    OnClose="CloseCreateModal"
                    OnCreated="OnProjectCreated" />

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
    .sidebar-new-project {
        margin: 12px 16px;
        width: calc(100% - 32px);
    }
    .nav-projects { flex: 1; overflow-y: auto; padding: 0 8px; }
    .nav-projects-collapsed { flex: 1; display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 8px 0; }
    .nav-project { padding: 4px 0; }
    .nav-project-name {
        display: flex; align-items: center; justify-content: space-between;
        padding: 8px; border-radius: var(--radius-md);
        color: var(--color-text-secondary); text-decoration: none;
        font-weight: 500; transition: background 150ms;
    }
    .nav-project-name:hover { background: var(--color-surface); }
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
    .nav-new-chat {
        display: block; width: 100%;
        padding: 5px 8px 5px 20px;
        background: none; border: none; cursor: pointer;
        color: var(--color-text-muted); font-size: 0.78rem;
        text-align: left; border-radius: var(--radius-sm);
        transition: background 150ms, color 150ms;
    }
    .nav-new-chat:hover { background: var(--color-surface); color: var(--color-accent); }
    .nav-project-icon {
        display: flex; align-items: center; justify-content: center;
        width: 36px; height: 36px; border-radius: var(--radius-md);
        background: var(--color-surface);
        color: var(--color-text-secondary); text-decoration: none;
        font-size: 0.85rem; font-weight: 600;
        transition: background 150ms;
    }
    .nav-project-icon:hover { background: var(--color-accent-bg); color: var(--color-accent); }
    .nav-project-icon--active { background: var(--color-accent-bg); color: var(--color-accent); }
    .sidebar-footer { margin-top: auto; padding: 16px; border-top: 1px solid rgba(255,255,255,0.05); }
    .sidebar-logout {
        display: flex; align-items: center; gap: 8px;
        width: 100%; justify-content: flex-start;
    }
    .logout-icon { font-size: 1rem; }
</style>

@code {
    private bool collapsed;
    private bool showCreateModal;
    private List<Project> projects = [];
    private Guid? currentProjectId;
    private Guid? currentChatId;

    protected override async Task OnInitializedAsync()
    {
        await LoadProjects();
        ParseCurrentRoute(Nav.Uri);
        Nav.LocationChanged += OnLocationChanged;
    }

    private async Task LoadProjects()
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

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        ParseCurrentRoute(e.Location);
        InvokeAsync(StateHasChanged);
    }

    private void ParseCurrentRoute(string uri)
    {
        var path = new Uri(uri).AbsolutePath;
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        currentProjectId = null;
        currentChatId = null;

        if (segments.Length >= 2 && segments[0] == "projects" && Guid.TryParse(segments[1], out var pid))
            currentProjectId = pid;
        else if (segments.Length >= 2 && segments[0] == "chat" && Guid.TryParse(segments[1], out var cid))
        {
            currentChatId = cid;
            currentProjectId = projects.FirstOrDefault(p => p.Chats.Any(c => c.Id == cid))?.Id;
        }
    }

    private void OpenCreateModal() => showCreateModal = true;
    private void CloseCreateModal() => showCreateModal = false;

    private async Task OnProjectCreated()
    {
        showCreateModal = false;
        await LoadProjects();
        await InvokeAsync(StateHasChanged);
    }

    private async Task CreateChat(Guid projectId)
    {
        var auth = await AuthState.GetAuthenticationStateAsync();
        var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return;

        try
        {
            var chat = new Chat { Title = "Новый чат", ProjectId = projectId, UserId = userId };
            Db.Chats.Add(chat);
            await Db.SaveChangesAsync();
            await LoadProjects();
            Nav.NavigateTo("/chat/" + chat.Id);
        }
        catch (Exception)
        {
            // TODO: добавить toast-уведомление об ошибке
        }
    }

    public void Dispose() => Nav.LocationChanged -= OnLocationChanged;

    private void ToggleCollapse() => collapsed = !collapsed;
}
```

- [ ] **Шаг 2: Собрать проект**

```bash
dotnet build Ai.WebUI.slnx
```

Ожидаем: `Build succeeded.`

- [ ] **Шаг 3: Коммит**

```bash
git add Ai.WebUI/Components/Layout/NavMenu.razor
git commit -m "feat: redesign NavMenu — modal for project creation, +chat button, logout icon, collapsed icons"
```

---

## Task 3: ProjectList.razor

**Files:**
- Modify: `Ai.WebUI/Components/Pages/Projects/ProjectList.razor`

- [ ] **Шаг 1: Заменить содержимое файла**

```razor
@* Ai.WebUI/Components/Pages/Projects/ProjectList.razor *@
@page "/"
@attribute [Authorize]
@using Ai.WebUI.Database
@using Ai.WebUI.Database.Entities
@using Microsoft.EntityFrameworkCore
@inject MyDbContext Db
@inject AuthenticationStateProvider AuthState
@inject NavigationManager Nav

<PageTitle>Проекты — Ai.WebUI</PageTitle>

<div style="max-width:800px">
    <div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:24px">
        <h1 style="font-size:1.5rem;font-weight:600">Проекты</h1>
        <button class="btn btn-accent" @onclick="() => showCreateModal = true">+ Новый проект</button>
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
                <div class="card project-card" @onclick="() => OpenProject(project.Id)">
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

<CreateProjectModal IsOpen="showCreateModal"
                    OnClose="() => showCreateModal = false"
                    OnCreated="OnProjectCreated" />

<style>
    .project-card { cursor: pointer; transition: box-shadow 150ms; }
    .project-card:hover { box-shadow: var(--shadow-flat); }
</style>

@code {
    private List<Project> projects = [];
    private bool showCreateModal;

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

    private void OpenProject(Guid id) => Nav.NavigateTo("/projects/" + id);

    private async Task OnProjectCreated()
    {
        showCreateModal = false;
        // Страница не перезагружается — навигация произошла в модалке
        // Но если пользователь закроет модалку без создания, список актуален
    }
}
```

- [ ] **Шаг 2: Собрать проект**

```bash
dotnet build Ai.WebUI.slnx
```

Ожидаем: `Build succeeded.`

- [ ] **Шаг 3: Коммит**

```bash
git add Ai.WebUI/Components/Pages/Projects/ProjectList.razor
git commit -m "feat: ProjectList uses CreateProjectModal instead of direct project creation"
```

---

## Task 4: ChatInput.razor — прикрепление файлов

**Files:**
- Modify: `Ai.WebUI/Components/Shared/ChatInput.razor`

- [ ] **Шаг 1: Заменить содержимое файла**

```razor
@* Ai.WebUI/Components/Shared/ChatInput.razor *@
@using Ai.WebUI.Database
@using Ai.WebUI.Database.Entities
@using Ai.WebUI.Services
@using Microsoft.EntityFrameworkCore
@inject DocumentService DocService
@inject IDbContextFactory<MyDbContext> DbFactory

<div class="chat-input-wrapper"
     @ondragover:preventDefault
     @ondragover="() => isDragOver = true"
     @ondragleave="() => isDragOver = false"
     @ondrop:preventDefault
     @ondrop="HandleDrop">

    @if (attachedDocs.Count > 0)
    {
        <div class="doc-chips">
            @foreach (var doc in attachedDocs)
            {
                <div class="doc-chip">
                    <span class="doc-chip-name">@doc.FileName</span>
                    <button class="doc-chip-remove" @onclick="() => RemoveDoc(doc)">×</button>
                </div>
            }
        </div>
    }

    @if (uploadError is not null)
    {
        <div class="upload-error">@uploadError</div>
    }

    <div class="chat-input-bar @(isDragOver ? "chat-input-bar--dragover" : "")">
        <label class="attach-btn" title="Прикрепить файл">
            📎
            <InputFile OnChange="HandleFileSelected" multiple
                       accept=".pdf,.txt,.md,.docx,.xlsx,.pptx"
                       class="attach-input" />
        </label>
        <textarea class="chat-textarea field-input"
                  @bind="text"
                  @bind:event="oninput"
                  @onkeydown="HandleKey"
                  placeholder="Введите сообщение... (Enter — отправить, Shift+Enter — новая строка)"
                  rows="1"
                  disabled="@Disabled" />
        <button class="btn btn-accent"
                @onclick="Send"
                disabled="@(Disabled || string.IsNullOrWhiteSpace(text))">
            ↑
        </button>
    </div>
</div>

<style>
    .chat-input-wrapper {
        display: flex; flex-direction: column; gap: 6px;
    }
    .doc-chips { display: flex; flex-wrap: wrap; gap: 6px; }
    .doc-chip {
        display: flex; align-items: center; gap: 6px;
        padding: 4px 10px; border-radius: 999px;
        background: var(--color-surface); font-size: 0.75rem;
        color: var(--color-text-secondary);
        box-shadow: var(--shadow-inset-sm);
    }
    .doc-chip-name { max-width: 160px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .doc-chip-remove {
        background: none; border: none; cursor: pointer;
        color: var(--color-text-muted); font-size: 1rem; line-height: 1;
        padding: 0;
    }
    .doc-chip-remove:hover { color: var(--color-log-error); }
    .upload-error {
        font-size: 0.75rem; color: var(--color-log-error);
        padding: 4px 8px; background: rgba(239,68,68,0.08);
        border-radius: var(--radius-sm);
    }
    .chat-input-bar {
        display: flex; gap: 8px; align-items: flex-end;
        padding: 12px 0;
        border-top: 1px solid rgba(255,255,255,0.05);
        transition: border-color 150ms;
    }
    .chat-input-bar--dragover { border-color: var(--color-accent); }
    .attach-btn {
        position: relative; cursor: pointer;
        color: var(--color-text-muted); font-size: 1.15rem;
        line-height: 1; flex-shrink: 0;
        transition: color 150ms;
        padding-bottom: 4px;
    }
    .attach-btn:hover { color: var(--color-accent); }
    .attach-input {
        position: absolute; inset: 0; opacity: 0;
        cursor: pointer; width: 100%; height: 100%;
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
    [Parameter, EditorRequired] public Guid ChatId { get; set; }
    [Parameter, EditorRequired] public string UserId { get; set; } = string.Empty;
    [Parameter] public EventCallback<List<Document>> OnDocumentsChanged { get; set; }

    private string text = string.Empty;
    private List<Document> attachedDocs = [];
    private bool isDragOver;
    private string? uploadError;

    private async Task HandleFileSelected(InputFileChangeEventArgs e) =>
        await ProcessFiles(e.GetMultipleFiles());

    private Task HandleDrop(DragEventArgs e)
    {
        isDragOver = false;
        return Task.CompletedTask;
    }

    private async Task ProcessFiles(IReadOnlyList<IBrowserFile> files)
    {
        uploadError = null;
        await using var db = DbFactory.CreateDbContext();
        foreach (var file in files)
        {
            if (!DocService.IsSupported(file.ContentType))
            {
                uploadError = $"Формат не поддерживается: {file.Name}";
                continue;
            }

            try
            {
                var extractedText = await DocService.ExtractTextAsync(file);
                var doc = new Document
                {
                    FileName = file.Name,
                    ContentType = file.ContentType,
                    FilePath = string.Empty,
                    ExtractedText = extractedText,
                    ChatId = ChatId,
                    UserId = UserId
                };
                db.Documents.Add(doc);
                attachedDocs.Add(doc);
            }
            catch (Exception)
            {
                uploadError = $"Ошибка при обработке файла: {file.Name}";
            }
        }
        await db.SaveChangesAsync();
        await OnDocumentsChanged.InvokeAsync(attachedDocs);
    }

    private async Task RemoveDoc(Document doc)
    {
        await using var db = DbFactory.CreateDbContext();
        var existing = await db.Documents.FindAsync(doc.Id);
        if (existing is not null) db.Documents.Remove(existing);
        attachedDocs.Remove(doc);
        await db.SaveChangesAsync();
        await OnDocumentsChanged.InvokeAsync(attachedDocs);
    }

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

- [ ] **Шаг 2: Собрать проект**

```bash
dotnet build Ai.WebUI.slnx
```

Ожидаем: `Build succeeded.`

- [ ] **Шаг 3: Коммит**

```bash
git add Ai.WebUI/Components/Shared/ChatInput.razor
git commit -m "feat: ChatInput — add file attachment with paperclip button and chips"
```

---

## Task 5: ChatPage.razor — удалить DocumentUpload, подключить новый ChatInput

**Files:**
- Modify: `Ai.WebUI/Components/Pages/Chat/ChatPage.razor`

- [ ] **Шаг 1: Найти блок `chat-bottom` в файле**

Открыть `Ai.WebUI/Components/Pages/Chat/ChatPage.razor` и найти этот фрагмент:

```razor
<div class="chat-bottom">
    <DocumentUpload ChatId="chat.Id" UserId="@userId" OnDocumentsChanged="OnDocsChanged" />
    <ChatInput OnSend="HandleSend" Disabled="@isStreaming" />
</div>
```

- [ ] **Шаг 2: Заменить блок `chat-bottom`**

Заменить найденный фрагмент на:

```razor
<div class="chat-bottom">
    <ChatInput OnSend="HandleSend"
               Disabled="@isStreaming"
               ChatId="chat.Id"
               UserId="@userId"
               OnDocumentsChanged="OnDocsChanged" />
</div>
```

- [ ] **Шаг 3: Собрать проект**

```bash
dotnet build Ai.WebUI.slnx
```

Ожидаем: `Build succeeded.`  
Если есть предупреждение о `DocumentUpload` — игнорируем, он будет удалён в Task 6.

- [ ] **Шаг 4: Коммит**

```bash
git add Ai.WebUI/Components/Pages/Chat/ChatPage.razor
git commit -m "feat: ChatPage — remove DocumentUpload block, pass params to ChatInput"
```

---

## Task 6: Удалить DocumentUpload.razor + финальная сборка

**Files:**
- Delete: `Ai.WebUI/Components/Shared/DocumentUpload.razor`

- [ ] **Шаг 1: Убедиться что на DocumentUpload нет других ссылок**

```bash
grep -r "DocumentUpload" Ai.WebUI/Components --include="*.razor"
```

Ожидаем: пустой вывод (нет ссылок).

- [ ] **Шаг 2: Удалить файл**

```bash
rm Ai.WebUI/Components/Shared/DocumentUpload.razor
```

- [ ] **Шаг 3: Финальная сборка**

```bash
dotnet build Ai.WebUI.slnx
```

Ожидаем: `Build succeeded.` без предупреждений о DocumentUpload.

- [ ] **Шаг 4: Запустить тесты**

```bash
dotnet test Ai.WebUI.Tests/Ai.WebUI.Tests.csproj
```

Ожидаем: все тесты зелёные.

- [ ] **Шаг 5: Коммит**

```bash
git add -u
git commit -m "chore: delete DocumentUpload.razor — logic moved to ChatInput"
```

---

## Task 7: Ручная проверка в браузере

- [ ] **Запустить приложение**

```bash
dotnet run --project Ai.WebUI/Ai.WebUI.csproj
```

- [ ] **Чеклист сайдбара**
  - [ ] «+ Проект» открывает модальное окно (не переходит на `/`)
  - [ ] Пустое название → inline-ошибка, модалка не закрывается
  - [ ] Создание проекта → появляется новый проект в списке, переход в чат
  - [ ] «+ чат» в раскрытом проекте → создаёт чат, переход в него
  - [ ] «Выйти» показывает иконку ⎋
  - [ ] Свёрнутый сайдбар → иконки первых букв проектов с tooltip

- [ ] **Чеклист страницы проектов**
  - [ ] «+ Новый проект» открывает ту же модалку
  - [ ] Escape или клик на оверлей закрывает модалку без создания

- [ ] **Чеклист прикрепления файлов**
  - [ ] 📎 кнопка → file picker с поддерживаемыми форматами
  - [ ] После выбора файла — chip с именем над полем ввода
  - [ ] «×» на chip → удаляет файл из списка и БД
  - [ ] Drag-and-drop файла на чат → файл прикрепляется
  - [ ] Неподдерживаемый формат → ошибка рядом с chips
  - [ ] Отправка сообщения с прикреплённым файлом → текст файла попадает в контекст

- [ ] **Коммит финального чеклиста (опционально)**

```bash
git commit --allow-empty -m "chore: manual QA passed for ui-ux-redesign"
```

---

## Task 8: Impeccable critique

- [ ] **Запустить `/impeccable` для critique изменённых файлов**

Scope:
- `Ai.WebUI/Components/Shared/CreateProjectModal.razor`
- `Ai.WebUI/Components/Layout/NavMenu.razor`
- `Ai.WebUI/Components/Pages/Projects/ProjectList.razor`
- `Ai.WebUI/Components/Shared/ChatInput.razor`
- `Ai.WebUI/Components/Pages/Chat/ChatPage.razor`

- [ ] **Если critique выявил проблемы — исправить и перезапустить**

```bash
dotnet build Ai.WebUI.slnx
dotnet test Ai.WebUI.Tests/Ai.WebUI.Tests.csproj
```

- [ ] **Финальный коммит после исправлений**

```bash
git add .
git commit -m "refactor: apply impeccable critique fixes for ui-ux-redesign"
```
