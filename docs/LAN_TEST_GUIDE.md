# EventHub 電腦與手機區域網路測試指南

本文件說明如何在 Windows 主持人電腦啟動 EventHub，並確認同一個 Wi-Fi 內的手機可以加入活動。以下 IP `192.168.0.107` 只是範例，實際操作時必須改成當下由 `ipconfig` 查到的 Wi-Fi IPv4。

## 1. 先理解三種網址

| 用途 | 範例 | 使用者 |
|---|---|---|
| VS 本機開發 | `http://localhost:5029` | 同一台電腦 |
| Local Event API | `http://localhost:5000` | 同一台電腦上的 Host／Display |
| Local Event LAN | `http://192.168.0.107:5000` | 手機及其他電腦 |

手機不能使用 `localhost`。`localhost` 在手機上代表手機自己，不是主持人電腦。

進行手機測試時，請勿混用 Port `5029` 與 `5000`。本文件的 Local Event 流程統一使用 Port `5000`。

## 2. 測試前準備

1. 電腦與手機連接同一個 Wi-Fi。
2. 不要使用名稱含有 Guest／訪客的 Wi-Fi。
3. 電腦與手機暫時關閉 VPN。
4. 手機可暫時關閉行動網路，避免瀏覽器改走 4G／5G。
5. Windows 網路設定建議使用「私人網路」。
6. Wi-Fi AP 不可啟用 AP Isolation／Client Isolation。
7. 確認沒有另一個 EventHub.Server 佔用 Port `5000`。

手機與電腦通常應取得相同網段的 IP。例如電腦是 `192.168.0.107`，手機通常也會是 `192.168.0.xxx`。

## 3. 查詢主持人電腦 IP

在 Windows 開啟命令提示字元或 PowerShell，執行：

```powershell
ipconfig
```

找到目前正在使用的 Wi-Fi 網卡，不要選擇媒體已中斷連線的網卡。記下：

```text
IPv4 位址：192.168.0.107
預設閘道：192.168.0.1
```

如果切換 Wi-Fi、重新連線路由器或改用手機熱點，IPv4 可能改變，必須重新檢查。

## 4. 設定 Local Event Profile

在 Visual Studio 2022 開啟：

```text
src/EventHub.Server/Properties/launchSettings.json
```

確認 `Local Event` 設定如下，並將範例 IP 改成實際 IPv4：

```json
"Local Event": {
  "commandName": "Project",
  "dotnetRunMessages": true,
  "launchBrowser": false,
  "applicationUrl": "http://0.0.0.0:5000",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Production",
    "EventHub__JoinBaseUrl": "http://192.168.0.107:5000"
  }
}
```

設定意義：

- `0.0.0.0:5000`：Server 接受本機及區域網路連線。
- `JoinBaseUrl`：Host、Display 與 QR Code 顯示給手機使用的網址。

## 5. 在 Visual Studio 2022 啟動 Local Event

1. 按 `Shift + F5`，停止目前正在執行的程式。
2. 在方案總管對 `EventHub.Server` 按右鍵。
3. 選擇「設定為啟始專案」。
4. 在 VS2022 上方綠色啟動按鈕旁的 Profile 選單選擇 `Local Event`。
5. 按 `F5`。
6. 保持 Server Console 開啟。

正常時 Console 應顯示類似：

```text
Now listening on: http://0.0.0.0:5000
Application started
```

## 6. 在電腦執行 Health Check

先在主持人電腦瀏覽器依序開啟：

```text
http://localhost:5000/health
```

```text
http://192.168.0.107:5000/health
```

兩個網址都應顯示：

```json
{"status":"ok"}
```

判斷方式：

| 結果 | 優先檢查 |
|---|---|
| 兩個網址都成功 | Server 與 LAN 監聽正常 |
| 兩個網址都失敗 | Server 未啟動、Profile 錯誤或 Port 錯誤 |
| localhost 成功、LAN IP 失敗 | Windows 防火牆或未使用 Local Event Profile |

網址必須使用 `http://`，不要改成 `https://`。

## 7. Windows 防火牆設定

第一次執行時若 Windows 詢問是否允許網路存取，請勾選「私人網路」後允許。

若 LAN IP Health Check 仍失敗，可手動允許 TCP 5000：

1. 按 `Win + R`。
2. 輸入 `wf.msc` 後按 Enter。
3. 選擇左側「輸入規則」。
4. 選擇右側「新增規則」。
5. 選擇「連接埠」。
6. 選擇 TCP。
7. 特定本機連接埠輸入 `5000`。
8. 選擇「允許連線」。
9. 至少勾選目前使用的網路類型，通常為「私人」。
10. 規則名稱輸入 `EventHub Server TCP 5000`。

完成後重新測試 LAN IP 的 `/health`。

## 8. 啟動 Host 並建立活動

保持 Server 執行，在方案總管對 `EventHub.Host` 按右鍵，選擇「偵錯」→「啟動新執行個體」。

Host 的 Server URL 使用：

```text
http://localhost:5000
```

輸入活動名稱與日期後按「建立活動」。建立成功後應顯示：

- 活動 ID。
- Host Token。
- Join Code。
- Join URL。
- QR Code。

Join URL 應包含目前的 LAN IP，例如：

```text
http://192.168.0.107:5000/join/8K3F2A
```

如果 Join URL 仍是 `localhost` 或舊 IP，請停止所有程式，確認 `Local Event` Profile 與 `JoinBaseUrl` 後重新啟動 Server。

## 9. 先用電腦模擬 Guest

將 Host 顯示的完整 Join URL 貼到電腦瀏覽器，填寫參與者資料並加入。

預期結果：

- Guest 顯示已加入活動。
- Host 顯示新的 Participant。
- 開啟無痕視窗可以模擬另一位參與者。
- 同一個瀏覽器重新整理後不會建立重複身份。

## 10. 使用手機測試

手機連上相同 Wi-Fi 後，先手動開啟：

```text
http://192.168.0.107:5000/health
```

看到 `{"status":"ok"}` 後，再使用相機掃描 Host 或 Display 的 QR Code，或手動輸入完整 Join URL。

加入成功後確認：

- 手機顯示活動頁面。
- Host Participant 名單立即更新。
- Host 在線人數更新。
- 手機重新整理後仍保持相同身份。

## 11. 啟動 Display

在方案總管對 `EventHub.Display` 按右鍵，選擇「偵錯」→「啟動新執行個體」。

Display 設定：

```text
Server API URL：http://localhost:5000
Event ID：貼上 Host 顯示的活動 ID
```

第一次測試建議取消「全螢幕」，選擇 Primary Screen，然後按「連線展示」。

預期 Display 顯示活動名稱、QR Code、Join Code 與參與人數。手機加入後，參與人數應透過 SignalR 即時增加。

## 12. Quiz 即時流程測試

1. Host 建立一題 A／B／C／D 單選題。
2. Host 按「開始題目」。
3. 手機與 Display 應立即顯示題目。
4. 手機作答後，Host 與 Display 的作答人數應更新。
5. Host 按「關閉作答」。
6. Display 顯示「時間到／等待公布答案」，不得顯示正確答案。
7. Host 按「公布正確答案」。
8. Display 顯示正確答案、答對率與選項統計。
9. Host 按「Leaderboard」。
10. Display 顯示 Top 10。

## 13. 故障判斷

| 現象 | 可能原因 |
|---|---|
| `localhost:5000/health` 失敗 | Server 未啟動、啟動失敗或 Port 錯誤 |
| localhost 成功但 LAN IP 失敗 | 防火牆或 Server 未使用 `0.0.0.0:5000` |
| 電腦 LAN IP 成功但手機失敗 | Guest Wi-Fi、AP Isolation、VPN、防火牆網路類型 |
| 手機 Health 成功但 QR Code 失敗 | QR Code 使用舊 IP／舊 Join Code |
| Guest 加入但 Host 沒更新 | Host 連到不同 Server、Event ID 不同或 SignalR 連線異常 |
| Display 顯示 Event ID 錯誤 | 尚未貼上 Host 顯示的完整活動 ID |
| Display 畫面未即時切換 | Display 與 Host 連到不同 Server，或 SignalR 正在重連 |

公司或飯店 Wi-Fi 可能禁止裝置互相連線。遇到這種情況可使用獨立路由器，或以手機熱點進行對照測試。切換網路後必須重新執行 `ipconfig` 並更新 `JoinBaseUrl`。

## 14. 完整檢查清單

- [ ] 電腦與手機在相同 Wi-Fi。
- [ ] 已關閉可能干擾路由的 VPN。
- [ ] `localhost:5000/health` 成功。
- [ ] 電腦使用 LAN IP 開啟 `/health` 成功。
- [ ] 手機使用 LAN IP 開啟 `/health` 成功。
- [ ] Host 可以建立活動。
- [ ] Join URL 使用目前正確的 LAN IP。
- [ ] 電腦瀏覽器可以加入活動。
- [ ] 手機掃描 QR Code 可以加入活動。
- [ ] Host 可以看到手機 Participant。
- [ ] 手機重新整理不會產生重複身份。
- [ ] Display 參與人數即時增加。
- [ ] Quiz 題目同步到手機與 Display。
- [ ] Reveal 前 Display 沒有正確答案。
- [ ] Reveal 後顯示統計與排行榜。

只要前三個 Health Check 都成功，就表示手機到主持人電腦的基本區域網路連線已建立。
