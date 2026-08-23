# EventHub 故障排除

以下採用「症狀 → 檢查順序 → 解決方式」說明。文件中的 `192.168.0.107` 都只是範例；實際使用時，請以活動電腦執行 `ipconfig` 後顯示的 Wi-Fi 或乙太網路 IPv4 位址為準。

## 手機掃描 QR Code 後完全打不開

依序檢查：

1. 看 QR Code 對應網址是否為 `localhost`。`localhost` 在手機上代表手機自己，不能連到活動電腦。
2. 看網址是否為 `0.0.0.0`。它只用來讓 Server 監聽所有網路介面，不能當成手機的目的網址。
3. 確認手機與活動電腦連到同一個區域網路。連到同一台 Wi-Fi 通常可以，但訪客 Wi-Fi 可能啟用 Client／AP Isolation，阻止裝置互連。
4. 在活動電腦執行 `ipconfig`，確認目前 IPv4。若是 `192.168.0.107`，手機測試網址範例為 `http://192.168.0.107:5000/health`。
5. 確認 Server 視窗仍在執行，並以 `Local Event` Profile 啟動。
6. 確認網址的 Port 與 Server 實際 Port 相同。`Local Event` 是 `5000`；VS 的 `http` Profile 是 `5029`。
7. 確認 Windows Defender 防火牆已允許 TCP 5000 的輸入連線。
8. 確認 Windows 網路設定檔是「私人網路」，且防火牆規則套用私人網路。
9. 若上述都正確，請向現場網路管理人員確認是否有 AP Isolation、用戶端隔離或 VLAN 隔離。

## Server 本機可以開，手機卻不能開

先在活動電腦依序測試：

```text
http://localhost:5000/health
http://192.168.0.107:5000/health
```

第二個位址只是範例，請換成實際 IPv4。

- 第一個成功、第二個失敗：通常是 Server 只監聽 localhost，或本機防火牆阻擋。請用 `Local Event` Profile，確認其 `applicationUrl` 是 `http://0.0.0.0:5000`。
- 兩個都成功、手機失敗：通常是 Windows 防火牆、Wi-Fi 裝置隔離、不同子網路或手機改走行動網路。
- 兩個都失敗：Server 可能未啟動、啟動失敗或 Port 不正確。請先查看 Visual Studio 的 Server 主控台輸出。

## 手機可以開頁面，但無法加入活動

1. 確認網址格式是 `/join/{JoinCode}`，而不是只開 Server 根網址。
2. 回到 Host 的 Event 畫面，核對 Join Code 與 Join URL。
3. 確認活動名稱及加入表單有正常顯示，再查看 Guest 畫面的錯誤訊息。
4. 查看 Server 主控台是否有 API 或資料庫錯誤。
5. 確認 Server 是從預期位置啟動；SQLite 資料預設位於 Server Content Root 下的 `Data/eventhub.db`。

目前新建 Event 預設可供參與者加入，但 Host 尚未提供完整的活動開放／關閉與開始／結束生命週期控制畫面。若需要這些控制，屬於目前尚未實作的後續功能。

## Host 顯示 Server Disconnected

1. 確認 `EventHub.Server` 還在執行。
2. 在瀏覽器開啟與 Host 相同 Base URL 的 `/health`。
3. 核對 Host 的 Server URL：`Local Event` 通常填 `http://localhost:5000`；VS `http` Profile 通常填 `http://localhost:5029`。
4. 若正在連線既有活動，核對 Event ID 與 Host Token；兩者都不是 Join Code。
5. 修正後重新按「連線監看」。

Host 目前不會自動替使用者判斷 Port，也沒有完整的 Event 清單與 Host Token 復原功能，請保存建立活動後顯示的 Event ID 與 Host Token。

## Display 顯示「無法連線：請輸入正確的 Event ID」

1. 按 `Esc` 回到 Display 連線設定畫面。
2. Server URL 填 API 位址，例如同機 `http://localhost:5000`。
3. Event ID 必須填 Host 建立活動後顯示的完整 GUID，不可填 Join Code。
4. 確認 Host 或瀏覽器可連到同一個 Server URL。
5. 按「連線展示」。

Display 連線後會透過 Current State Query 恢復目前畫面。Host Header 的 `Server Connected` 只表示 Host 已連線，不能用來證明 Display 已連線。

## Display 中斷或 Server 重啟後畫面不正確

1. 先保留 Display 執行，等待 SignalR 自動重新連線。
2. 確認 Server 已完全啟動，且原本的 SQLite `Data` 資料仍存在。
3. 若長時間沒有恢復，按 `Esc` 檢查 Server URL 與 Event ID，再重新連線。
4. Host 也重新連線同一個 Event，核對目前 Quiz 與 Display Mode。
5. 必要時由 Host 明確按 Waiting、Live、Result 或 Ranking，重新指定展示模式。

不要只靠錯過的 SignalR 通知判斷狀態；Display 正常設計會在重連後重新查詢 Server Current State。

## QR Code 仍然產生 localhost 網址

1. 停止 Server。
2. 執行 `ipconfig` 取得目前活動電腦 IPv4。
3. 修改 `EventHub.Server/Properties/launchSettings.json` 的 `Local Event` Profile：

```json
"EventHub__JoinBaseUrl": "http://192.168.0.107:5000"
```

上述 IP 只是範例，請換成實際 IPv4。

4. 重新以 `Local Event` 啟動 Server。
5. Host 使用 `http://localhost:5000` 建立或重新連線活動。
6. 核對新顯示的 Join URL，再用手機掃描 QR Code。

Host 與 Display 若和 Server 在同一台電腦，可以繼續使用 `localhost`；只有提供給手機的 Join URL 必須使用手機可到達的 LAN IP。

## 換電腦後活動資料不見了

目前專案尚未提供安裝程式、正式 Publish Profile 或資料移轉精靈。換機前應：

1. 關閉 Server，避免複製到尚未寫完的資料。
2. 備份完整 `Data` 資料夾，不能只複製單一畫面或設定檔。
3. 保存 Event ID 與 Host Token。
4. 在新電腦安裝 Visual Studio 2022 或 .NET 8 SDK，並還原整個 Repository／建置輸出。
5. 將 `Data` 放回新 Server 的正確 Content Root。
6. 重新執行 `ipconfig`；新電腦 IP 通常會不同。
7. 更新 `EventHub__JoinBaseUrl`，並重新建立防火牆規則。
8. 依 [GettingStarted.md](GettingStarted.md) 的開機前檢查重新測試。

## 換 Wi-Fi 後原本網址失效

DHCP 可能分配新的 IPv4。請：

1. 執行 `ipconfig` 查新 IP。
2. 更新 `Local Event` Profile 的 `EventHub__JoinBaseUrl`。
3. 確認 Windows 將新網路設為私人網路。
4. 確認 TCP 5000 防火牆規則仍套用目前網路設定檔。
5. 確認新 Wi-Fi 沒有 Client／AP Isolation。
6. 重啟 Server，重新連線 Host 與 Display，並核對 QR Code。

## Port 5000 已被占用

在命令提示字元或 PowerShell 執行：

```powershell
netstat -ano | findstr :5000
tasklist /FI "PID eq 1234"
```

`1234` 只是範例，請換成 `netstat` 最右欄顯示的 PID。

- 若是另一個自己啟動的 EventHub.Server，回到它的視窗按 `Ctrl+C` 或停止 Visual Studio 偵錯。
- 不要未確認程序身分就任意結束程序。
- 若必須改 Port，Server 的監聽 Port、`EventHub__JoinBaseUrl`、Host URL、Display URL 與防火牆規則都要一起調整。

## 防火牆已建立規則，手機仍連不上

開啟「Windows Defender 防火牆（進階安全性）」並檢查輸入規則：

- 規則已啟用。
- 動作是允許連線。
- 通訊協定是 TCP。
- 本機 Port 是 5000。
- Profile 包含目前使用的私人網路。
- 規則沒有被更高優先層級的公司政策封鎖。

請勿為了測試永久關閉整個防火牆。若公司電腦受群組原則管理，請由 IT 人員協助建立規則。

## 題庫無法匯入

1. 確認 Host 已連到 Server，並已建立或連線 Event。
2. 先用「匯出範本」取得目前系統格式，不要自行猜欄位名稱。
3. 確認 CSV 是 UTF-8、檔案不超過 5 MB、題數不超過 500。
4. 確認每題秒數介於 5 到 120 秒。
5. 確認 `Difficulty` 使用 `Easy`、`Medium` 或 `Hard`。
6. 確認 `Mode` 使用 `Practice` 或 `Scored`。
7. 檢查 Key、順序及題目是否重複，並查看預覽畫面的逐列錯誤。
8. 題目或選項若包含逗號、雙引號或換行，請使用標準 CSV 引號規則。

匯入會先顯示預覽，確認後 Server 仍會再次驗證。修正 CSV 後重新選檔即可，不要略過驗證直接修改資料庫。

## Quiz 主按鈕無法按或不是預期文字

1. 進入 `Question Bank`，確認已選擇題庫。
2. 確認題庫確實包含題目，而不是只有匯入失敗的預覽。
3. 進入 `Quiz Activity` 查看目前題目與狀態。
4. 題目 Open 時應先關閉作答，Closed 後才能公布答案。
5. 內建預設練習是獨立的兩題 Practice 題庫，完成後仍需選擇已匯入的正式題庫。
6. 要讓 Primary Action 從練習自然切換到正式比賽，匯入題庫本身需同時包含 Practice 與 Scored 題目。
7. 若 Host 曾斷線，重新連線 Event，讓畫面從 Server Current State 恢復。

目前題目狀態與按鈕行為詳見 [HostOperation.md](HostOperation.md)。

## Visual Studio 顯示很多錯誤，但命令列 Build 成功

這通常是 Visual Studio Designer 或 IntelliSense 的快取狀態，不代表原始碼一定壞掉。請依序嘗試：

1. 確認開啟的是 `Project_EventHub.sln`。
2. 關閉發生錯誤的 Designer 頁籤，再重新開啟。
3. 在 Visual Studio 執行「建置 → 重建方案」。
4. 關閉 Visual Studio 後重新開啟 Solution。
5. 若命令列 `dotnet build Project_EventHub.sln` 仍是 0 Error，可在 Visual Studio 關閉時刪除 Repository 內的 `.vs` 快取資料夾，再重新開啟。

請勿為了消除舊的 IntelliSense 訊息，直接覆寫 `.Designer.cs` 或把固定控制項改成 Runtime 動態建立。
