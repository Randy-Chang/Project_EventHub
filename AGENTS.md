# AGENTS.md

本文件定義此 Repository 中 AI Coding Agent（包含 Codex）進行分析、修改、重構與新增功能時必須遵循的開發規範。

除非使用者在當次任務中明確指定不同做法，否則以下規則皆視為預設要求。

---

# 1. 基本原則

修改程式碼前，先理解目前 Solution、Project、Namespace、Dependency 與既有設計方式。

不得因為新增單一功能，就任意重構與該功能無關的大量既有程式碼。

優先採取：

> 最小必要修改（Minimal Necessary Change）

如果既有架構可以合理延伸，應優先沿用，而不是重新建立第二套平行架構。

---

# 2. 文字檔與換行格式

修改後請把所有本次碰過的文字檔統一成：

**Windows CRLF**

包括但不限於：

- `.cs`
- `.csproj`
- `.sln`
- `.json`
- `.xml`
- `.config`
- `.md`
- `.html`
- `.css`
- `.js`
- `.ts`
- `.razor`
- `.props`
- `.targets`
- `.editorconfig`

修改完成後必須檢查：

- 不得存在 CRLF / LF 混用。
- 本次有修改的文字檔必須統一為 CRLF。
- 不要因換行格式轉換造成與任務無關的整份檔案巨大 Diff。

若 Repository 已有明確且衝突的格式規範，應先指出衝突，再依使用者要求處理。

---

# 3. 架構與 SOLID

修改時請遵循現有專案架構與 SOLID 原則。

優先維持：

- Single Responsibility Principle
- Open/Closed Principle
- Liskov Substitution Principle
- Interface Segregation Principle
- Dependency Inversion Principle
- Low Coupling
- High Cohesion
- Separation of Concerns

尤其注意：

## Single Responsibility

Class、Service、Form、Controller、Hub 各自只負責合理且清楚的責任。

避免建立同時處理：

- UI
- Database
- Business Logic
- Network
- Logging

的 God Class。

## Dependency Inversion

高階 Business Logic 不應直接依賴具體 Infrastructure Implementation。

在確實存在替換、測試或邊界需求時，優先透過 Interface / Abstraction 建立 Dependency。

但不要為每一個只有單一用途的小 Class 都建立 Interface。

Interface 必須有實際架構價值。

## Interface Segregation

Interface 應維持小而明確。

避免建立：

`IEverythingService`

這類包含大量不相關方法的 Interface。

---

# 4. 避免過度設計

不得為了「看起來符合 Clean Architecture」或「套用 Design Pattern」而增加不必要的抽象層。

避免在沒有需求時主動加入：

- Repository Pattern 套 Repository Pattern
- Generic Repository
- Unit of Work wrapper
- CQRS Framework
- Mediator Framework
- Event Sourcing
- Microservices
- Message Broker
- Redis
- Distributed Cache
- Kubernetes
- 複雜 Plugin System
- 過度 Generic 化
- 無實際替換需求的 Interface

每新增一個 abstraction，都應能回答：

> 它目前解決了什麼實際問題？

如果無法回答，優先採用更直接的設計。

---

# 5. Domain Logic

Business Rule 應放在合理的 Domain 或 Application Layer。

不得把主要 Business Logic 寫在：

- WinForms Form
- UserControl
- ASP.NET Controller
- SignalR Hub
- EF Core DbContext
- Display UI

Controller 與 Hub 的責任應偏向：

- Input validation
- Request routing
- Authentication / connection context
- 呼叫 Application Layer
- 回傳結果
- 發送 SignalR Event

而不是直接實作完整 Use Case。

---

# 6. Application Layer

Application Layer 負責 Use Case orchestration。

例如：

- JoinEvent
- StartQuiz
- SubmitAnswer
- CloseQuestion
- CalculateScore
- StartLuckyDraw
- ConfirmWinner
- UploadPhoto

Application Service 不應直接依賴 WinForms Control 或 ASP.NET-specific UI Type。

---

# 7. Infrastructure Layer

Infrastructure 負責外部技術細節，例如：

- EF Core
- SQLite
- File Storage
- QR Code generator
- System Clock implementation
- Random Number Generator implementation
- Future Printer implementation

Infrastructure 不應反向控制 Domain。

---

# 8. Server Authoritative

EventHub 為多人即時活動系統。

涉及公平性與共用狀態時，Server 必須是唯一可信來源。

Server Authoritative 的資料包括：

- Event State
- Game State
- Current Question
- Question Start Time
- Question Close Time
- Answer
- Correctness
- Score
- Ranking
- Lucky Draw Result

Client 不得自行決定最終結果。

Client 顯示倒數可以使用 Local Timer，但真正截止判定必須以 Server Time 為準。

---

# 9. 時間處理

Business Logic 不應在各處直接呼叫：

`DateTime.Now`

或：

`DateTime.UtcNow`

若時間會影響：

- Quiz deadline
- Answer timing
- Score
- Activity scheduling
- Audit Log

應集中透過可替換 Clock abstraction。

內部時間優先保存 UTC。

顯示層再轉換為 Local Time。

---

# 10. 隨機數與抽獎

Lucky Draw 屬於重要 Business Logic。

抽獎不得直接在 UI 中呼叫 `Random` 後決定結果。

Random selection 應封裝成可測試的 abstraction。

抽獎流程必須保證：

- Eligible Participant 計算正確
- 避免同次 Draw 重複執行
- 支援排除已中獎 Participant
- 重抽有明確狀態
- 結果有 Audit Log
- Server 為最終結果來源

不得只靠 Client 防止重複抽獎。

---

# 11. Quiz

Quiz 必須明確區分：

- Draft
- Waiting
- Open
- Closed
- Revealed
- Completed

或其他具有相同語意的 State Model。

不要使用大量散落的 Boolean，例如：

```csharp
IsStarted
IsClosed
IsAnswerShown
IsFinished
```

造成互相矛盾的狀態組合。

應優先使用明確 State。

---

# 12. Answer Submission

同一位 Participant 對同一題原則上只能存在一筆有效 Answer。

必須考慮：

- Button double click
- HTTP retry
- Browser reconnect
- SignalR reconnect
- Network retry
- User refresh

不要只靠 Frontend disable button 避免重複提交。

Server 必須有實際資料一致性保護。

---

# 13. SignalR

SignalR 用於即時狀態同步。

Hub 不應包含大型 Business Logic。

Hub Method 應呼叫 Application Service。

SignalR Event 名稱應語意清楚。

例如：

- ParticipantJoined
- QuestionStarted
- QuestionClosed
- AnswerCountUpdated
- AnswerRevealed
- LeaderboardUpdated
- PollStarted
- PollClosed
- LuckyDrawStarted
- LuckyDrawCompleted
- PhotoAdded

避免使用模糊名稱，例如：

- Update
- Refresh
- Change
- DataUpdated

除非上下文非常明確。

---

# 14. REST API

REST API 應用於：

- 初始資料取得
- CRUD
- Upload
- 查詢
- 重連後 State Recovery

SignalR 不應取代所有 API。

架構需明確區分：

> REST 負責 State Retrieval / Command  
> SignalR 負責 Realtime Notification

---

# 15. 斷線與恢復

不要假設 Client 永遠在線。

必須考慮：

- Browser refresh
- Mobile screen lock
- Wi-Fi 短暫斷線
- SignalR disconnect
- Host restart
- Display restart

Client reconnect 後，應能重新取得 Server Current State，而不是依賴錯過的 SignalR Event 重建狀態。

---

# 16. Server Restart

核心活動資料不得只保存在 Memory。

至少以下資料必須能持久化：

- Event
- Participant
- Quiz
- Question
- Answer
- Score
- Lucky Draw Result
- Prize
- Photo Metadata
- Audit Log

Server Restart 後應能恢復到合理狀態。

Transient connection information 可以只存在 Memory。

---

# 17. Database

第一版使用 SQLite。

EF Core Entity Configuration 應集中管理。

避免將大量 Mapping 全塞在：

`OnModelCreating()`

如 Mapping 開始變多，應使用：

`IEntityTypeConfiguration<T>`

建立清楚的 Configuration。

Database Constraint 應補足重要的資料一致性規則。

不要只依靠 Application Code。

---

# 18. WinForms

WinForms 必須使用 Visual Studio Designer 標準模式。

## WinForms UI 實作規則

所有使用 WinForms 的顯示與操作畫面，必須使用 Visual Studio WinForms Designer 標準模式建立。

- 不得將整個視窗或全部固定控制項改成由程式碼動態產生。
- 固定控制項的宣告、初始化、Layout、`Dock` 與 `Anchor` 設定必須放在 `.Designer.cs`。
- UI 邏輯、Event Handling、資料載入、網路操作與畫面狀態更新必須放在 `.cs`。
- 只有真正由資料筆數決定的內容，例如選項、排行榜列或統計列，才可以合理地動態 Render。
- 修改 Designer Layout 後，必須確認常用解析度與 Windows DPI 下不會重疊，且主要操作按鈕可被使用者操作。

控制項宣告與 Designer Generated Layout 放在：

`.Designer.cs`

UI Logic、Event Handler 與 Presentation Logic 放在：

`.cs`

不得將整個 UI 以 Runtime Dynamic Controls 的方式重新建立。

除非該 UI 本身具有真正動態資料需求，否則不要用程式碼取代 Designer。

Form 不應直接操作 Database。

Form 不應直接包含大型 Domain Logic。

---

# 19. UI Thread

WinForms 更新 UI 時必須注意 UI Thread。

不要因 SignalR Callback、Background Task 或 async operation 直接跨執行緒更新 Control。

應使用正確的：

- Invoke
- BeginInvoke
- SynchronizationContext

或現有專案統一方式。

避免散落不一致的跨執行緒處理。

---

# 20. Async / Await

Network、Database 與 File I/O 優先使用 async API。

避免：

- `.Result`
- `.Wait()`

造成 Deadlock 或阻塞 UI Thread。

WinForms Event Handler 可以使用 `async void`，但一般 Business Method 不應使用 `async void`。

---

# 21. CancellationToken

合理的長時間操作與 I/O API 應支援 `CancellationToken`。

不要為所有 trivial method 強制加入 CancellationToken。

---

# 22. Exception Handling

不要使用：

```csharp
catch (Exception)
{
}
```

吞掉 Exception。

Exception 應：

- 被合理處理
- 被 Log
- 或向上傳遞

UI 可以顯示 User-friendly message，但完整 Exception 應保留於 Log。

---

# 23. Logging

重要操作必須具備適當 Logging。

至少包含：

- Server startup
- Participant join
- Game start
- Question start / close
- Lucky draw
- Winner confirmation
- Unexpected exception
- Database failure
- SignalR connection issue

Log 不得保存不必要的敏感資料。

---

# 24. 命名

使用有語意的英文名稱。

Class 使用 PascalCase。

Method 使用 PascalCase。

Parameter / Local variable 使用 camelCase。

Boolean 命名優先：

- Is
- Has
- Can
- Should

例如：

`IsEligibleForLuckyDraw`

優於：

`CheckLuckyDraw`

如果 method 是執行檢查並回傳結果，名稱應表達其實際行為。

---

# 25. Enum

Enum 名稱應描述狀態或種類。

例如：

```csharp
public enum QuizSessionState
{
    Waiting,
    Open,
    Closed,
    Revealed,
    Completed
}
```

避免 Magic Number 或 Magic String 表示狀態。

---

# 26. Null

優先透過資料模型與流程設計降低 Null 的可能性。

不要大量使用：

`!`

壓制 Nullable Warning。

若使用 Null-forgiving operator，必須有合理理由。

---

# 27. Collection 與 LINQ

LINQ 可以使用，但優先維持可讀性。

避免過長、多層 Nested LINQ 造成 Business Rule 難以閱讀。

當流程本身有明確步驟時，一般 imperative code 可能更清楚。

---

# 28. Comments

Comment 用於說明：

- Why
- Constraint
- Non-obvious business rule
- Technical limitation

不要用 Comment 重複描述程式碼本身已清楚表達的內容。

Public API 或重要 Class 可使用 XML Documentation。

---

# 29. Tests

Domain Rule 與 Application Rule 應優先具備 Unit Test。

特別是：

- Quiz scoring
- Deadline validation
- Duplicate answer
- Ranking
- Lucky draw eligibility
- Already-won exclusion
- Redraw
- State transition

UI Layout 不需要為了追求 coverage 強制加入 Unit Test。

---

# 30. 修改既有程式碼

修改前先搜尋：

- 是否已有相同 Service
- 是否已有 Utility
- 是否已有 DTO
- 是否已有 Interface
- 是否已有 Extension Method
- 是否已有共用 UI Pattern

不要重複建立已有能力。

---

# 31. Refactoring

如果發現既有程式碼有問題，但與目前任務無直接關係：

不要順手進行大範圍重構。

可以在完成任務後指出：

- 問題
- 風險
- 建議

等待後續任務再處理。

---

# 32. Security

所有來自 Client 的資料視為不可信任。

Server 必須驗證：

- Event ID
- Participant ID
- Question ID
- Option ID
- Current State
- Deadline
- Permission

不要因為 UI 沒有提供某個操作，就假設 Client 無法呼叫該 API。

---

# 33. File Upload

Photo Upload 必須驗證：

- File size
- Content type
- Supported format
- File name
- Storage path

不得直接使用 Client 提供的 File Name 作為實際 Server Path。

必須防止 Path Traversal。

---

# 34. 效能

第一版目標以一般公司尾牙活動規模為主。

不要過早進行 Distributed System Optimization。

但避免明顯問題，例如：

- 每位 Client 每秒多次 Poll Database
- 每個 SignalR Broadcast 都重新讀取整張 Table
- 排行榜產生 N+1 Query
- Image 直接以超大 Binary 存進不適合的資料流程

先保持簡單且合理。

---

# 35. NuGet 套件

新增 NuGet Package 前，先確認：

1. .NET 內建能力是否已足夠。
2. 現有專案是否已有相同用途 Package。
3. Package 是否仍持續維護。
4. License 是否適合專案使用。
5. 是否真的值得增加 Dependency。

不要為 trivial functionality 引入大型 Library。

---

# 36. 完成修改後

每次完成程式修改後，至少檢查：

1. Solution 是否可以 Build。
2. 是否出現新的 Compiler Warning。
3. 是否出現新的 Nullable Warning。
4. Test 是否通過。
5. 本次修改的文字檔是否全部為 CRLF。
6. 是否存在混用換行。
7. 是否產生無關的大量 Diff。
8. 是否有 Debug Code 遺留。
9. 是否有未使用的 using。
10. 是否有 Temporary File 被加入 Repository。

若某項無法執行，必須明確說明原因。

---

# 37. 回報格式

完成任務後，回報至少包含：

## Changed

說明修改了哪些內容。

## Architecture

如果涉及架構調整，說明責任如何分配以及原因。

## Verification

說明：

- Build 結果
- Test 結果
- CRLF 檢查結果

## Remaining Risks

只有存在實際風險時才列出。

不要為了填格式而虛構問題。

---

# 38. 最終原則

本專案追求：

> Simple, Clear, Testable, Maintainable

優先：

- 清楚
- 穩定
- 可維護
- 可測試
- 易於理解

而不是：

- Pattern 數量
- Interface 數量
- Layer 數量
- 技術複雜度

當簡單方案已足以滿足目前需求時，使用簡單方案。

---

# 39. 投影顯示畫面設計規則

本專案的 Display 畫面主要用於活動現場的投影幕、電視或大型顯示器。設計與修改 Display 相關功能時，必須遵守以下規則。

## 基準顯示規格

- 主要設計基準為 1920 × 1080。
- 畫面比例以 16:9 為主要目標。
- 現場建議使用 Windows 顯示縮放 100%。
- Web Display 現場建議使用瀏覽器縮放 100%，並必須支援瀏覽器全螢幕顯示。
- WinForms Display 必須支援無邊框全螢幕顯示，並允許安全地離開全螢幕。
- 不得假設所有設備都一定是 1920 × 1080。
- 至少必須相容以下解析度：
  - 1366 × 768
  - 1600 × 900
  - 1920 × 1080
  - 2560 × 1440
  - 3840 × 2160

## 響應式畫面規則

- 不得將整個畫面依賴固定像素座標排版。
- 不得只針對單一解析度設計。
- Web Display 優先使用：
  - CSS Grid
  - Flexbox
  - 百分比尺寸
  - `min()`
  - `max()`
  - `clamp()`
  - `aspect-ratio`
  - `object-fit`
  - `vw`
  - `vh`
  - `vmin`
- WinForms Display 應使用具有相同目的的 Designer 標準機制：
  - `Dock`
  - `Anchor`
  - `TableLayoutPanel`
  - `FlowLayoutPanel`
  - 百分比 `ColumnStyle` / `RowStyle`
  - 正確的 `AutoScaleMode` 與 `AutoScaleDimensions`
  - `PictureBoxSizeMode.Zoom` 等保持比例的圖片顯示方式
- 可以針對 Logo、QR Code 等必要元素設定合理的最小與最大尺寸。
- 重要內容必須保留安全邊界，不得緊貼畫面邊緣。
- 主要內容至少保留畫面寬高約 3%～5% 的安全邊界。
- 不得因解析度或 DPI 改變造成：
  - 元素重疊
  - 文字截斷
  - 按鈕超出畫面
  - QR Code 變形
  - 圖片比例失真
  - 頁面或畫面出現不必要的捲軸

## 照片顯示規則

- 照片必須保持原始長寬比例。
- 禁止直接拉伸照片填滿容器。
- Web 照片牆縮圖原則上使用：

  ```css
  object-fit: cover;
  ```

- WinForms 照片牆應採用等價的等比例裁切或縮放策略，不得使用會拉伸原圖的 `PictureBoxSizeMode.StretchImage`。

## 驗證要求

- Display 版面異動完成後，必須至少針對 1366 × 768 與 1920 × 1080 進行檢查。
- 能取得對應設備時，應再驗證 1600 × 900、2560 × 1440 與 3840 × 2160。
- 若未實際執行某個解析度、DPI、全螢幕或第二螢幕測試，完成回報必須明確標示 `Not Executed`，不得僅依程式碼推定為通過。
