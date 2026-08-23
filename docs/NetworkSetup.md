# EventHub 基礎網路設定

這份文件用完全新手的角度說明 EventHub 為何需要 IP、Port 與 Windows Firewall。以下所有 `192.168.0.107` 都只是範例，不代表每台電腦都使用此 IP。

## Server 與 Client

Server 是集中提供資料與服務的程式。活動電腦執行 `EventHub.Server` 後，那台一般 Windows 電腦就是 EventHub Server。

Client 是連向 Server 的使用端：

- `EventHub.Host` 是主持人 Client。
- `EventHub.Display` 是投影 Client。
- 手機 Guest Web 是瀏覽器 Client。

所有 Client 都連 Server，不是手機直接連 Host。

## IP Address 與 Port

IP Address 可以想成建築地址，Port 可以想成該建築中的房號：

```text
192.168.0.107:5000
└──── IP ────┘ └Port
```

以上 IP 只是範例。`192.168.0.107` 找到活動電腦，`:5000` 則找到該電腦上的 EventHub.Server。

## localhost

`localhost` 與 `127.0.0.1` 都代表「目前自己這台裝置」。

- 電腦開 `http://localhost:5000`：找該電腦自己。
- 手機開 `http://localhost:5000`：找手機自己，不是活動電腦。

因此 QR Code 若是 localhost，手機一定不能藉此找到 EventHub Server。

## 0.0.0.0

`0.0.0.0` 是 Server Listen Address（監聽位址），代表 Server 接受所有本機網路介面的 Port 5000 連線。

```text
http://0.0.0.0:5000
```

它適合用來設定 Server 如何接收連線，但不是 Guest URL。手機不能把 `0.0.0.0` 當作活動電腦地址。

## LAN、Wi-Fi 與 Internet

LAN（Local Area Network）是現場的區域網路：

```text
Phone
  │
Wi-Fi
  │
Router / AP
  │
Ethernet 或 Wi-Fi
  │
Server PC
```

Wi-Fi 不等於 Internet。Wi-Fi 是裝置連入本地網路的一種方式；Internet 是通往外部世界的連線。EventHub 的核心資源在本機，因此即使外網中斷，只要 Phone 與 Server 仍在同一個 LAN 且 Router／AP 正常，活動可以繼續。

有些訪客 Wi-Fi 會啟用 Client Isolation／AP Isolation，刻意禁止同一 Wi-Fi 的裝置互相連線。這種網路即使能上 Internet，也可能不能使用 EventHub。

## DHCP：為什麼 IP 會改變

Router 通常透過 DHCP 自動分配 IP。更換電腦、Router、Wi-Fi，或重新連線後，活動電腦可能從 `192.168.0.107` 變成其他位址。因此每次活動與換網路後都應重新執行：

```cmd
ipconfig
```

若需要固定 IP，應由現場網路管理者設定 DHCP Reservation 或固定網路規劃，不要在不了解網段時隨意手動填寫 Windows IP。

## Windows Firewall

Windows Firewall 是電腦的門禁。即使 IP、Server、Port 全部正確，它仍可能阻擋手機進入。

EventHub 的 `Local Event` Profile 主要需要 TCP Port 5000 的 Incoming（輸入）連線。建議只允許活動使用的 Private Network Profile，不要把「關閉整個 Firewall」當成解法。

### 使用 GUI 建立 Inbound Rule

1. 在開始功能表搜尋「具有進階安全性的 Windows Defender 防火牆」。
2. 選擇「輸入規則」。
3. 右側選擇「新增規則」。
4. 規則類型選「連接埠」。
5. 選 TCP，特定本機連接埠輸入 `5000`。
6. 選擇「允許連線」。
7. Profile 建議只勾選 `私人`；是否需要網域 Profile 請依公司 IT 規範。
8. 名稱輸入 `EventHub TCP 5000`。

若公司政策禁止自行新增規則，請交由 IT 處理。不要在正式活動前才第一次確認。

## Private 與 Public Network Profile

Windows 會把網路標記為 Private（私人）或 Public（公用）。受信任的活動內部 LAN 通常應設為 Private；公用 Profile 的防火牆限制通常較嚴格。

可在 Windows「設定 → 網路和網際網路 → Wi-Fi／乙太網路 → 目前連線」查看 Network Profile。是否允許修改仍應遵守公司 IT 規範。

## launchSettings.json

`src/EventHub.Server/Properties/launchSettings.json` 是 Visual Studio 與 `dotnet run` 在開發階段使用的啟動設定，不是正式部署安裝檔。

目前包含：

| Profile | 實際設定 | 用途 |
|---|---|---|
| `http` | `http://localhost:5029` | 同一台電腦開發測試 |
| `https` | `https://localhost:7270;http://localhost:5029` | 本機 HTTPS／HTTP 開發測試 |
| `Local Event` | `http://0.0.0.0:5000` | 手機與 LAN 現場測試 |
| `IIS Express` | HTTP `2392`、HTTPS `44371` | Visual Studio IIS Express；目前操作指南不採用 |

`Local Event` 目前還設定：

```json
"ASPNETCORE_ENVIRONMENT": "Production",
"EventHub__JoinBaseUrl": "http://192.168.0.107:5000"
```

`192.168.0.107` 只是目前檔案中的範例／特定電腦值，換電腦或網路時必須更新。

常見欄位意思：

- `commandName: Project`：直接啟動 ASP.NET Core Project。
- `dotnetRunMessages: true`：Console 顯示啟動與監聽資訊。
- `launchBrowser`：啟動時是否自動開瀏覽器；Local Event 目前是 `false`。
- `applicationUrl`：開發 Server 要監聽的 URL 與 Port。
- `environmentVariables`：只在該啟動 Profile 下提供的環境設定。

## appsettings.json 與 Development Override

Server 的 `appsettings.json` 預設：

- `EventHub:DataDirectory`：`Data`。
- `EventHub:JoinBaseUrl`：`http://localhost:5000`。
- `EventHub:Display:LeaderboardTop`：10。

Development 環境的 `appsettings.Development.json` 會把 JoinBaseUrl 改成 `http://localhost:5029`。環境變數 `EventHub__JoinBaseUrl` 的優先順序更高，因此 Local Event Profile 可以覆蓋成 LAN IP。

## applicationUrl 與 Guest URL 不同

| 設定 | 作用 |
|---|---|
| `launchSettings.json` | Visual Studio／dotnet run 在開發時採用哪個啟動 Profile |
| Server Listen Address | Server 接受哪些網路介面與 Port，例如 `0.0.0.0:5000` |
| LAN IP | 活動電腦在目前 LAN 的地址，例如 `192.168.0.107`（僅為範例） |
| Join URL | 手機真正開啟的網址，例如 `http://192.168.0.107:5000/join/8K3F2A` |
| Firewall | 外部 Client 是否能進入該 Port |

Server Listen 使用 `0.0.0.0:5000` 時，通常不需要因 LAN IP 改變而修改 Listen Address；但 JoinBaseUrl 一定要改成新的 LAN IP，否則 QR Code 仍會指向舊地址。

## Host 與 Display 的 Server URL

Host 的 Server URL 由 Event 畫面文字欄位輸入，Designer 預設為 `http://localhost:5000`，目前沒有獨立 Host 設定檔。Display 從自己的 `appsettings.json` 讀取預設值，也能在啟動畫面修改。

同一台活動電腦上：

- Host／Display API URL 可用 `http://localhost:5000`。
- 手機 Join URL 必須用 LAN IP，例如 `http://192.168.0.107:5000`（僅為範例）。

## 換一台電腦或換網路 Checklist

1. 執行 `ipconfig`，重新取得 IPv4。
2. 更新 Local Event 的 `EventHub__JoinBaseUrl`。
3. 確認 Server Console 是 `0.0.0.0:5000`。
4. 確認 Windows Network Profile 與 Firewall Port 5000。
5. 本機測試 `/health`。
6. 建立活動，檢查 Join URL 是新 IP。
7. 用真實手機掃 QR Code 測試。

不要只因為上次活動成功，就假設這次 IP 與 Firewall 環境完全相同。
