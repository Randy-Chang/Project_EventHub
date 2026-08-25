# AGENTS.md

本文件是 AI Coding Agent 在此 Repository 的預設工作規範。使用者當次明確指示優先；其餘情況遵循以下原則。

## 工作方式

- 修改前先理解 Solution、Project references、Namespace、既有 Service、DTO、abstraction 與 UI pattern。
- 涉及 Project responsibility、dependency direction、Domain model、資料持久化或 Client／Server 邊界的修改前，先閱讀 `docs/ARCHITECTURE.md`；若實作改變上述架構，必須同步更新該文件。
- 採最小必要修改；保留使用者既有變更，不順手重構無關程式碼，也不建立重複能力。
- 優先選擇簡單、清楚、可測試且可維護的做法。每個新 abstraction 必須解決目前實際存在的邊界、替換或測試需求。
- 不為套用 pattern 主動加入 Generic Repository、Unit of Work wrapper、CQRS／Mediator framework、Event Sourcing、Microservice、Message Broker、Redis 或複雜 Plugin System。
- 新增 NuGet package 前確認 .NET 或現有 package 無法滿足需求，並考量維護狀態與 License。

## 架構邊界

- `EventHub.Domain` 保存 business state 與 invariant，不依賴 UI、ASP.NET、EF Core 或其他 Infrastructure。
- `EventHub.Application` orchestration use case，透過必要 abstraction 使用 persistence、clock、random、file 或其他外部能力。
- `EventHub.Infrastructure` 實作 EF Core SQLite、Repository、CSV、Credential、Presence 等技術細節。
- Controller、Minimal API endpoint 與 SignalR Hub 僅處理輸入驗證、連線 context、Application Service 呼叫、結果映射與通知，不放完整 business flow。
- WinForms Form／UserControl 僅負責 presentation、event handling 與呼叫 client/service，不直接操作資料庫或承載主要 Domain Logic。
- 遵循 SRP、低耦合與高內聚；不要為只有單一直接用途的小 class 強制建立 interface。

## Server authoritative 與資料一致性

- Event／Quiz／Display state、目前題目、起訖時間、答案、正確性、分數、排名及抽獎結果以 Server 為唯一可信來源。
- Client timer 只供顯示；deadline 與結果由 Server 驗證。
- 所有 Client 輸入皆不可信，Server 必須驗證 ID、Credential／permission、目前 state、deadline 與資料歸屬。
- REST 用於 command 與 state retrieval；SignalR 用於 realtime notification。Client 重連後必須能重新取得完整 current state。
- 核心活動資料必須持久化；只有 connection／presence 等可重建資訊可放在 memory。
- 重試、double click、refresh 與 reconnect 不得造成重複答案或重複執行重要命令；關鍵 invariant 同時以 application logic 與 database constraint 保護。

## Domain 規則

- Quiz 使用明確 state model，避免多個可能矛盾的 boolean。State transition、deadline、計分、排名與 duplicate answer 應優先具備 unit test。
- 影響 business rule、排程或 audit 的時間使用可替換 clock，內部保存 UTC；不要在相關邏輯散落 `DateTime.Now`／`DateTime.UtcNow`。
- Lucky Draw 若新增，selection 必須在 Server 端以可測試 random abstraction 執行，處理 eligibility、已中獎排除、idempotency、redraw state 與 audit log。
- Upload 若新增，驗證 size、content type、format、file name 與 storage path；不可直接以 Client file name 建立 Server path，並防止 path traversal。

## WinForms 與 Display

- 固定控制項的宣告、初始化、layout、`Dock`、`Anchor` 放在 `.Designer.cs`；event、資料載入、network 與畫面狀態放在 `.cs`。
- 只有資料筆數決定的選項、排行榜或統計列可動態 render；不得用 runtime controls 重建整個固定 UI。
- Background／SignalR callback 更新 control 時，使用專案既有的 `Invoke`、`BeginInvoke` 或 `SynchronizationContext` pattern。
- Network、database、file I/O 優先使用 async API；避免 `.Result`、`.Wait()` 與非 event-handler 的 `async void`。合理的長時間 I/O 支援 `CancellationToken`。
- Display 以 1920×1080、16:9 為設計基準，但至少相容 1366×768、1600×900、2560×1440 與 3840×2160。使用 Designer 的 `Dock`、`Anchor`、`TableLayoutPanel`、百分比尺寸及正確 DPI scaling，不依賴整頁固定座標。
- Display 主要內容保留約 3%～5% 安全邊界；不得有重疊、截字、不可操作按鈕、不必要捲軸或圖片變形。照片保持比例，不使用 `PictureBoxSizeMode.StretchImage`。
- Display layout 異動至少實測 1366×768 與 1920×1080；其他解析度、DPI、全螢幕或第二螢幕未執行時，回報 `Not Executed`，不可推定通過。

## C# 品質與安全

- 使用有語意的英文名稱：type／method 用 PascalCase，parameter／local 用 camelCase，boolean 優先以 `Is`、`Has`、`Can`、`Should` 開頭。
- 以 enum 表達有限狀態，避免 magic number／string。透過模型與流程降低 null；不要無理由使用 null-forgiving operator。
- LINQ 以可讀性為準；有明確多步 business flow 時可使用更清楚的 imperative code。
- Comment 說明 why、constraint 或不直觀規則，不重述程式碼。重要 public API 可加 XML documentation。
- 不得空白捕捉或吞掉 Exception。合理處理、記錄或向上傳遞；UI 顯示友善訊息，但 log 保留診斷資訊且不寫入不必要的敏感資料。
- 重要操作應記錄：Server startup、participant join、game／question state change、draw／winner、unexpected exception、database failure 與 SignalR issue。
- 避免明顯效能問題，例如高頻全表 polling、每次 broadcast 讀整張 table、N+1 query 或不合理的大型 image binary 流程；不做尚無需求的 distributed optimization。

## 檔案格式

- 本次修改的文字檔統一為 Windows CRLF，不得混用 CRLF／LF。
- 遵循 Repository 的 `.gitattributes`／`.editorconfig`；若規則衝突，先向使用者指出。
- 不要只為換行格式造成與任務無關的整份巨大 diff。

## 完成前驗證

依修改範圍執行並回報：

1. `dotnet build Project_EventHub.sln`。
2. 相關 test；合理時執行完整 `dotnet test Project_EventHub.sln --no-build`。
3. 新增的 compiler／nullable warning。
4. 本次文字檔 CRLF 與混用換行檢查。
5. Diff 是否只包含必要修改，且無 debug code、unused using 或 temporary file。
6. Display 異動的必要解析度與 DPI 實測結果。

若任何項目無法執行，明確標示原因，不以推定結果代替。

## 完成回報

至少包含：

- `Changed`：修改內容。
- `Architecture`：只有涉及責任或相依調整時才說明。
- `Verification`：Build、Test、warning 與 CRLF 結果。
- `Remaining Risks`：只有存在實際風險時才列出。
