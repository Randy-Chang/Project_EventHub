# EventHub 架構

本文件描述目前已實作的系統邊界。功能操作請從 [文件導覽](README.md) 開始；尚未實作的功能不在此預先設計。

## 執行元件

```text
Guest Browser ── HTTP / SignalR ──┐
Host WinForms ── HTTP / SignalR ──┼── EventHub.Server ── SQLite
Display WinForms ─ HTTP / SignalR ┘
```

- `EventHub.Server` 是唯一可信來源，提供 Minimal API、SignalR、Guest 靜態網站與資料庫存取。
- `EventHub.Host` 是主持人控制台，建立或恢復活動、匯入題庫及控制 Quiz 與 Display。
- `EventHub.Display` 是投影端，只呈現 Server 保存的目前狀態。
- `EventHub.Web` 提供手機 Guest 使用的靜態 Web 資源。

REST 負責命令與目前狀態查詢；SignalR 只通知 Client 狀態已改變。Client 收到通知或重連後，會再向 Server 取得完整狀態，因此不依賴錯過的訊息重建流程。

## Project 責任與相依方向

```text
EventHub.Domain
      ▲
EventHub.Application
      ▲
EventHub.Infrastructure
      ▲
EventHub.Server ── EventHub.Web

EventHub.Host       EventHub.Display
  (獨立 HTTP / SignalR clients)
```

- `EventHub.Domain`：Event、Participant、Quiz、作答、計分等 Entity、Value Object 與狀態規則；不依賴其他 Project。
- `EventHub.Application`：Use case orchestration、DTO 與 Infrastructure abstraction；只依賴 Domain。
- `EventHub.Infrastructure`：EF Core SQLite、Repository、CSV、Presence 與 Credential 實作；實作 Application 定義的邊界。
- `EventHub.Server`：組合依賴、驗證 HTTP／SignalR 輸入、呼叫 Application Service 並送出即時通知。
- `EventHub.Host`、`EventHub.Display`：WinForms presentation clients，不直接存取資料庫或 Domain。

## 目前核心模型

- `Event`：活動名稱、日期、Join Code、Lifecycle、Join Policy 與 Display Mode。
- `Participant`：活動參與者與恢復身份所需的 Credential。
- `Quiz`、`QuizQuestion`、`QuizOption`：題庫及題目定義，支援 `Practice` 與 `Scored`。
- `QuizQuestionSession`：一次題目流程，狀態為 `Waiting`、`Open`、`Closed` 或 `Revealed`。
- `ParticipantAnswer`：同一 Participant 對同一 Session 只有一筆有效答案，資料庫亦有唯一約束。
- `ParticipantQuestionResult`、`ParticipantQuizScore`：公布答案後產生的結果及正式題累積分數。

題目起訖時間、是否逾時、答案正確性、分數與排名均由 Server 決定。Client 倒數只負責顯示。

## 持久化與暫態資料

SQLite 保存 Event、Participant、Quiz、Question、Session、Answer、Result、Score 與 Display Mode。Server 啟動時套用 migration，並恢復逾時但仍為 Open 的題目狀態。

Participant 的即時連線狀態由 `InMemoryParticipantPresenceStore` 保存，屬於可重建的暫態資料；Server 重啟後 Client 重連即可恢復。

## 已實作範圍

- 活動建立、Join Code／QR Code 與 Host Token 恢復。
- Participant 加入、身份恢復及 Presence。
- CSV 題庫、內建 Practice、Scored Quiz、截止判定、計分與排行榜。
- Host 即時監看及 Display 的 Waiting、Live、Result、Ranking 模式。
- Server restart 與 Client reconnect 後的狀態恢復。

Poll、Lucky Draw、Photo Wall 與 Image Quiz 尚未實作；新增時應延伸既有 Domain／Application／Infrastructure 邊界，不在 UI 或 Hub 內建立平行 Business Logic。
