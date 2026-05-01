---
name: LogSystem
description: Централизованная система мониторинга логов микросервисов в реальном времени
colors:
  ember-orange: "#FF6B2C"
  ember-orange-light: "#FF8C42"
  bg-dark: "#2D2D35"
  panel-dark: "#363640"
  inset-dark: "#1E1E26"
  surface-dark: "#3A3A45"
  bg-light: "#F0F0F5"
  panel-light: "#FFFFFF"
  inset-light: "#E4E4EC"
  text-primary-dark: "#FFFFFF"
  text-primary-light: "#1A1A2E"
  text-secondary: "#8A8A9A"
  text-muted: "#7D7D8A"
  log-error: "#FF4D4D"
  log-warn: "#FFB02E"
  log-info: "#4DA6FF"
  log-debug: "#48C25C"
  status-online: "#4ADE80"
  status-offline: "#6B7280"
typography:
  title:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "1.5rem"
    fontWeight: 600
    lineHeight: 1.3
  headline:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "1.25rem"
    fontWeight: 500
    lineHeight: 1.4
  body:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "0.875rem"
    fontWeight: 400
    lineHeight: 1.5
  label:
    fontFamily: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
    fontSize: "0.75rem"
    fontWeight: 500
    lineHeight: 1.4
    letterSpacing: "0.02em"
  mono:
    fontFamily: "'JetBrains Mono', 'Fira Code', 'Cascadia Code', monospace"
    fontSize: "0.875rem"
    fontWeight: 400
    lineHeight: 1.6
rounded:
  sm: "4px"
  md: "8px"
  lg: "16px"
  xl: "24px"
  full: "9999px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "16px"
  lg: "24px"
  xl: "32px"
  2xl: "48px"
components:
  button-default:
    backgroundColor: "{colors.panel-dark}"
    textColor: "{colors.text-primary-dark}"
    rounded: "{rounded.md}"
    padding: "8px 16px"
  button-accent:
    backgroundColor: "{colors.ember-orange}"
    textColor: "#FFFFFF"
    rounded: "{rounded.md}"
    padding: "8px 16px"
  button-ghost:
    backgroundColor: "transparent"
    textColor: "{colors.text-secondary}"
    rounded: "{rounded.md}"
    padding: "8px 16px"
  button-ghost-hover:
    backgroundColor: "rgba(255, 107, 44, 0.12)"
    textColor: "{colors.text-primary-dark}"
    rounded: "{rounded.md}"
    padding: "8px 16px"
  input-default:
    backgroundColor: "{colors.inset-dark}"
    textColor: "{colors.text-primary-dark}"
    rounded: "{rounded.md}"
    padding: "0 16px"
    height: "36px"
  card:
    backgroundColor: "{colors.panel-dark}"
    rounded: "{rounded.lg}"
    padding: "24px"
---

# Design System: LogSystem

## 1. Overview

**Creative North Star: "The Precision Instrument"**

LogSystem — это профессиональный инструмент измерения, а не витрина. Как осциллограф или спектральный анализатор, система существует ради данных, которые через неё проходят. Визуальный язык сдержан намеренно: neumorphic-поверхности создают глубину без украшений, единственный хроматический акцент — Ember Orange — зарезервирован строго для действий и интерактивных элементов. Всё остальное — нейтрально.

Система поддерживает два полноценных визуальных режима, отражающих реальные рабочие контексты: тёмная тема для работы с потоком данных в условиях низкой освещённости; светлая — для аналитической работы и построения отчётов. Ни одна из тем не является "по умолчанию" — пользователь выбирает контекст. Оба режима сохраняют полную семантику и иерархию.

Система отвергает три паттерна: шаблонные Bootstrap/Material admin-панели с их серо-синей безликостью и нулевым характером; перегруженные дашборды, где каждый пиксель занят виджетами без иерархии; retro-терминальную эстетику с зелёным текстом на чёрном. LogSystem — точный инструмент, не театр данных.

**Key Characteristics:**
- Neumorphic elevation: глубина создаётся парными тенями, а не цветовыми границами
- Единственный акцент: Ember Orange используется строго для интерактивных элементов
- Семантические цвета: уровни логов читаются мгновенно по цвету
- Монопропорциональная типографика для технических значений и данных
- Две полноценные темы с идентичной семантикой

## 2. Colors: The Ember Palette

Монохромные нейтральные поверхности плюс один тёплый акцент и строгая семантическая палитра для уровней логов. Тёплый оттенок нейтралей (лёгкий пурпурный подтон) перекликается с Ember Orange, не конкурируя с ним.

### Primary
- **Ember Orange** (`#FF6B2C`): Единственный акцент во всей системе. CTA-кнопки, активные состояния навигации, hover ссылок, focus-кольца. Никогда — фон, декорация или информационный сигнал.
- **Ember Orange Light** (`#FF8C42`): Hover-состояние акцентных элементов. Конечная точка градиента в accent-кнопках (`135deg, #FF6B2C → #FF8C42`).

### Neutral (тёмная тема)
- **Deep Slate** (`#2D2D35`): Основной фон приложения. Базовая поверхность, от которой отсчитываются все уровни elevation.
- **Elevated Slate** (`#363640`): Панели, карточки, сайдбар. Поднятые поверхности (+5 lightness).
- **Mid Slate** (`#3A3A45`): Промежуточные поверхности, hover-подложки строк таблицы.
- **Sunken Void** (`#1E1E26`): Поля ввода, вложенные секции. Самая тёмная поверхность — уходит вниз.

### Neutral (светлая тема)
- **Cool Mist** (`#F0F0F5`): Основной фон в светлой теме. Лёгкий голубоватый подтон.
- **Pure Panel** (`#FFFFFF`): Карточки и основные панели.
- **Recessed** (`#E4E4EC`): Поля ввода и inset-элементы.

### Text
- **Stark White** (`#FFFFFF`): Основной текст в тёмной теме.
- **Deep Navy** (`#1A1A2E`): Основной текст в светлой теме.
- **Steel Mist** (`#8A8A9A`): Вторичный текст — метаданные, временные метки, подписи.
- **Faded Ash** (`#7D7D8A`): Приглушённый текст — плейсхолдеры, отключённые состояния.

### Semantic — Log Levels
- **Alert Red** (`#FF4D4D` dark / `#DC2626` light): FATAL и ERROR. Требует немедленного внимания.
- **Warning Amber** (`#FFB02E` dark / `#D97706` light): WARN. Требует внимания, не срочно.
- **Data Blue** (`#4DA6FF` dark / `#2563EB` light): INFO. Информационный поток.
- **Signal Green** (`#48C25C`): DEBUG и TRACE. Минимальный уровень — фоновый шум системы.

### Named Rules
**The One Voice Rule.** Ember Orange — единственный хроматический цвет на любом экране. Его редкость — это его смысл. Любое использование оранжевого сигнализирует: "здесь можно действовать". Если Orange появляется на 10% площади экрана — это уже слишком много.

**The Semantic Immutability Rule.** Alert Red, Warning Amber, Data Blue, Signal Green принадлежат исключительно уровням логов и статусам. Нельзя использовать эти цвета для брендинга, декора или произвольных состояний.

## 3. Typography

**Body/UI Font:** Inter (с фолбэком: -apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif)
**Code/Data Font:** JetBrains Mono (с фолбэком: Fira Code, Cascadia Code, monospace)

**Character:** Inter — нейтральный и читаемый при высокой плотности данных, идеален для таблиц и форм. JetBrains Mono — для технических значений, где моноширинность критична для точного чтения цифр и кодов.

### Hierarchy
- **Title** (semibold 600, 1.5rem, line-height 1.3): Заголовки страниц и крупных секций.
- **Headline** (medium 500, 1.25rem, line-height 1.4): Подзаголовки разделов, названия карточек метрик.
- **Body** (regular 400, 0.875rem, line-height 1.5): Основной текст UI, строки таблицы логов. В читаемых текстовых блоках — максимум 65-75ch.
- **Label** (medium 500, 0.8125rem, letter-spacing 0.02em): Метки полей, заголовки колонок. Иногда uppercase с letter-spacing для категорий.
- **Small Label** (medium 500, 0.75rem, letter-spacing 0.02em): Теги уровней логов, бейджи, подсказки.
- **Mono** (regular 400, 0.875rem, line-height 1.6): Сообщения логов, стектрейсы, trace ID, JSON payload. Всегда JetBrains Mono.

### Named Rules
**The Mono-for-Data Rule.** Любое значение, которое читается посимвольно — trace ID, stack trace, JSON, временная метка с миллисекундами, request GUID — рендерится JetBrains Mono. Никогда не смешивать Inter и JetBrains Mono для одного токена данных в одной строке без явного визуального разделения.

## 4. Elevation

Система использует **neumorphic elevation** — глубина создаётся парными тенями от одного воображаемого источника света сверху-слева. В тёмной теме: `rgba(0,0,0,0.4)` (тёмная) + `rgba(255,255,255,0.05)` (светлая). В светлой теме: `rgba(0,0,0,0.15)` + `rgba(255,255,255,0.8)`. Поверхности не имеют видимых границ — разделение создаётся исключительно тенями.

Три состояния поверхности образуют полный цикл взаимодействия:
- **Raised (выпуклая)**: панели, карточки, кнопки в покое — выступают.
- **Flat (плоская)**: hover-состояние кнопок — переходное.
- **Inset (вогнутая)**: поля ввода, нажатые кнопки, вложенные секции — уходят вглубь.

### Shadow Vocabulary
- **Raised** (`6px 6px 12px dark, -6px -6px 12px light`): Карточки и основные панели.
- **Raised-sm** (`3px 3px 6px dark, -3px -3px 6px light`): Кнопки в покое, небольшие элементы управления.
- **Flat** (`2px 2px 5px dark, -2px -2px 5px light`): Hover-состояние кнопок.
- **Inset** (`inset 4px 4px 8px dark, inset -4px -4px 8px light`): Поля ввода, контейнеры с данными.
- **Inset-sm** (`inset 2px 2px 4px dark, inset -2px -2px 4px light`): Небольшие inset-элементы, бейджи.
- **Pressed** (`inset 3px 3px 6px dark, inset -3px -3px 6px light`): Нажатое состояние кнопок.

### Named Rules
**The Paired Shadow Rule.** Neumorphic-тень всегда состоит из двух компонентов: тёмного и светлого. Одиночная тень нарушает иллюзию материала и выглядит как обычный drop-shadow. Никаких одиночных теней.

**The No-Border Rule.** Границы между поверхностями создаются тенями, не `border`. Видимая граница (`border: 1px solid`) допустима только как семантический сигнал (focus-ring, выбранный элемент навигации), но никогда — как разделитель соседних поверхностей.

## 5. Components

### Buttons
- **Shape:** Мягко скруглённые — 8px (md/lg), 4px (sm).
- **Default:** `panel-dark` фон, raised-sm тень, основной цвет текста. Hover — flat; active — pressed. Для вторичных действий.
- **Accent:** Градиент `135deg, #FF6B2C → #FF8C42`, белый текст, оранжевое свечение `0 2px 8px rgba(255,107,44,0.3)`. Hover усиливает свечение до `0 4px 12px rgba(255,107,44,0.4)`. Только для главных CTA.
- **Ghost:** Прозрачный фон, steel-mist текст. Hover — accent-bg подложка (orange 8-12%), текст становится основным. Для вторичных действий рядом с accent.
- **Danger:** Accent-стиль, но Alert Red (#FF4D4D). Только для необратимых деструктивных действий.
- **Loading state:** Spinner заменяет иконку, `aria-busy="true"`, opacity не меняется.
- **Disabled:** Opacity 0.5, cursor not-allowed. Тень сохраняется.

### Inputs / Fields
- **Style:** Inset-sm тень, `inset-dark` / `inset-light` фон, radius-md (8px), padding `0 16px`, min-height 36px, `border: 1px solid transparent`.
- **Focus:** Граница становится `1px solid var(--color-accent)` — единственный случай, где граница несёт позиционный смысл.
- **Icons:** Иконка поиска/фильтра в левом слоте (16px, stroke-based SVG, steel-mist). Кнопка очистки в правом — появляется только при наличии значения.
- **Placeholder:** Faded Ash (#7D7D8A). Никогда не заменяет label полностью.
- **Disabled:** Opacity 0.5, pointer-events none.

### Cards / Containers
- **Corner Style:** Гладко скруглённые (16px — radius-lg).
- **Background:** `panel-dark` / `panel-light`.
- **Shadow:** Raised (6px) — основные карточки. Card-inset для вложенных секций данных.
- **Border:** Нет (elevation через тени).
- **Internal Padding:** lg (24px).
- **Nested cards:** Запрещены. Используйте card-inset секции или разделительные линии внутри карточки.

### Navigation (Sidebar)
- **Structure:** Постоянный левый сайдбар, 260px развёрнут / 64px свёрнут. Переход: 250ms ease.
- **Items (покой):** Ghost-стиль — прозрачный фон, steel-mist текст, иконка 20-24px.
- **Items (active):** accent-bg подложка (8-12%), ember-orange текст и иконка.
- **Error badges:** Alert Red бейдж с количеством ошибок у каждого сервиса в реальном времени.
- **Typography:** 0.875rem, medium (500).

### Log Row (Signature Component)
Центральный компонент системы — строка таблицы логов:
- **Height:** 44px фиксированная (обязательно для виртуального скролла с тысячами строк).
- **Level badge:** Pill-тег с цветом уровня и тонированным фоном (8-12% opacity).
- **Timestamp:** JetBrains Mono, steel-mist, фиксированная ширина — всегда читаем параллельно с соседними строками.
- **Service:** Label-стиль, steel-mist.
- **Message:** Inter body, основной текст, truncated с ellipsis — занимает всё оставшееся пространство.
- **Expanded state:** Inline-раскрытие под строкой, inset-фон, JetBrains Mono для payload и stacktrace, подсветка синтаксиса.

## 6. Do's and Don'ts

### Do:
- **Do** использовать Ember Orange (#FF6B2C) только для интерактивных элементов: primary-кнопок, активных состояний навигации, focus-колец.
- **Do** рендерить все технические значения (trace ID, timestamp с миллисекундами, JSON, stack trace) в JetBrains Mono.
- **Do** применять парные neumorphic-тени (тёмная + светлая). Одиночная тень нарушает систему.
- **Do** поддерживать оба визуальных режима (dark/light). Каждый новый компонент обязан использовать CSS-переменные, а не захардкоженные цвета.
- **Do** соблюдать WCAG AA: минимум 4.5:1 для обычного текста, 3:1 для крупного текста и интерактивных элементов.
- **Do** применять семантические цвета уровней логов строго по назначению: Alert Red только для ERROR/FATAL, Warning Amber только для WARN.
- **Do** использовать `prefers-reduced-motion` — отключать все `transition` и `animation`, сохраняя функциональность.

### Don't:
- **Don't** использовать `border-left` или `border-right` больше 1px как цветной акцент на карточках или строках. Никогда. Это прямое нарушение neumorphic-системы.
- **Don't** воспроизводить Bootstrap/Material шаблонный подход: серо-синие панели без характера, `#1976D2` primary, скругления везде одинаковые.
- **Don't** перегружать экраны виджетами без иерархии — нарушение принципа "Данные прежде всего" из PRODUCT.md.
- **Don't** применять `background-clip: text` с градиентом. Запрещено.
- **Don't** использовать glassmorphism (backdrop-filter + rgba blur) как декорацию. Система neumorphic, не glassmorphic.
- **Don't** использовать Signal Green (#48C25C) для чего-либо кроме DEBUG/TRACE уровней и online-статуса.
- **Don't** создавать вложенные карточки (карточка внутри карточки с теми же raised-тенями). Используйте card-inset.
- **Don't** добавлять анимированные иллюстрации, яркие градиентные фоны, emoji в UI, скругления >24px на контейнерах — это consumer-style, несовместимый с профессиональным инструментом.
