# 完全第一次使用 EventHub 的操作指南

這份指南適合第一次在 Windows 電腦啟動 EventHub，且不熟悉 Server、IP、Port 或區域網路的人。以下所有 `192.168.0.107` 都只是範例；活動當天必須使用該電腦真正的 IPv4。

## 1. EventHub 怎麼運作

`EventHub.Server` 是系統中心。這裡的 Server 不一定是昂貴的伺服器；活動現場那台 Windows 電腦執行 `EventHub.Server` 後，本身就是 Server。

```text
手機 Guest ─┐
Host ───────┼─→ EventHub.Server ─→ SQLite
Display ────┘
```

Host、Display 與手機都是 Client，也就是向 Server 取得資料的使用端。手機不是直接連到 Host；因此 Server 沒有啟動時，Host、Display 和手機都不能正常使用。

## 2. 活動電腦需要準備什麼

- Windows 電腦一台。
- Visual Studio 2022（開發階段建議）或 .NET 8 SDK。
- Repository 完整檔案，包含 Server、Host、Display 與 Guest Web。
- 可用的有線 LAN 或 Wi-Fi 無線基地台／Router。
- 至少 2～3 支測試手機。
- 投影機、電視或第二螢幕（要測 Display 時）。

目前尚未提供 Publish Profile、安裝程式或可直接複製到新電腦的一鍵部署包。第一次換電腦仍需使用 Visual Studio 2022 或 `dotnet run`。

## 3. 找出活動電腦的 IPv4

IP Address 可以先理解成「這台電腦在目前區域網路裡的地址」。

1. 按 `Win + R`。
2. 輸入 `cmd` 後按 Enter。
3. 執行：

```cmd
ipconfig
```

4. 找到目前真正有連線的 `Wi-Fi` 或 `乙太網路` 區段。
5. 記下 `IPv4 位址`，例如 `192.168.0.107`。

請忽略顯示「媒體已中斷連線」的網卡，也不要使用 `127.0.0.1`、虛擬機器或 VPN 網卡的位址。`192.168.0.107` 只是範例；更換電腦、Wi-Fi 或 Router 後很可能不同。

## 4. 第一次建置 Solution

1. 開啟 Visual Studio 2022。
2. 選擇「開啟專案或方案」。
3. 開啟 Repository 根目錄的 `Project_EventHub.sln`。
4. 等待 NuGet 套件還原完成。
5. 選擇「建置 → 建置方案」，或按 `Ctrl + Shift + B`。
6. 確認輸出為 0 Error。

## 5. 選擇正確的 Server 啟動模式

### 只在同一台電腦測試

將 `EventHub.Server` 設成啟始專案，選擇 `http` Profile。它會使用：

```text
http://localhost:5029
```

`localhost` 代表「目前這台裝置自己」。這適合 Server、Host、Display 都在同一台電腦的開發測試，但手機上的 localhost 是手機自己，所以手機不能用此 QR Code。

### 手機與活動電腦一起測試

1. 開啟 `src/EventHub.Server/Properties/launchSettings.json`。
2. 找到 `Local Event` Profile。
3. 將 `EventHub__JoinBaseUrl` 的 IP 改成步驟 3 查到的 IPv4。例如以下只是範例：

```json
"EventHub__JoinBaseUrl": "http://192.168.0.107:5000"
```

4. 在 Visual Studio 上方選擇 `Local Event`。
5. 按 `F5` 或 `Ctrl + F5` 啟動 `EventHub.Server`。

`Local Event` 會讓 Server 監聽 `http://0.0.0.0:5000`。`0.0.0.0` 代表接受所有本機網路介面的 Port 5000 連線，不是手機要輸入的網址。

如果不使用 Visual Studio，也可在 Repository 根目錄開啟 PowerShell。以下 IP 仍只是範例：

```powershell
$env:EventHub__JoinBaseUrl = "http://192.168.0.107:5000"
dotnet run --project src/EventHub.Server/EventHub.Server.csproj --urls http://0.0.0.0:5000
```

## 6. 確認 Server 正常

Server Console 應顯示類似：

```text
Now listening on: http://0.0.0.0:5000
```

同一台電腦的瀏覽器開啟：

```text
http://localhost:5000/health
```

正常時會看到：

```json
{"status":"ok"}
```

如果使用 `http` 開發 Profile，改測 `http://localhost:5029/health`。Console 的 `Now listening on:` 才是判斷實際 Port 的依據。

## 7. 啟動 Host 並建立活動

1. 啟動 `EventHub.Host`。
2. 左側選擇 `Event`。
3. `Server URL` 輸入 Host 要連的 Server：
   - `Local Event` 同機執行：`http://localhost:5000`
   - `http` Profile：`http://localhost:5029`
4. 輸入「活動名稱」。
5. 選擇「活動日期」。
6. 按「建立活動」。

建立成功後，Host 會自動填入活動 ID、Host Token、Join Code、Join URL 與 QR Code，並連線監看 Participant。請將活動 ID 與 Host Token 保存到安全位置；目前尚未實作 Token 找回畫面。

若要在 Server Restart 後連回既有活動，填入 Server URL、活動 ID、Host Token，按「連線既有活動」。

目前 Host 尚未提供活動開放／關閉、開始／結束的操作 UI；新活動建立後預設可以加入。

## 8. 用手機測試 QR Code

建立活動後先確認 Join URL。例如以下只是範例：

```text
http://192.168.0.107:5000/join/8K3F2A
```

它必須符合：

- IP 是活動電腦目前的 LAN IPv4。
- Port 與 Server 相同，此處是 5000。
- 不是 `localhost`。
- 不是 `0.0.0.0`。

讓手機連上與 Server 相同的 LAN／Wi-Fi，掃描 QR Code。Guest 頁面可輸入姓名（必填）、暱稱、員工編號、部門與桌次。加入後，Host Participant 清單與人數應立即更新。

## 9. 啟動 Display

1. 啟動 `EventHub.Display`。
2. `Server API URL` 輸入 Server 位址；同機使用 `http://localhost:5000` 或目前 Profile 的 Port。
3. 將 Host 顯示的活動 ID 複製到 Display 的 Event ID。
4. 選擇 Primary 或 Secondary 螢幕。
5. 只有一個螢幕時，可取消「全螢幕」方便測試。
6. 按「連線展示」。

Display 的 `appsettings.json` 預設 Server URL 是 `http://localhost:5000`、Event ID 是空白、優先選擇非 Primary 螢幕，並預設全螢幕。畫面欄位仍可在連線前修改。`F11` 切換全螢幕，`Esc` 離開全螢幕並顯示設定區。

Host Header 的 `Server Connected` 表示 Host 已連到 Server，不代表 Display 已被獨立偵測。判斷 Display 是否成功，應看 Display 自己的連線訊息及是否顯示活動 Waiting 畫面。

## 10. 匯入題庫並測試 Quiz

1. Host 左側選擇 `Question Bank`。
2. 按「匯出範本」，取得 UTF-8 BOM CSV 範本；也可使用 `samples/QuizQuestionBankSample.csv`。
3. 編輯後按「匯入 CSV」。
4. Server 先顯示 Preview；沒有錯誤時按確認才會匯入。
5. 選擇題庫與題目，再進入 `Quiz Activity`。
6. 使用下方唯一的主要操作按鈕依序操作：開始本題、關閉作答、公布答案、下一題。

CSV 必須包含：

```text
QuestionKey,QuizTitle,Category,Difficulty,Order,Question,OptionA,OptionB,OptionC,OptionD,CorrectOption,DurationSeconds,Explanation,Mode
```

`Mode` 只接受 `Practice` 或 `Scored`。Practice 不計分；Scored 使用正式計分與排行榜。

若尚未匯入題庫，可在 Quiz Activity 按「執行內建熱身」。這會建立獨立的兩題 Practice 題庫，不會插入正式題庫。因為內建題庫只有 Practice，完成後要回 Question Bank 選擇已匯入的正式題庫。若同一個匯入題庫同時含 Practice 與 Scored，最後一題 Practice 公布後可直接使用「開始正式比賽」。

## 11. 活動開始前最低測試

至少使用 2～3 支真實手機完成：

- [ ] 每支手機都能掃 QR Code 並加入。
- [ ] Host Participant Count 與在線人數正確。
- [ ] Display Waiting 顯示活動名稱、QR Code、Join Code 與人數。
- [ ] Practice／Scored 題目可 Start。
- [ ] 手機可 Answer，Host／Display 作答數更新。
- [ ] Host 可 Close，逾時答案被 Server 拒絕。
- [ ] Host 可 Reveal，Guest／Display 才看到正確答案。
- [ ] Result 統計與 Leaderboard 正常。
- [ ] 手機重新整理後仍是同一 Participant。
- [ ] Display 關閉重開後能恢復 Server Current State。
- [ ] 暫時切斷再恢復網路後，Host／Display 能重新同步。

正式活動前至少完整跑完一次，不要只測到「網頁打得開」。
