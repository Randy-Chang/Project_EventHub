# EventHub

EventHub 是用於尾牙、春酒、家庭日與一般企業活動的 Local LAN 即時互動平台。活動電腦執行 Server，主持人使用 Host，投影機使用 Display；參與者以手機瀏覽器掃描 QR Code 加入。核心流程只需要區域網路，活動現場即使沒有 Internet 也能運作。

目前已實作活動建立與加入、Participant Presence、CSV 題庫、Practice／Scored Quiz、即時計分、排行榜、QR Join，以及 WinForms 大螢幕展示。Poll、Lucky Draw、Photo Wall、Image Quiz 目前尚未實作。

## Architecture

```text
手機 Browser
       │
       │ Wi-Fi / LAN
       ▼
EventHub.Server
       │
 ┌─────┴─────┐
 ▼           ▼
Host       Display
```

- `EventHub.Server`：整個系統的中心，保存活動資料並提供 Web、HTTP API 與 SignalR 即時通知。
- `EventHub.Host`：主持人控制台，用來建立活動、匯入題庫及控制 Quiz／Display。
- `EventHub.Display`：投影機或第二螢幕使用的展示端，沒有重要管理功能。
- `EventHub.Web`：手機瀏覽器使用的 Guest Web。

Solution 另包含 `EventHub.Domain`、`EventHub.Application`、`EventHub.Infrastructure`，以及 Domain、Application、Infrastructure、Host 四個測試專案。

## Quick Start

1. 開啟 `Project_EventHub.sln` 並建置 Solution。
2. 活動當天在命令提示字元執行 `ipconfig`，找出活動電腦目前使用網卡的 IPv4。
3. 將 `Local Event` Profile 的 `EventHub__JoinBaseUrl` 改成該 IPv4；Repository 內的 IP 只是範例。
4. 確認 Windows Firewall 的 Private Profile 允許 TCP Port 5000。
5. 以 `Local Event` Profile 啟動 `EventHub.Server`，確認 Console 顯示 `http://0.0.0.0:5000`。
6. 在本機瀏覽器開啟 `http://localhost:5000/health`，確認回傳 `{"status":"ok"}`。
7. 啟動 `EventHub.Host`，Server URL 使用 `http://localhost:5000`，輸入活動名稱與日期後建立活動。
8. 確認 Join URL 是活動電腦的 LAN IPv4，不是 `localhost` 或 `0.0.0.0`。
9. 讓 2～3 支同網路手機掃描 QR Code、加入活動，確認 Host Participant 數量更新。
10. 啟動 `EventHub.Display`，輸入 Server URL 與 Host 顯示的 Event ID，再按「連線展示」。
11. 在 Question Bank 匯入 CSV，或先在 Quiz Activity 執行內建熱身。
12. 完整測試 Start、Answer、Close、Reveal、Result 與 Ranking 後再開始正式活動。

同一台電腦的快速開發測試可使用 Server 的 `http` Profile（`http://localhost:5029`），但手機不能使用這個模式產生的 localhost QR Code。

## Documentation

- [文件導覽](docs/README.md)
- [完全第一次使用 EventHub](docs/GettingStarted.md)
- [主持人 Host 操作手冊](docs/HostOperation.md)
- [EventHub 基礎網路設定](docs/NetworkSetup.md)
- [症狀式故障排除](docs/Troubleshooting.md)
- [目前系統架構](docs/ARCHITECTURE.md)

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

目前 Repository 沒有 Publish Profile 或安裝程式。支援的開發執行方式是 Visual Studio 2022，或安裝 .NET 8 SDK 後使用 `dotnet run`；Windows 防火牆可使用 `scripts/Configure-EventHubFirewall.ps1` 設定。SQLite 預設資料位於 Server Content Root 下的 `Data/eventhub.db`。
