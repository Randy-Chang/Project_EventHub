# EventHub 第一版架構提案

## 1. 架構摘要

EventHub 第一版採用 **Modular Monolith（模組化單體）**：一個 ASP.NET Core Server 承擔 REST、SignalR、Guest Web 靜態內容與 SQLite 存取；Host 是 WinForms Client；Display 是只讀的 Browser Client。這讓現場只需啟動一個 Server Process、開放一個 LAN Port，仍保留未來將相同 Server 部署到 Cloud 的能力。

核心原則：

- Server Authoritative：截止時間、答案、分數、排名與抽獎結果只由 Server 決定。
- REST 負責 Command、CRUD、初始狀態與斷線恢復；SignalR 只負責即時通知。
- SQLite 保存可恢復的活動狀態；SignalR Connection、線上狀態等短暫資料留在 Memory。
- 先維持單一部署單元，不加入 Message Broker、Redis、Microservices、CQRS Framework 或 Event Sourcing。
- 使用明確的小型 Application Service 與 Repository Boundary，不建立 Generic Repository 或額外 Unit of Work Wrapper。

```text
Guest Browser ── HTTPS/HTTP + SignalR ──┐
                                        │
Host WinForms ─ HTTP + SignalR ───── EventHub.Server
                                        │
Display Browser ─ HTTP + SignalR ───────┘
                                        │
                              Application / Domain
                                        │
                              Infrastructure / SQLite
```

LAN 第一版可使用 HTTP；若活動資料包含敏感資訊，正式部署應配置受信任的區域憑證或 Reverse Proxy。Cloud 部署時可直接改用 HTTPS，Client Contract 不需改變。

## 2. Solution 與 Project 責任

```text
Project_EventHub.sln
src/
├─ EventHub.Domain
├─ EventHub.Application
├─ EventHub.Infrastructure
├─ EventHub.Server
├─ EventHub.Web
├─ EventHub.Host
└─ EventHub.Display                 # Display milestone 才建立實作
tests/
├─ EventHub.Domain.Tests
└─ EventHub.Application.Tests
```

### EventHub.Domain

包含 Aggregate、Entity、Value Object、Enum、Domain Rule 與 Domain Exception。不得依賴 EF Core、ASP.NET Core、SignalR 或 WinForms。

### EventHub.Application

包含 Use Case orchestration、Command/Result DTO、Repository Port、Clock/Random/File Storage 等必要 Port，以及計分、排名、抽獎與狀態恢復服務。只依賴 Domain。

### EventHub.Infrastructure

實作 EF Core SQLite、Entity Configuration、Migration、Photo File Storage、Cryptographic Token、Random Selection 與其他外部技術細節。依賴 Application 與 Domain。

### EventHub.Server

Composition Root 與唯一 Backend Process。負責 REST Endpoint、SignalR Hub、輸入驗證、Host/Guest Credential 轉換、靜態檔案供應、Log、健康檢查及啟動 Recovery。Controller/Endpoint 與 Hub 不放 Business Rule。

### EventHub.Web

Guest Mobile Web 的 Razor Class Library／靜態資產。透過同一個 Server Origin 提供，避免額外 Process、CORS 與 LAN Port。只使用 REST/SignalR Contract，不直接存取 Infrastructure。

### EventHub.Host

WinForms Host Console。使用 `.Designer.cs` 保存 Layout，`.cs` 保存 Event Handler 與 Presentation Logic。只透過 REST/SignalR 操作 Server，不直接讀取 SQLite。

### EventHub.Display

大螢幕 Browser UI。只讀取 Server State 與接收通知，不具備管理 Command。待 Display milestone 再建立，避免空殼專案造成假進度。

### Tests

Domain Tests 驗證狀態轉換與純規則；Application Tests 以 Fake Port 驗證 Use Case、Idempotency、Deadline、抽獎與 Recovery orchestration。Infrastructure Integration Test 待資料庫行為增加時再加入。

## 3. Domain Model

### Aggregate Boundary

- `Event`：活動生命週期、Join Policy 與活動層級設定。
- `Participant`：單一活動內的參與身份、報到、分數摘要與中獎摘要。
- `Quiz`：題庫定義，包含 `Question` 與 `QuestionOption`。
- `QuizSession`：某次 Quiz 執行狀態與 Current Question；`AnswerSubmission` 以唯一 Constraint 保護。
- `Poll` / `PollSession`：投票定義、執行狀態與 `PollVote`。
- `Prize` / `LuckyDrawSession`：獎項、候選規則、抽取、確認、重抽與結果。
- `Photo`：Metadata、Moderation State 與 Storage Key；Binary 不放入一般資料表。
- `AuditLogEntry`：不可變的重要操作紀錄。

大型集合（例如所有 Answer、所有 Participant）不掛成永遠載入的 Navigation Collection；由 Application Query 直接查詢，避免 Aggregate 無限制膨脹。

## 4. Entity、Value Object 與 Enum

主要 Entity：

- `Event(Id, Name, EventDate, State, IsJoinOpen, HostCredentialHash)`
- `Participant(Id, EventId, Name, Nickname, EmployeeNumber, Department, TableNumber, IsCheckedIn, Score, HasWon, SessionCredentialHash, LastSeenAtUtc)`
- `Quiz(Id, EventId, Title)`
- `Question(Id, QuizId, Prompt, ImageStorageKey?, Duration, BaseScore, SpeedBonusMax, Order)`
- `QuestionOption(Id, QuestionId, Label, Text, IsCorrect)`
- `QuizSession(Id, EventId, QuizId, State, CurrentQuestionId?, StartedAtUtc?, ClosesAtUtc?)`
- `AnswerSubmission(Id, QuizSessionId, QuestionId, ParticipantId, OptionId, SubmittedAtUtc, IsCorrect, AwardedScore)`
- `Poll`, `PollOption`, `PollSession`, `PollVote`
- `Prize`, `LuckyDrawSession`, `LuckyDrawResult`
- `Photo(Id, EventId, ParticipantId, StorageKey, OriginalFileName, ContentType, Size, State, UploadedAtUtc)`
- `AuditLogEntry(Id, EventId, ActorType, ActorId?, Action, EntityType, EntityId?, OccurredAtUtc, DetailsJson)`

Value Object：

- `EventName`
- `ParticipantDisplayName`
- `QuestionDuration`
- `Score`（必要時允許加總但不允許負值）
- `UtcTimeRange`
- `StorageKey`
- `IdempotencyKey`

主要 Enum：

- `EventState`: `Draft`, `Ready`, `Running`, `Ended`, `Cancelled`
- `QuizSessionState`: `Waiting`, `Open`, `Closed`, `Revealed`, `Completed`
- `PollSessionState`: `Waiting`, `Open`, `Closed`, `Completed`
- `LuckyDrawState`: `Ready`, `Drawing`, `Drawn`, `Confirmed`, `Cancelled`
- `PhotoModerationState`: `Pending`, `Visible`, `Hidden`

第一個 Slice 只實作實際使用到的 `Event` 與 `Participant` 欄位；其餘為後續演進目標，不先建立無行為的貧血 Class。

## 5. Application Service

- `EventService`：建立活動、開關加入、開始、結束、取得目前狀態。
- `ParticipantService`：加入或恢復 Participant、驗證 Session、取得名單與個人狀態。
- `ParticipantPresenceService`：Connection 上下線、線上 Snapshot；持久身份與 Connection 分離。
- `QuizAuthoringService`：題目與選項 CRUD。
- `QuizSessionService`：開始題目、關閉、公布、完成與 Recovery。
- `AnswerSubmissionService`：驗證狀態、Server Deadline、唯一作答與 Idempotency。
- `ScoreCalculator`：`BaseCorrectScore + SpeedBonus` 的純函式規則。
- `LeaderboardService`：個人排名；以 `LeaderboardGrouping` 預留 Department/Table 查詢維度。
- `PollService`：開關投票、提交唯一 Vote、統計結果。
- `LuckyDrawService`：建立 Draw Attempt、計算 Eligible Set、抽取、確認與重抽。
- `PhotoService`：驗證 Upload、寫入 File Storage、Moderation 與查詢輪播清單。
- `RecoveryService`：Server Startup 時校正未完成 Session。

所有影響時間的服務注入 .NET `TimeProvider`；抽獎注入 `IRandomSelector`。只有確實跨技術邊界或需要測試替換時才建立 Interface。

## 6. SignalR Hub 邊界

建議一個 `EventHub`，依 Event 加入 Group：

- `event:{eventId}:guests`
- `event:{eventId}:hosts`
- `event:{eventId}:displays`
- 必要時 `participant:{participantId}`

Hub 僅負責：驗證 Connection Credential、呼叫 Application Service、Group routing、將 Application Result 發送為 typed event。Hub 不計分、不判斷 Deadline、不執行抽獎。

Server Event：

- `ParticipantJoined`
- `ParticipantPresenceChanged`
- `EventStateChanged`
- `QuestionStarted`
- `AnswerCountUpdated`
- `QuestionClosed`
- `AnswerRevealed`
- `LeaderboardUpdated`
- `PollStarted`, `PollResultUpdated`, `PollClosed`
- `LuckyDrawStarted`, `LuckyDrawCompleted`
- `PhotoAdded`, `PhotoVisibilityChanged`

SignalR Event 可能遺失或重送，Client 只把它當作「狀態已改變」通知；重連後一定呼叫 REST Snapshot，不以 Event replay 重建狀態。

## 7. REST API Contract

API 以 `/api/v1` 版本化。第一個 Slice 實作標記為 `V1 Slice` 的 Contract。

| Method | Route | 用途 |
|---|---|---|
| `POST` | `/api/v1/events` | 建立活動，回傳 Event 與一次性 Host Token（V1 Slice） |
| `GET` | `/api/v1/events/{eventId}` | Guest/Display 初始 Event State（V1 Slice） |
| `POST` | `/api/v1/events/{eventId}/participants/join` | 新加入或以 Session Token 恢復身份（V1 Slice） |
| `GET` | `/api/v1/events/{eventId}/participants/me` | Guest 恢復個人狀態 |
| `GET` | `/api/v1/events/{eventId}/participants` | Host 取得含線上狀態的名單（V1 Slice） |
| `POST` | `/api/v1/events/{eventId}/join-policy` | Host 開關加入 |
| `POST` | `/api/v1/events/{eventId}/state` | Host 執行合法活動狀態轉換 |
| `GET` | `/api/v1/events/{eventId}/state` | Client reconnect snapshot |
| `POST` | `/api/v1/quiz-sessions/{id}/questions/{questionId}/start` | Host 開始題目 |
| `POST` | `/api/v1/quiz-sessions/{id}/answers` | Guest 提交答案，需 Idempotency Key |
| `POST` | `/api/v1/quiz-sessions/{id}/questions/current/close` | Host 關閉作答 |
| `POST` | `/api/v1/quiz-sessions/{id}/questions/current/reveal` | Host 公布答案 |
| `GET` | `/api/v1/events/{eventId}/leaderboard?top=10&grouping=individual` | 排行榜 |
| `POST` | `/api/v1/poll-sessions/{id}/votes` | 提交投票 |
| `POST` | `/api/v1/prizes/{prizeId}/draw-attempts` | 建立一次抽獎 Attempt，需 Idempotency Key |
| `POST` | `/api/v1/draw-attempts/{id}/confirm` | 確認中獎 |
| `POST` | `/api/v1/draw-attempts/{id}/redraw` | 取消本次候選並重抽 |
| `POST` | `/api/v1/events/{eventId}/photos` | Multipart 上傳照片 |
| `POST` | `/api/v1/photos/{photoId}/visibility` | Host 隱藏／顯示 |

Host Endpoint 使用 `X-Host-Token`；Guest Endpoint 使用 `X-Participant-Token` 或 SignalR Query Credential。Production/Cloud milestone 應升級為標準 Authentication Scheme，不把 Token 寫入 Log 或 SignalR Event。

## 8. Quiz State Machine

```text
Waiting ── StartQuestion ──> Open ── Close/Deadline ──> Closed
   ^                         │                         │
   │                         └── Deadline 由 Server ──┘
   │                                                   │
   └──── NextQuestion <── Revealed <── RevealAnswer ───┘
                           │
                           └── no next question ──> Completed
```

規則：

- `Open` 時 Server 記錄 `StartedAtUtc` 與 `ClosesAtUtc`。
- Answer 以 Server 收件時間判斷，Client Timer 只用於顯示。
- `(QuizSessionId, QuestionId, ParticipantId)` 建立唯一索引；重試同一 Idempotency Key 回傳第一次結果，不重複計分。
- `Close`、`Reveal`、`Complete` 使用合法狀態轉換與 Concurrency Token，Host 連點不產生第二次效果。
- Image Quiz 只是 `Question.ImageStorageKey` 有值的同一種 Question，不複製 Session、Answer 或 Score 流程。
- `ScoreCalculator` 輸入 Base Score、Max Bonus、Started/Closed/Submitted 時間，輸出不可變的計分明細；錯誤答案為零分。

## 9. Lucky Draw 流程

```text
Host Command + IdempotencyKey
  → 驗證 Prize 尚有名額
  → 建立 DrawAttempt (Ready)
  → 在 Transaction 內鎖定邏輯版本
  → 計算 Eligible Participants
  → Server IRandomSelector 選取候選人
  → 保存 Candidate + State=Drawn + Audit Log
  → Broadcast LuckyDrawCompleted
  → Host Confirm 或 Redraw
```

Eligibility Query 同時套用：活動、已報到、部門/桌次等規則、是否排除已中獎、未被本次已確認結果使用。`DrawAttempt.IdempotencyKey` 在 Event 範圍唯一。確認中獎時同一 Transaction 更新 Result、Participant/Prize 摘要與 Audit Log。Redraw 不刪除舊紀錄，而是標記舊 Attempt `Cancelled/Redrawn`，以新 Idempotency Key 建立新 Attempt，因此可追溯完整歷史。

SQLite 不提供 Row Lock；使用短 Transaction、唯一索引與 `Version` Optimistic Concurrency Token 防止兩個 Host Command 同時成功。單一 Server Process 亦可在 Application 層以 Event/Prize keyed lock 降低衝突，但資料庫 Constraint 仍是最後防線。

## 10. Participant Identity

第一版採 **Event-scoped opaque session credential**：

1. Guest Web 首次 Join 前以 Web Crypto 產生 256-bit 隨機 Token 並立即保存；無法產生 Token 的受信任 Client 可由 Server 產生。
2. Database 只保存 SHA-256 Hash，不保存明文。
3. Browser 以 `eventId` 為 Key 保存於 `localStorage`；Refresh 或 SignalR reconnect 時帶回。
4. Server 以 `(EventId, SessionCredentialHash)` 找回同一 Participant，不建立新身份；即使首次 Response 遺失，重送相同 Join Request 也可取得同一身份。
5. SignalR ConnectionId 只代表短暫連線，絕不作為 Participant Identity。

同一 Participant 可能有多個 Tab/裝置；Presence Store 以 Participant 對應多個 Connection，最後一條 Connection 中斷才視為 Offline。`EmployeeNumber` 非空時建立 Event-scoped normalized unique index，但不能單憑員工編號自動登入，避免冒用。

清除瀏覽器資料或換手機後沒有 Credential，第一版需由 Host 協助重新綁定；這比用姓名/員工編號直接認領安全。後續可加入一次性 Rejoin Code，不需要導入外部 Login。

## 11. SQLite Schema / EF Core Model

第一個 Slice：

- `Events`: `Id PK`, `Name`, `EventDateUtc`, `State`, `IsJoinOpen`, `HostCredentialHash`, `CreatedAtUtc`, `Version`
- `Participants`: `Id PK`, `EventId FK`, identity/profile fields, `SessionCredentialHash`, `IsCheckedIn`, `Score`, `HasWon`, `LastSeenAtUtc`, `CreatedAtUtc`, `Version`
- Unique: `Participants(EventId, SessionCredentialHash)`
- Filtered Unique: `Participants(EventId, NormalizedEmployeeNumber)` when non-null
- Index: `Participants(EventId, CreatedAtUtc)`

後續表：`Quizzes`, `Questions`, `QuestionOptions`, `QuizSessions`, `AnswerSubmissions`, `Polls`, `PollOptions`, `PollSessions`, `PollVotes`, `Prizes`, `LuckyDrawSessions`, `LuckyDrawResults`, `Photos`, `AuditLogEntries`, `IdempotencyRecords`。

重要 Constraint：

- `AnswerSubmissions(QuizSessionId, QuestionId, ParticipantId)` unique。
- `PollVotes(PollSessionId, ParticipantId)` unique；匿名是結果顯示政策，不代表 Server 無法阻止重複投票。
- `LuckyDrawSessions(EventId, IdempotencyKey)` unique。
- Photo 只保存安全生成的 `StorageKey`；實體檔案在活動專屬目錄，Client File Name 只作 Metadata。
- Entity Mapping 使用個別 `IEntityTypeConfiguration<T>`，不把所有規則堆在 `OnModelCreating`。

第一個 Slice 可用 Initial Migration 建立資料庫；正式發佈不得使用破壞性重建。資料庫檔案與 Photo Storage 放在可設定的 `Data` 路徑並於活動前備份。

## 12. Server Restart Recovery

持久化：Event、Participant、Quiz/Question、Session State、Answer/Score、Poll Vote、Prize/Draw、Photo Metadata、Audit Log。Transient：SignalR Connection、目前 UI Animation、短時間 Presence Cache。

Startup 流程：

1. 執行已核准 Migration，驗證 Database 可讀寫。
2. 載入非終止狀態的 Event/Quiz/Poll/Draw Session。
3. 若 `QuizSession=Open` 且 `ClosesAtUtc <= now`，以 Recovery Command 轉為 `Closed`；不延長 Deadline。
4. 若 Draw 停在 `Drawing` 且沒有已保存 Candidate，標記失敗並保留 Audit；若 Candidate 已保存則恢復為 `Drawn`，不重新抽。
5. Presence 全部從 Offline 開始，Client reconnect 後重新建立。
6. Client 收到 reconnect 後呼叫 REST State Snapshot，再重新加入 SignalR Group。

每個 Recovery 變更都寫 Log；涉及抽獎或計分的修正同時寫 Audit Log。

## 13. 第一個 Vertical Slice

流程：

1. Host 呼叫 `POST /api/v1/events` 建立活動，取得 Event ID 與 Host Token。
2. Guest 開啟 Server 首頁，輸入 Event ID 與基本身份。
3. Guest Join Endpoint 建立 Participant 或以已保存 Session Token 恢復。
4. Guest 以 Participant Credential 建立 SignalR Connection。
5. Server Presence Service 維護多 Connection 狀態，通知 Host Group。
6. Host 先用 REST 取得完整 Participant Snapshot，再用 SignalR 接收 Presence 變更。
7. Browser Refresh 後使用同一 Token，不新增 Participant；Server Restart 後身份仍可由 SQLite 恢復，Presence 由重連重建。

Acceptance Criteria：

- Event 與 Participant 在 Server Restart 後仍存在。
- 同一瀏覽器 Refresh 不新增第二筆 Participant。
- Guest connect/disconnect 能讓 Host 名單在合理時間內更新。
- Host 未持有正確 Token 時不能取得 Participant List 或訂閱 Host Group。
- Domain/Application Rule 有 Unit Test；Solution Build 無新 Warning。

## 14. Development Milestones

1. **Foundation + Presence Slice**：本文件、Solution、Event/Participant、SQLite、Guest Join、SignalR Presence、Host Dashboard、Tests。
2. **Event Operations + Recovery**：Join Policy、活動狀態、QR Code、Host 設定、Migration/Backup 與 Startup Recovery。
3. **Quiz Vertical Slice**：Question Authoring、State Machine、Answer Idempotency、Score Calculator、即時人數與個人排名。
4. **Image Quiz + Display**：共用 Question Image、Display 題目/倒數/答案/Top N。
5. **Poll**：匿名顯示政策、唯一 Vote、即時統計與 Recovery。
6. **Lucky Draw**：Prize、Eligibility、Idempotent Draw、Confirm/Redraw、Audit 與 Display Animation。
7. **Photo Wall**：安全 Upload、Storage、Moderation、輪播與未來 Print Port 邊界。
8. **Hardening**：負載測試、LAN Runbook、Firewall/QR Address 檢測、Backup/Restore、Security Review 與活動演練。

每個 Milestone 都以可執行 Vertical Slice 完成，不先一次建立所有 Entity、Interface 與空 Service。

## 15. 風險與待確認事項

- **LAN Address/Firewall**：Windows Firewall、AP Client Isolation、VPN 與多張網卡可能使手機無法連線。需提供啟動檢查與現場 Runbook。
- **HTTP Credential Exposure**：LAN HTTP Token 可被同網段攔截。封閉可信網路可接受作為第一版限制；含敏感資料時需 HTTPS。
- **Participant Recovery**：清除 Browser Storage 或換手機會失去 Session。需決定 Host Rebind 或一次性 Rejoin Code 的 UX。
- **員工編號與個資**：需決定是否必填、保存期限、匯出與活動後刪除政策。
- **時間規則**：速度 Bonus 的曲線、網路延遲容忍、同分排名仍待產品決策；Calculator 先保持可替換的純規則。
- **抽獎法遵與可驗證性**：Random 演算法、Eligibility 快照、重抽理由與 Audit 保存期限需由主辦方確認。
- **照片容量與審核**：上限、允許格式、是否先審後播、EXIF 移除與資料保留政策需確認。
- **單機故障**：第一版 Local Server 是單點；至少要有 UPS 建議、定期 SQLite Backup 與備援筆電 Restore 演練。
- **活動規模**：預期人數、同時作答峰值、照片大小決定 SQLite/SignalR 壓測基準。目前以單場數百人、單 Server 為設計前提。

以上風險不需要立即引入分散式架構；先以演練、Constraint、Idempotency、Backup 與清楚的操作流程降低風險。
