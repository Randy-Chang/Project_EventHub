# EventHub 使用說明書

本說明書適用於目前的 `Project_EventHub` 開發版本，主要說明如何使用 Visual Studio 2022 啟動 Server、Host Console 與 Display、建立活動、讓 Guest 加入，以及操作 Quiz 快問快答。

> 最容易忘記的重點：VS2022 的 `http` Profile（`localhost:5029`）只供同一台電腦測試。活動當天請使用 `Local Event` Profile 或 LAN 啟動命令，並把 `JoinBaseUrl` 設成活動主機的實際 IPv4；手機不能連 localhost。

## 1. 目前可以使用的功能

- Host 建立活動。
- Host 顯示持久化 Join Code、加入網址與 QR Code，並可複製網址。
- Guest 使用瀏覽器加入活動。
- Host 即時查看已加入與在線 Participant。
- Host 建立單選 Quiz 題目。
- Host 開始、關閉及公布 Quiz 答案。
- Guest 即時收到題目並提交答案。
- Host 即時查看作答人數。
- Guest 在公布答案後查看正確答案及自己的作答結果。
- Guest 重新整理網頁後恢復原本身份與 Quiz 狀態。
- Server 重啟後從 SQLite 恢復活動、Participant、題目、場次及答案。
- Display 可顯示 Waiting QR、題目倒數、作答結束、答案統計與 Top 10 排行榜。
- Display 可選擇投影螢幕、全螢幕顯示，並在 reconnect 後恢復 Server 保存的畫面模式。

目前尚未完成：

- Poll、Lucky Draw、Photo Wall、Image Quiz。
- Host Token 遺失後的管理畫面或恢復功能。

## 2. 最快啟動方式：Visual Studio 2022

### 2.1 開啟 Solution

1. 開啟 Visual Studio 2022。
2. 選擇「開啟專案或方案」。
3. 開啟 Repository 根目錄的 `Project_EventHub.sln`。
4. 等待 NuGet 套件還原完成。
5. 按 `Ctrl + Shift + B` 建置方案。

建置成功時，Visual Studio 的輸出視窗應顯示 0 個錯誤。

### 2.2 設定多個啟始專案

第一次使用時設定一次即可：

1. 在方案總管對 Solution `Project_EventHub` 按右鍵。
2. 選擇「設定啟始專案」。
3. 選擇「多個啟始專案」。
4. 將 `EventHub.Server` 設定為「啟動」。
5. 將 `EventHub.Host` 設定為「啟動」。
6. 將 `EventHub.Display` 設定為「啟動」。
7. 其他 Project 設定為「無」。
8. 確認 `EventHub.Server` 使用 `http` Profile。
9. 按「確定」。

### 2.3 啟動

1. 按 `F5`，或按 Visual Studio 上方的綠色啟動按鈕。
2. 等待 Server Console 顯示：

```text
Now listening on: http://localhost:5029
```

3. Host Console 應同時開啟。
4. 在 Host Console 的 `Server URL` 輸入：

```text
http://localhost:5029
```

若要確認 Server 是否正常，可在同一台電腦的瀏覽器開啟：

```text
http://localhost:5029/health
```

正常時會看到類似：

```json
{"status":"ok"}
```

## 3. 建立活動

1. 確認 Host Console 的 Server URL 為 `http://localhost:5029`。
2. 輸入活動名稱。
3. 選擇活動日期。
4. 按「建立活動」。
5. 建立成功後，畫面會自動填入：
   - 活動 ID
   - Host Token
   - 活動名稱與 Join Code
   - Join URL 與 QR Code
6. Host 會自動連接 SignalR，不需要再次按「連線監看」。

請立即將活動 ID 與 Host Token 保存到安全的位置，例如活動專用的文字檔。

- Join URL／QR Code：提供 Guest 加入活動使用。
- 活動 ID：系統內部識別及 Host 重新連線使用，不需公開給 Guest。
- Host Token：只有 Host 管理操作使用，不可公開給 Guest。
- 目前若 Host Token 遺失，還沒有管理畫面可以找回。

如果按下「建立活動」後活動 ID 與 Host Token 仍是空白，代表活動沒有建立成功。先檢查 Server URL 與 Port，不要直接按「連線監看」。

## 4. Guest 加入活動

### 4.1 在同一台電腦測試

直接開啟 Host Console 顯示的 Join URL：

```text
http://localhost:5029/join/<Join Code>
```

例如：

```text
http://localhost:5029/join/8K3F2A
```

Guest 填寫：

- 姓名（必填）
- 暱稱
- 員工編號
- 部門
- 桌次

按「加入活動」後，Host Console 的 Participant 清單應立即出現此人。

在電腦測試時可按「複製網址」或直接掃描畫面 QR Code；但 localhost QR Code 只能在同機測試，手機無法連線，Host 會顯示警告。

### 4.2 Guest 身份與重新整理

Guest Browser 會將活動身份保存在 LocalStorage。

- 重新整理網頁不會建立新 Participant。
- Wi-Fi 短暫斷線後，SignalR 會嘗試重新連線。
- 重連後會重新向 Server 取得目前 Quiz 狀態。
- 換瀏覽器、無痕視窗、清除網站資料或換手機，會被視為新的 Browser 身份。

不要隨意清除瀏覽器網站資料，否則原本的 Participant 身份可能無法自動恢復。

## 5. Quiz 快問快答

### 5.1 建立題目

1. 確認活動已建立且 Host 已連線。
2. 在「Quiz 快問快答」區輸入題目。
3. 輸入 A、B、C、D 選項。
4. 第一版支援 2～4 個選項；不需要的 C 或 D 可以留空。
5. 在「正確答案」選擇 A、B、C 或 D。
6. 設定作答秒數，允許範圍為 3～300 秒。
7. 按「建立題目」。

建立成功後，狀態會顯示 `Waiting`，且「開始題目」按鈕會啟用。

正確答案必須是實際有填寫的選項。例如只有 A、B 兩個選項時，不可將正確答案設為 C 或 D。

### 5.2 開始題目

1. 按「開始題目」。
2. Server 會建立 Question Session，並決定開始時間與截止時間。
3. 已連線的 Guest 不需要重新整理，會立即看到題目與選項。
4. Host 狀態變成 `Open`。
5. Host 可看到：
   - 已作答人數
   - 已加入 Participant 總數
   - 在線人數

Guest 畫面倒數只供顯示，真正是否逾時由 Server 時間判斷。

### 5.3 Guest 作答

1. Guest 點選一個選項。
2. Server 接受後，Guest 顯示「答案已送出」。
3. 選項會被鎖定，避免重複點擊。
4. 同一位 Participant 對同一個 Question Session 只能保存一份答案。

即使 Client 重複 Request，Server 與 SQLite 唯一約束仍會阻止重複答案。

### 5.4 關閉作答

1. Host 按「關閉作答」。
2. 狀態變成 `Closed`。
3. Guest 顯示「本題作答結束」。
4. Server 會拒絕所有後續答案。

如果倒數已經結束，即使 Host 尚未按關閉，Server 仍不接受逾時答案。

### 5.5 公布答案

1. Question 必須先處於 `Closed`。
2. Host 按「公布正確答案」。
3. 狀態變成 `Revealed`。
4. Guest 會看到：
   - 正確答案
   - 自己選擇的答案
   - 答對、答錯或未作答

正確答案在 `Revealed` 之前不會傳送給 Guest。

### 5.6 Quiz 狀態說明

```text
Waiting  題目已建立，尚未開始
Open     開放作答
Closed   已停止作答，尚未公布答案
Revealed 已公布正確答案
```

### 5.7 大螢幕 Display

1. 啟動 `EventHub.Display`。
2. Server API URL 輸入與 Host 相同的 Server，例如本機測試使用 `http://localhost:5029`。
3. Event ID 輸入 Host Console 顯示的活動 ID。
4. 選擇投影機／第二螢幕；沒有第二螢幕時可選 Primary 並取消「全螢幕」。
5. 按「連線展示」。
6. Host 的「大螢幕控制」可切換等待、題目、結果與排行榜。

操作關係：

- Host 開始題目：Display 自動顯示題目與 Server-time countdown。
- Host 關閉作答：Display 顯示「時間到，等待公布答案」，不顯示正確答案。
- Host 公布答案：Display 自動顯示正確答案、答對率與選項分布。
- Host 按「顯示排行榜」：Display 顯示 Top 10。
- Host 按「顯示等待畫面」：Display 回到 QR Code 與加入人數。

Display 快捷鍵：

- `F11`：切換全螢幕。
- `Esc`：離開全螢幕並顯示連線設定。

Display 關閉重開、SignalR reconnect 或 Server restart 後，都會呼叫 Current Display State API 恢復 Server 保存的畫面。Display 不會自行在 Result、Leaderboard、Waiting 間定時跳轉。

## 6. 使用手機進行區域網路測試

VS2022 的 `localhost:5029` 模式主要供同一台電腦測試。手機不能使用 `localhost`，因為手機上的 `localhost` 代表手機自己。

需要手機加入時，請先停止 VS2022 中的 Server，以 `ipconfig` 找到 Host 電腦 Wi-Fi IPv4，再於 Repository 根目錄開啟 PowerShell。以下以 `192.168.1.20` 為例：

```powershell
$env:EventHub__JoinBaseUrl = "http://192.168.1.20:5000"
dotnet run --project src/EventHub.Server/EventHub.Server.csproj --urls http://0.0.0.0:5000
```

若使用 VS2022，也可選擇 Server 的 `Local Event` Profile；啟動前請先將 `src/EventHub.Server/Properties/launchSettings.json` 中的範例 IP `192.168.1.100` 改為實際 IPv4。

接著：

1. 使用 `ipconfig` 找出 Host 電腦 Wi-Fi 網卡的 IPv4 位址，例如 `192.168.1.20`。
2. Host Console 的 Server URL 可使用同機 API 位址：

```text
http://localhost:5000
```

3. 建立活動後，確認 Host 顯示的 Join URL 是實際 LAN IP，例如：

```text
http://192.168.1.20:5000/join/8K3F2A
```

4. 用手機掃描 Host 顯示的 QR Code；瀏覽器會直接進入該活動的加入頁。

5. 手機與 Host 電腦必須連接同一個 Wi-Fi。
6. Windows Firewall 必須允許 TCP Port 5000 的 Private Network inbound traffic。
7. Wi-Fi AP 不可啟用 Client Isolation／AP Isolation。

區域網路模式的重點是：Server 必須監聽 `0.0.0.0:5000`，而 `JoinBaseUrl` 必須使用手機可達的 LAN IP。Host 的 API Base URL 可以使用 localhost。

## 7. 關閉與再次啟動

### 7.1 關閉

1. 先停止活動操作。
2. 關閉 Host Console。
3. 回 Visual Studio 按 `Shift + F5` 停止 Server，或在 Server Terminal 按 `Ctrl + C`。

### 7.2 再次啟動

1. 先啟動 Server。
2. 再啟動 Host Console。
3. 輸入與 Server 相同的 URL。
4. 將先前保存的活動 ID 與 Host Token 填回 Host Console。
5. 按「連線監看」。

Host 會向 Server 重新取得相同 Join Code／Join URL，並重新產生 QR Code；QR 圖片本身不存入資料庫。

Server 重啟不會刪除活動資料，但 Host Console 目前不會自動保存活動 ID 與 Host Token，因此必須由使用者另外保存。

## 8. SQLite 資料位置

預設資料庫位置：

```text
src/EventHub.Server/Data/eventhub.db
```

若要備份，請先完全停止 Server，再複製整個 `Data` 目錄。Server 執行期間可能存在 SQLite WAL 檔案，不建議只複製單一 `eventhub.db`。

## 9. 常見問題

### 9.1 Host 顯示「操作失敗」，活動 ID 與 Token 是空白

可能原因：Host Console 的 Server URL 或 Port 錯誤。

VS2022 `http` Profile 請使用：

```text
http://localhost:5029
```

先開啟以下網址確認 Server：

```text
http://localhost:5029/health
```

若 `/health` 無法開啟，請確認 `EventHub.Server` 是否正在執行。

### 9.2 Host 使用 5090、5000 或其他 Port 連不上

Host Console 不會自動猜測 Server Port。請以 Server Console 的 `Now listening on:` 為準。

- VS2022 `http` Profile：通常是 `5029`。
- 本說明書的區域網路 PowerShell 命令：固定是 `5000`。

### 9.3 手機無法開啟 Guest 網頁

依序檢查：

1. 手機網址不可使用 `localhost`。
2. 手機與電腦是否在同一個 Wi-Fi。
3. Server 是否以 `http://0.0.0.0:5000` 啟動。
4. Host 顯示的 Join URL 是否為目前正確的 LAN IPv4，而不是 localhost 或舊 IP。
5. Windows Firewall 是否允許 Port 5000。
6. Wi-Fi 是否啟用 Client Isolation。

### 9.4 Guest 加入了，但 Host 沒有出現

1. 確認 Guest 是從 Host 顯示的同一個 Join URL／QR Code 進入。
2. 確認 Host、Guest 使用同一台 Server 與同一個 Port。
3. 確認 Host Console 顯示已連線。
4. 重新按 Host 的「連線監看」，讓 Host 從 REST 重新載入 Participant 清單。

### 9.5 Guest 重新整理後變成另一個人

正常的同一瀏覽器重新整理不會產生新身份。若身份不同，通常是因為：

- 使用無痕模式。
- 換了 Browser。
- 清除了網站資料或 LocalStorage。
- 使用不同 Server Host Name，例如先使用 `localhost`，之後改用 `192.168.1.20`。

### 9.6 Quiz 題目按鈕無法操作

- 「開始題目」：必須先建立題目。
- 「關閉作答」：只有 `Open` 時可用。
- 「公布正確答案」：只有 `Closed` 時可用。
- 若狀態不一致，確認 Server 仍在線，然後重新按「連線監看」。

## 10. 每次活動前快速檢查表

- [ ] `Project_EventHub.sln` 建置成功。
- [ ] Server Console 已顯示 `Now listening on:`。
- [ ] Host Console 使用相同的 Server URL 與 Port。
- [ ] `/health` 回傳 `ok`。
- [ ] 活動 ID 與 Host Token 已另外保存。
- [ ] Join URL 顯示實際 LAN IP，沒有 localhost 警告。
- [ ] 手機可掃描 QR Code 並顯示正確活動名稱。
- [ ] 測試 Guest 可以加入。
- [ ] Host 可以即時看到 Guest。
- [ ] Display Waiting 畫面顯示 EventName、QR Code、Join Code 與 Participant Count。
- [ ] Host Start／Close／Reveal 時 Display 顯示正確階段，Reveal 前沒有正確答案。
- [ ] Host 可切換排行榜與等待畫面。
- [ ] Display 關閉重開後可恢復目前畫面。
- [ ] 測試 Quiz 建立、開始、作答、關閉及公布答案。
- [ ] Browser Refresh 後可以恢復身份與 Quiz 狀態。
- [ ] 手機與 Host 使用相同 Wi-Fi。
- [ ] Windows Firewall 與 Wi-Fi Client Isolation 已確認。
- [ ] 正式活動前已使用實際場地網路完成演練。

## 11. 一分鐘版操作流程

```text
開啟 Project_EventHub.sln
→ F5 啟動 Server 與 Host
→ Host Server URL 填 http://localhost:5029
→ 建立活動
→ 保存活動 ID 與 Host Token
→ Display 輸入 Server URL 與 Event ID 並連線
→ Guest 開啟 Join URL／掃描 QR Code 並加入
→ Host 建立 Quiz 題目
→ 開始題目
→ Guest 作答
→ Host 關閉作答
→ Host 公布正確答案
```
