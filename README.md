# Project EventHub

EventHub 是以公司尾牙、春酒、家庭日與一般企業活動為目標的區域網路互動平台。第一個 Vertical Slice 已包含：

- Host 建立活動並取得 Host Token。
- Guest 由同一個 ASP.NET Core Server 提供的 Mobile Web 加入。
- Event 與 Participant 保存至 SQLite。
- Browser 使用 Event-scoped Session Token，在重新整理後恢復同一身份。
- Guest 透過 SignalR 回報 Presence；Host WinForms 即時顯示在線名單。
- Host Participant API 與 SignalR Group 以 Host Token 保護。

完整設計請見 [Architecture Proposal](docs/ARCHITECTURE.md)。

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

SQLite 預設位於 `src/EventHub.Server/Data/eventhub.db`（相對於 Server Content Root）。此目錄被 Git 忽略。SignalR Browser Client 已 vendoring 於 `EventHub.Web`，執行期間不需要 CDN 或 Internet。

## 現場部署提醒

- Windows Firewall 必須允許所選 Port 的 Private Network inbound traffic。
- Wi-Fi AP 不可啟用 Client Isolation，手機與 Host 必須能互相連線。
- 正式活動前應以實際手機、Wi-Fi 與預估同時人數完成演練。
- 第一版 LAN HTTP 適用於受信任的封閉網路；若保存敏感個資，應配置 HTTPS。
