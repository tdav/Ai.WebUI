# Спецификация: AI Backend, EF Core, Chat History Reducer, SSE

**Дата:** 2026-05-01  
**Проект:** Ai.WebUI — офлайн-first AI-платформа  
**Стек:** Blazor Server (.NET 10), Semantic Kernel, Ollama, EF Core + PostgreSQL, ASP.NET Identity

---

## 1. Цель

Добавить в пустой Blazor Server-стартер полноценный AI-чат с:
- Ollama-бекендом через Semantic Kernel (http://localhost:8000/)
- Стриминговыми ответами (SSE → Blazor SignalR → typewriter-эффект)
- Chat History Reducer для управления размером контекста
- Персистентностью через EF Core + PostgreSQL
- ASP.NET Identity для аутентификации
- UI строго по дизайн-системе DESIGN.md (neumorphic, Ember Orange, Inter/JetBrains Mono)

---

## 2. Модель данных (EF Core)

### AppUser : IdentityUser
| Поле | Тип | Описание |
|------|-----|----------|
| DisplayName | string | Отображаемое имя |
| CreatedAt | DateTime | Дата регистрации |
| Settings | string (JSON) | Тема (dark/light), дефолтная модель, язык |

### Project
| Поле | Тип | Описание |
|------|-----|----------|
| Id | Guid | PK |
| Name | string | Название проекта |
| Description | string? | Описание |
| UserId | string | FK → AppUser |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

### Chat
| Поле | Тип | Описание |
|------|-----|----------|
| Id | Guid | PK |
| Title | string | Заголовок чата |
| ProjectId | Guid | FK → Project |
| UserId | string | FK → AppUser |
| ModelId | string | Имя модели Ollama (например, llama3.2) |
| ChatHistoryJson | string | Сериализованная SK ChatHistory (JSON) |
| TotalTokens | int | Кешированный счётчик токенов |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

### Document
| Поле | Тип | Описание |
|------|-----|----------|
| Id | Guid | PK |
| FileName | string | Оригинальное имя файла |
| ContentType | string | MIME-тип (application/pdf, text/plain и др.) |
| FilePath | string | Путь в wwwroot/uploads/{userId}/{chatId}/ |
| ExtractedText | string | Извлечённый текст для передачи в контекст |
| ChatId | Guid | FK → Chat |
| UserId | string | FK → AppUser |
| CreatedAt | DateTime | |

### Связи
```
AppUser 1──* Project
Project 1──* Chat
Chat    1──* Document
```

### База данных
```
Host=localhost;Database=webui_ai_log;Username=postgres;Password=postgres
```
NuGet: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`

---

## 3. AI-слой

### Конфигурация (appsettings.json)
```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:8000/",
    "DefaultModel": "llama3.2"
  },
  "ChatHistory": {
    "MaxTokens": 4096,
    "ReduceThreshold": 3000
  }
}
```

### OllamaChatService (Scoped)
- Оборачивает `Kernel` с `OllamaChatCompletionService`
- **`StreamAsync(ChatHistory history, string modelId) → IAsyncEnumerable<string>`**  
  Вызывает `kernel.GetStreamingChatMessageContentsAsync()`, возвращает токены по одному
- **`CompleteAsync(ChatHistory history, string modelId) → string`**  
  Полный ответ без стриминга — используется редьюсером для суммаризации

### ChatHistoryReducer (реализует `IChatHistoryReducer`)
- Триггер: `TotalTokens > ReduceThreshold` перед каждым вызовом к модели
- Стратегия суммаризации:
  1. SystemMessage сохраняется всегда
  2. Последние N (= 6) сообщений сохраняются нетронутыми
  3. Все более старые сообщения суммируются через `CompleteAsync` в одно `AssistantMessage` с префиксом `[Summary of earlier conversation]`
  4. Итоговая история = SystemMessage + SummaryMessage + последние N сообщений
- После редукции `TotalTokens` обновляется в Chat

### Поток обработки запроса
```
1. User отправляет сообщение в ChatPage
2. Сообщение добавляется в ChatHistory
3. ChatHistoryReducer.ReduceAsync(history) — если TotalTokens > ReduceThreshold
4. Документы чата добавляются как UserMessage перед основным запросом (если есть)
5. OllamaChatService.StreamAsync(history, modelId)
6. await foreach (token) → currentStreamingText += token → StateHasChanged()
7. По завершении стриминга: AssistantMessage добавляется в history
8. Chat.ChatHistoryJson и Chat.TotalTokens сохраняются в БД
```

---

## 4. Аутентификация (ASP.NET Identity)

- ASP.NET Identity поверх того же `AppDbContext` (PostgreSQL)
- Cookie-аутентификация (стандарт Blazor Server)
- Неаутентифицированный пользователь → редирект на `/login`
- `[Authorize]` на всех страницах кроме Auth
- Регистрация открытая (localhost = защита через сетевой доступ)

**Страницы:**
- `/login` — форма email + пароль
- `/register` — регистрация нового пользователя

---

## 5. Структура проекта

Решение состоит из трёх проектов:

```
Ai.WebUI.slnx
├── Ai.WebUI/                          (Blazor Server — UI + сервисы)
├── Ai.WebUI.Database/                 (Class Library — EF Core, сущности, миграции)
└── Ai.WebUI.DataFormats/              (Class Library — обработка форматов документов, уже создан)
```

### Ai.WebUI.Database (Class Library)
```
Ai.WebUI.Database/
├── AppDbContext.cs
├── Entities/
│   ├── AppUser.cs
│   ├── Project.cs
│   ├── Chat.cs
│   └── Document.cs
└── Migrations/
```
- Содержит `AppDbContext`, все EF Core-сущности и миграции
- `Ai.WebUI` ссылается на этот проект через `<ProjectReference>`
- NuGet: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`

### Ai.WebUI.DataFormats (Class Library — существующий)
```
Ai.WebUI.DataFormats/
└── ... (существующая структура)
```
- Используется `DocumentService` из `Ai.WebUI` для извлечения текста из загружаемых файлов (PDF, TXT и др.)
- `Ai.WebUI` ссылается на этот проект через `<ProjectReference>`

### Ai.WebUI (Blazor Server)
```
Ai.WebUI/
├── Components/
│   ├── Pages/
│   │   ├── Auth/
│   │   │   ├── Login.razor
│   │   │   └── Register.razor
│   │   ├── Projects/
│   │   │   ├── ProjectList.razor      (главная страница после логина)
│   │   │   └── ProjectDetail.razor   (чаты внутри проекта)
│   │   └── Chat/
│   │       └── ChatPage.razor        (основная страница чата)
│   ├── Layout/
│   │   ├── MainLayout.razor          (sidebar + content area)
│   │   ├── NavMenu.razor             (проекты + чаты в sidebar)
│   │   └── ... (существующие)
│   └── Shared/
│       ├── MessageBubble.razor       (одно сообщение в чате)
│       ├── ChatInput.razor           (поле ввода + кнопка отправки)
│       ├── DocumentUpload.razor      (drag-and-drop загрузка файлов)
│       └── ModelSelector.razor       (выбор модели Ollama — список из GET /api/tags)
├── Services/
│   ├── AI/
│   │   ├── OllamaChatService.cs
│   │   └── ChatHistoryReducer.cs
│   └── DocumentService.cs            (использует Ai.WebUI.DataFormats для извлечения текста)
└── Infrastructure/
    └── ServiceExtensions.cs          (регистрация всех сервисов в DI)
```

---

## 6. UI/UX (по DESIGN.md)

### Общие правила
- Все цвета через CSS-переменные — никаких захардкоженных значений
- Тема (`dark`/`light`) хранится в `AppUser.Settings`, применяется через атрибут `data-theme` на `<html>`
- Ember Orange (`#FF6B2C`) — только CTA-кнопки, активная навигация, focus-кольца

### Навигация (Sidebar)
- 260px развёрнут / 64px свёрнут, переход 250ms
- Список проектов с Ember Orange для активного
- Badge с количеством чатов у каждого проекта
- Ghost-стиль для неактивных пунктов

### Чат
- **MessageBubble (user):** inset-карточка, выровнена вправо, Inter body
- **MessageBubble (assistant):** raised-карточка, текст в JetBrains Mono, выровнена влево
- **Стриминг:** токены добавляются в активный MessageBubble без перерисовки — typewriter без skeleton/spinner
- **ChatInput:** inset-поле ввода, accent-кнопка отправки (Ember Orange), disabled во время стриминга

### Документы
- Drag-and-drop зона: inset-контейнер с dashed-border, при drag-over border становится Ember Orange (`1px solid var(--color-accent)`)
- Прикреплённые документы — pill-теги с именем файла и кнопкой удаления

### Формы Auth (Login/Register)
- Карточка по центру экрана, raised-elevation
- Inset-поля ввода, focus-кольцо Ember Orange
- Accent-кнопка submit

---

## 7. NuGet-зависимости

**Ai.WebUI (Blazor Server):**
```xml
<PackageReference Include="Microsoft.SemanticKernel" Version="1.*" />
<PackageReference Include="Microsoft.SemanticKernel.Connectors.Ollama" Version="1.*" />
<ProjectReference Include="..\Ai.WebUI.Database\Ai.WebUI.Database.csproj" />
<ProjectReference Include="..\Ai.WebUI.DataFormats\Ai.WebUI.DataFormats.csproj" />
```

**Ai.WebUI.Database (Class Library):**
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.*" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.*" />
```

Версии — последние стабильные на момент реализации.

---

## 8. Вне скоупа (не реализуется сейчас)

- Агентный цикл (tool calling, плановщик) — SK ядро готово, добавляется позже
- Векторный поиск / RAG для документов
- Экспорт/импорт истории чатов
- Облачные провайдеры (OpenAI, Anthropic) — архитектура готова, коннекторы добавляются позже
- Мультипользовательский инвайт-флоу
