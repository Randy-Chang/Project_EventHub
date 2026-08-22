# Project EventHub

EventHub 是以公司尾牙、春酒、家庭日與一般企業活動為目標的區域網路互動平台。目前已包含活動加入與 Quiz 快問快答兩個 Vertical Slice：

- Host 建立活動並取得 Host Token。
- Guest 由同一個 ASP.NET Core Server 提供的 Mobile Web 加入。
- Event 與 Participant 保存至 SQLite。
- Browser 使用 Event-scoped Session Token，在重新整理後恢復同一身份。
- Guest 透過 SignalR 回報 Presence；Host WinForms 即時顯示在線名單。
- Host Participant API 與 SignalR Group 以 Host Token 保護。
- Host 可建立、開始、關閉及公布單選 Quiz 題目。
- Guest 可即時作答，並在答案公布後查看自己的結果。
- Quiz Session 與 Participant Answer 保存至 SQLite，支援重新整理與 Server Restart Recovery。

完整設計請見 [Architecture Proposal](docs/ARCHITECTURE.md)。

第一次使用或忘記操作流程時，請見 [EventHub 使用說明書](docs/USER_GUIDE.md)。

## 專案結構

```text
src/
├─ EventHub.Domain
├─ EventHub.Application
├─ EventHub.Infrastructure
├─ EventHub.Server
├─ EventHub.Web
└─ EventHub.Host
tests/
├─ EventHub.Domain.Tests
└─ EventHub.Application.Tests
```

`EventHub.Display` 會在 Display Vertical Slice 才建立，以免先留下沒有實際行為的空殼。

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

`5029` 是 VS2022 開發 Profile 使用的 Port，只適合同一台電腦本機測試。手機不可使用 `localhost`。

### 命令列與區域網路測試

```powershell
dotnet tool restore
dotnet restore Project_EventHub.sln
dotnet build Project_EventHub.sln --no-restore
dotnet test Project_EventHub.sln --no-build
dotnet run --project src/EventHub.Server/EventHub.Server.csproj --urls http://0.0.0.0:5000
```

另一個 Terminal 啟動 Host：

```powershell
dotnet run --project src/EventHub.Host/EventHub.Host.csproj
```

Host 建立活動後，將 Guest 網址中的 Host 電腦 LAN IP 與活動 ID 分享給參與者：

```text
http://<host-lan-ip>:5000/?eventId=<event-id>
```

區域網路模式下，Host Console 的 `Server URL` 應輸入：

```text
http://<host-lan-ip>:5000
```

請勿混用 `5029` 與 `5000`。實際網址應以 Server Console 顯示的 `Now listening on:` 為準，Host Console 與 Guest 網址必須使用相同的 Server 位址及 Port。

SQLite 預設位於 `src/EventHub.Server/Data/eventhub.db`（相對於 Server Content Root）。此目錄被 Git 忽略。SignalR Browser Client 已 vendoring 於 `EventHub.Web`，執行期間不需要 CDN 或 Internet。

## 現場部署提醒

- Windows Firewall 必須允許所選 Port 的 Private Network inbound traffic。
- Wi-Fi AP 不可啟用 Client Isolation，手機與 Host 必須能互相連線。
- 正式活動前應以實際手機、Wi-Fi 與預估同時人數完成演練。
- 第一版 LAN HTTP 適用於受信任的封閉網路；若保存敏感個資，應配置 HTTPS。
