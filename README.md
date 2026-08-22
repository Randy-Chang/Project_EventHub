# Project EventHub

EventHub 是以公司尾牙、春酒、家庭日與一般企業活動為目標的區域網路互動平台。目前已包含活動加入、QR Code 與 Quiz 快問快答 Vertical Slice：

- Host 建立活動並取得 Host Token。
- Event 具有持久化的 6 碼 Join Code，Host 可顯示加入網址及離線產生的 QR Code。
- Guest 由同一個 ASP.NET Core Server 提供的 Mobile Web 加入。
- Event 與 Participant 保存至 SQLite。
- Browser 使用 Event-scoped Session Token，在重新整理後恢復同一身份。
- Guest 透過 SignalR 回報 Presence；Host WinForms 即時顯示在線名單。
- Host Participant API 與 SignalR Group 以 Host Token 保護。
- Host 可建立、開始、關閉及公布單選 Quiz 題目。
- Guest 可即時作答，並在答案公布後查看自己的結果。
- Quiz Session 與 Participant Answer 保存至 SQLite，支援重新整理與 Server Restart Recovery。
- Display WinForms 可在投影機／第二螢幕顯示 QR Waiting、題目、倒數、結果統計與 Top 10 排行榜。
- Host 可控制 Waiting、Question、Result、Leaderboard；Display restart／SignalR reconnect 後會重新取得 Server Current State。

完整設計請見 [Architecture Proposal](docs/ARCHITECTURE.md)。

第一次使用或忘記操作流程時，請見 [EventHub 使用說明書](docs/USER_GUIDE.md)。

需要測試手機與主持人電腦的區域網路連線時，請見 [電腦與手機區域網路測試指南](docs/LAN_TEST_GUIDE.md)。

## 專案結構

```text
src/
├─ EventHub.Domain
├─ EventHub.Application
├─ EventHub.Infrastructure
├─ EventHub.Server
├─ EventHub.Web
├─ EventHub.Host
└─ EventHub.Display
tests/
├─ EventHub.Domain.Tests
├─ EventHub.Application.Tests
├─ EventHub.Infrastructure.Tests
└─ EventHub.Host.Tests
```

`EventHub.Display` 是只透過 HTTP／SignalR 連線的 WinForms 展示端，不會參考 Infrastructure 或直接讀取 SQLite。

## 開發環境執行

### Visual Studio 2022 本機測試

使用 Visual Studio 2022 的 `http` Profile 啟動 `EventHub.Server` 時，Server 預設網址為：

```text
http://localhost:5029
```

Host Console 的 `Server URL` 也必須輸入：

```text
http://localhost:5029
```

可先在瀏覽器開啟 `http://localhost:5029/health`；看到 `{"status":"ok"}` 表示 Server 正常。

`5029` 是 VS2022 `http` 開發 Profile 使用的 Port，只適合同一台電腦本機測試。此模式產生的加入網址會顯示 localhost 警告，手機不可使用該 QR Code。

### 命令列與區域網路測試

先以 `ipconfig` 找到活動主機的 Wi-Fi IPv4（以下以 `192.168.1.20` 為例），再執行：

```powershell
dotnet tool restore
dotnet restore Project_EventHub.sln
dotnet build Project_EventHub.sln --no-restore
dotnet test Project_EventHub.sln --no-build
$env:EventHub__JoinBaseUrl = "http://192.168.1.20:5000"
dotnet run --project src/EventHub.Server/EventHub.Server.csproj --urls http://0.0.0.0:5000
```

也可以在 VS2022 選擇 `EventHub.Server` 的 `Local Event` Profile；使用前必須把 `launchSettings.json` 中範例 IP `192.168.1.100` 改成活動主機的實際 IPv4。

另一個 Terminal 啟動 Host：

```powershell
dotnet run --project src/EventHub.Host/EventHub.Host.csproj
```

再啟動 Display：

```powershell
dotnet run --project src/EventHub.Display/EventHub.Display.csproj
```

Display 輸入 Server API URL 與 Host 顯示的 Event ID，選擇投影螢幕後按「連線展示」。`F11` 切換全螢幕，`Esc` 離開全螢幕。

Host 建立活動後會直接顯示 Join Code、Guest 加入網址及 QR Code，例如：

```text
http://192.168.1.20:5000/join/8K3F2A
```

區域網路模式下，Host Console 的 `Server URL` 是 API 位址，可使用同機位址：

```text
http://localhost:5000
```

手機使用的 Join URL 則來自 Server 的 `EventHub:JoinBaseUrl`。兩者責任不同，不需要是相同 Host Name，但必須連到同一個 Server Port。

請勿混用 `5029` 與 `5000`。實際監聽網址以 Server Console 的 `Now listening on:` 為準；Guest Join URL 的 IP 必須是手機可連線的活動主機 LAN IP。

SQLite 預設位於 `src/EventHub.Server/Data/eventhub.db`（相對於 Server Content Root）。此目錄被 Git 忽略。SignalR Browser Client 已 vendoring 於 `EventHub.Web`，執行期間不需要 CDN 或 Internet。

## 現場部署提醒

- Windows Firewall 必須允許所選 Port 的 Private Network inbound traffic。
- Wi-Fi AP 不可啟用 Client Isolation，手機與 Host 必須能互相連線。
- 正式活動前應以實際手機、Wi-Fi 與預估同時人數完成演練。
- 第一版 LAN HTTP 適用於受信任的封閉網路；若保存敏感個資，應配置 HTTPS。

## CSV 題庫匯入

Host 連線到活動後，可在「正式題庫（CSV 匯入）」區使用「匯出範本」取得 UTF-8 BOM 範本，或參考 [`samples/QuizQuestionBankSample.csv`](samples/QuizQuestionBankSample.csv)。

正式欄位為：

```text
QuestionKey,QuizTitle,Category,Difficulty,Order,Question,OptionA,OptionB,OptionC,OptionD,CorrectOption,DurationSeconds
```

- 一個 CSV 只能有一個 `QuizTitle`，同活動不可匯入同名題庫。
- `QuestionKey` 與 `Order` 在同一題庫內不可重複。
- `Difficulty` 僅接受 `Easy`、`Medium`、`Hard`，目前不影響計分。
- `CorrectOption` 僅接受 `A`～`D`，作答秒數須為 5～120。
- 檔案必須是 UTF-8、最大 5 MB、最多 500 題。
- 預覽有任何錯誤時不會寫入資料；按下確認後 Server 仍會重新驗證，整批成功或整批回滾。
