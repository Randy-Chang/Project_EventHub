# 主持人 Host 操作手冊

這份文件只說明目前 Phase 4.8.1 Host Console 的實際操作。網路原理請參考 [NetworkSetup.md](NetworkSetup.md)。

## Host Layout

Host 主要分為五個區域：

1. **Global Header**：顯示 Current Event、Activity、Quiz、Participant 數、Server 狀態、Display Mode 與 Activity State。
2. **左側 Navigation**：`Dashboard`、`Event`、`Question Bank`、`Quiz Activity`。
3. **Activity Workspace**：中央主要操作內容。
4. **Live Monitor／Display Control**：右側顯示題次、Mode、State、倒數、作答／在線人數，以及 Waiting、Live、Result、Ranking 按鈕。
5. **Primary Action**：底部唯一主要流程按鈕，依 Server 狀態變成開始、關閉、公布、下一題或排行榜。

Header 的 `Server Connected` 代表 Host 已連到 Server，不是 Display Client 的獨立在線指示。

## Event：建立或連線活動

### 建立新活動

1. 左側選擇 `Event`。
2. 確認 Server URL：Local Event 同機通常是 `http://localhost:5000`；VS `http` Profile 是 `http://localhost:5029`。
3. 輸入活動名稱。
4. 選擇活動日期。
5. 按「建立活動」。

成功後會顯示活動 ID、Host Token、Join Code、Join URL、QR Code 與 Participant 清單。Join URL 可按「複製加入網址」。若畫面警告使用 localhost，該 QR Code 只能供同機測試，手機不能使用。

### 連線既有活動

Server Restart 或 Host 重開後：

1. 填入 Server URL。
2. 填入先前保存的活動 ID。
3. 填入 Host Token。
4. 按「連線既有活動」。

Host 會重新取得 Join Info、Participant、Quiz Current State、Display State 與題庫。切換活動時會清除上一個活動的畫面資料。

目前尚未實作活動列表、Host Token 找回，以及活動開放／關閉、開始／結束的 Host UI。

## Question Bank：CSV 題庫

### 匯出範本

按「匯出範本」，選擇存檔位置。此操作直接向 Server 的公開 Template Endpoint 下載，不要求活動已建立。範本為 UTF-8 BOM、CRLF，包含 2 題 Practice 與 3 題 Scored 範例。

### 匯入 CSV

1. Host 必須先連到活動。
2. 按「匯入 CSV」並選擇檔案。
3. Server 執行 Preview，畫面顯示題數、錯誤與警告。
4. Preview 有錯誤時不能匯入。
5. 確認後 Server 會再次驗證並整批寫入；任一題失敗時整批不寫入。
6. 匯入成功後，可從下拉選單切換 Quiz，並用上一題／下一題瀏覽。

主要限制：

- 必須是 UTF-8 `.csv`，最大 5 MB、最多 500 題。
- 一個檔案只能有一個 `QuizTitle`。
- 同活動的題庫名稱不可重複。
- `QuestionKey` 與 `Order` 在同題庫不可重複。
- `Difficulty`：`Easy`、`Medium`、`Hard`，目前不影響計分。
- `Mode`：`Practice` 或 `Scored`。
- `CorrectOption`：A～D。
- CSV 匯入作答時間：5～120 秒。

欄位順序：

```text
QuestionKey,QuizTitle,Category,Difficulty,Order,Question,OptionA,OptionB,OptionC,OptionD,CorrectOption,DurationSeconds,Explanation,Mode
```

## Default Practice：內建熱身

Quiz Activity 左側提供「執行內建熱身」：

- Server 建立或沿用獨立的 `EventHub 內建操作練習` 題庫。
- 固定兩題 Practice。
- Practice 答題會保存結果與統計，但正式分數、正式排行榜皆不增加。
- 不會把題目插入已匯入的 Quiz。
- 目前題庫已含 Practice 時，該入口不顯示。

內建熱身只有 Practice 題。完成後請回 Question Bank 選擇已匯入的正式題庫；它不能直接帶你進入另一個獨立題庫的正式題。

## Quiz Activity：完整流程

選擇題庫後，正常操作不需要在多個結果頁面間來回切換。中央 Quiz Activity 同時包含題目、結果與排行榜頁籤；底部 Primary Action 根據 Server 狀態決定下一步。

### Primary Action 對照

| 目前狀態 | 按鈕文字 | 行為 |
|---|---|---|
| 未選題庫 | `請先選擇題庫` | 停用；先匯入／選擇題庫或執行內建熱身 |
| Waiting／題目已選 | `開始本題` | Server 開放作答，Display 進入 Question |
| Open | `關閉作答` | 停止作答，Display 顯示作答結束 |
| Closed | `公布答案` | Server 計分，Guest／Display 顯示答案與結果 |
| Revealed／同 Mode 尚有題目 | `下一題` | 選擇下一題，之後再按開始本題 |
| 同一匯入 Quiz 的最後 Practice | `開始正式比賽` | 直接開始該 Quiz 第一題 Scored；沒有 Scored 時停用 |
| 最後一題 Scored | `查看最終排行榜` | Display 與 Host 顯示最終排行榜 |

一般混合題庫流程：

```text
Practice：開始本題 → 關閉作答 → 公布答案 → 下一題
最後 Practice → 開始正式比賽
Scored：關閉作答 → 公布答案 → 下一題
最後 Scored → 查看最終排行榜
```

Practice 為 0 分。Scored 目前答對基礎分 500，最高速度 Bonus 500，答錯 0 分。

## Live Monitor 與 Display Control

右側 Live Monitor 顯示 Activity、題次、PRACTICE／SCORED Mode、WAITING／OPEN／CLOSED／REVEALED State、Server deadline 倒數、Answered 與 Online 人數，以及目前 Display Mode。

Display 控制按鈕：

- `Waiting`：活動名稱、QR Code、Join Code、Participant Count。
- `Live`：目前題目；Closed 時顯示作答結束。
- `Result`：公布後的正確答案、答對率與選項分布。
- `Ranking`：Top 10 排行榜。

正常 Quiz Flow 中，Start 會把 Server Display Mode 設為 Question，Reveal 會設為 Result；Close 仍在 Question Mode 顯示 Closed 畫面。最後排行榜由主要操作按鈕切換。右側四個按鈕是主持人需要時的 Manual Override，例如中場回 Waiting。

Display 不會自行在 Result、Ranking、Waiting 間依秒數跳轉。

## 緊急操作與恢復

- **Host 暫時斷線**：SignalR 自動重連後會查詢 Current State。若未恢復，重新填活動 ID／Host Token 後按「連線既有活動」。
- **Display 暫時斷線**：會顯示重新連線訊息並保留最後畫面；重連後查詢 Current Display State。
- **Display 重開**：重新輸入相同 Server URL 與 Event ID，按「連線展示」。
- **Participant 重新整理**：同一 Browser 的 LocalStorage Token 會恢復同一身份；換 Browser、無痕模式或清除網站資料則可能成為新身份。
- **Server Restart**：活動、Participant、Quiz、答案、分數及 Display Mode 保存在 SQLite；Presence 連線狀態由 Client 重連重新建立。

如果 Server 完全停止，Host 與 Display 都不能替代 Server 繼續控制活動。
