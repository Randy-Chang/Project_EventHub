# EventHub 產品目標與核心功能

## 1. 文件目的

本文件定義 EventHub 朝「接近商用活動互動平台」發展時應具備的核心能力、活動模組、品質要求與開發順序，作為後續需求討論及 Roadmap 的依據。

目標是做出可在尾牙、春酒、家庭日、婚禮或企業活動現場穩定使用的軟體，而不是複製特定廠商的介面、素材或專有流程。拍拍印公開產品呈現的範圍包含活動報名／報到、手機拍貼、照片套框與列印、雲端相簿、大螢幕互動、抽獎及快問快答，可作為功能面向的市場參考：[拍拍印互動服務總覽](https://linein.cc/zh/event/service)、[拍拍印特色功能](https://linein.cc/zh/wedding/features)、[拍拍印企業版](https://enterprise.linein.cc/)。

## 2. 產品定位與範圍假設

EventHub 定位為：

> 在單一活動場地的 LAN 中，由一台 Windows 活動電腦集中控制，讓主持人、投影幕與參與者手機即時互動的活動平台。

目前先以個人維護、非 SaaS 營運為假設，因此追求的是「商用品質的現場可靠性」，而不是商業公司的營運規模。

### 需要做到

- 一位操作人員能在活動前完成設定、演練、備份及設備檢查。
- 活動當天即使沒有 Internet，核心流程仍可在 LAN 中執行。
- 手機不需安裝 App，以掃描 QR Code 進入為主。
- Server 是活動狀態、答案、分數、抽獎與列印工作的唯一可信來源。
- Host、Display、手機重新連線後能恢復目前狀態。
- 發生操作錯誤、斷線、Server restart 或 printer failure 時，有清楚且安全的恢復方式。

### 現階段不需要做到

- 多租戶 SaaS、訂閱付款、方案計價、客戶帳務與客服工單。
- 公開 Marketplace、第三方 Plugin 平台或讓陌生使用者自行註冊開活動。
- 跨地區分散式部署、Kubernetes、Message Broker 或大型雲端架構。
- LINE 官方帳號、社群平台登入、CRM 或行銷自動化整合。
- AI 去背、AI 修圖、AI 生成影像、360 環景影片或智慧硬體整合。
- 完整活動企劃、婚禮電子喜帖、宴會廳導航等非現場互動核心功能。

若未來確定要公開分享或讓其他人自行安裝，再另行評估安裝程式、升級機制、授權、文件在地化與使用者支援。

## 3. 核心平台能力

以下不是獨立遊戲，而是所有活動模組共用的底座。

### 3.1 活動管理

- 建立、編輯、複製、封存及查看活動。
- 明確的活動生命週期：`Draft`、`Ready`、`Joining`、`Running`、`Completed`。
- 開放／停止加入、開始／完成活動，並防止不合法的狀態轉換。
- 保存活動名稱、日期、主視覺、Logo、色彩、加入規則及各模組設定。
- 活動前 Preview／Rehearsal Mode，測試資料不得污染正式結果。
- 產生活動檢查表，顯示 Server、LAN、Display、題庫、獎項、照片儲存空間與 printer readiness。

### 3.2 Host 身份與權限

- 安全保存並恢復最近主持的活動，不要求操作人員手抄 Token。
- Host Credential 可撤銷、重設或輪替，且不得顯示在投影或 Guest 端。
- 區分至少兩種操作權限：可變更流程的 Host，以及只能查看狀態的 Operator／Observer。
- 重要且不可逆操作需二次確認，例如完成活動、清空測試資料、重抽或取消列印。

### 3.3 Participant 與報到

- QR Code／Join Code 加入，支援姓名、暱稱、員工編號、部門、桌次等可設定欄位。
- 欄位必填、格式、唯一性及是否允許匿名皆由 Server 驗證。
- 名單預先匯入、現場報到、手動新增與 CSV 匯出。
- 同一身份避免重複加入；換手機時可由 Host 協助恢復或重新綁定。
- 顯示已報到、在線、離線及最後連線時間，但 Presence 不作為抽獎唯一資格依據。
- 可依部門、桌次、隊伍、標籤或報到狀態篩選活動資格。

### 3.4 活動流程控制

- Host 有單一清楚的「下一步」與目前活動狀態，不必在多個頁面猜測操作順序。
- 建立可排序的活動流程，例如：報到 → 暖場投票 → Quiz → 抽獎 → 照片牆 → 結束。
- 每個活動項目支援準備、開始、暫停／關閉、公布結果及完成等適用狀態。
- 提供手動 Display Override，但不得因此改變 Quiz、Poll 或 Draw 的 Domain State。
- 所有 Client 均可透過 Current State API 恢復，不依賴曾收到的 SignalR Event。

### 3.5 Display 與品牌視覺

- Waiting、Live、Result、Ranking、Photo Wall、Poll、Draw 等模式。
- 1920×1080 為設計基準，並支援常見 16:9 解析度、Windows DPI 與全螢幕。
- 主題色、Logo、背景、字型尺寸及安全邊界可於活動層級設定。
- Host 可預覽下一個畫面，避免未準備內容直接投影。
- 投影端斷線時顯示可辨識的 fallback 畫面；重連後自動回復 Server current state。

### 3.6 稽核、診斷與復原

- Audit Log 記錄操作者、時間、活動、命令與結果，至少涵蓋活動狀態、題目、抽獎、照片審核及列印操作。
- Host 顯示 Server、SignalR、Display、Storage 與 Printer 的健康狀態。
- SQLite 與照片檔案支援一致的備份、還原及活動封存。
- Server 啟動時檢查 migration、儲存空間、設定與未完成狀態，無法安全啟動時提供明確原因。
- 匯出活動結果：Participant、Quiz、Poll、Draw、Photo metadata、Print Job 與 Audit Log。
- 提供敏感資料清除與活動資料保存期限設定。

## 4. 活動模組

### 4.1 Quiz 快問快答

目前已有基礎版本，商用品質仍應包含：

- CSV 題庫 Preview、驗證、Practice／Scored 題目及題序管理。
- 文字題與圖片題，選項可包含圖片。
- Server deadline、重複作答保護、公布前隱藏答案、計分與排行榜。
- 可設定速度分、固定分、答錯不扣分等清楚的計分規則。
- 題目統計、未作答名單、結果匯出及重連恢復。
- 正式場次開始後鎖定會影響公平性的題目與計分設定。

### 4.2 Poll 即時投票

- 單選、多選與是非投票。
- 可設定匿名或記名、每人票數、開始及截止時間。
- 結果可即時顯示或由 Host 關閉後公布。
- Server 防止超額與重複投票，並保存原始票與統計快照。
- Display 支援票數、百分比與長條圖；Host 可查看參與率。

### 4.3 Lucky Draw 摸彩抽獎

- 匯入或依 Participant 規則計算 eligible pool。
- 依部門、桌次、標籤、報到狀態或尚未中獎條件篩選。
- 獎項、名額、抽取順序及是否允許重複中獎可設定。
- Draw command 必須 idempotent，Server 產生並持久化唯一結果。
- 中獎後支援確認、放棄、缺席、重抽；每次異動保留 Audit Log。
- Display 動畫只負責揭曉，不得自行決定 winner。
- 可匯出完整中獎與重抽紀錄。

### 4.4 Photo Wall 活動照片牆

- Participant 從手機拍照或選擇照片上傳。
- Server 驗證檔案大小、格式、實際 content、尺寸及 storage path。
- 自動產生 thumbnail；原圖、縮圖及合成圖分開管理。
- Host 審核、隱藏、刪除、置頂及批次下載。
- Display 以保持比例的網格或輪播呈現，避免拉伸並可安全處理直／橫式照片。
- Guest 可查看自己的照片、上傳狀態及被拒絕原因。
- 活動結束後可產生 ZIP 或唯讀相簿；公開下載必須有明確期限與權限。

### 4.5 Mobile Booth 手機拍貼

- 手機相機拍攝或選圖，支援裁切、旋轉與選擇活動邊框。
- 邊框使用透明 PNG template，定義輸出尺寸、安全區與照片 placement。
- 由 Server 產生最終合成圖，確保預覽、下載與列印結果一致。
- Guest 可下載電子檔，並選擇是否提交列印。
- 防止同一 Participant 重複大量送印，可設定每人／每組配額。

商用同類服務常見的基本流程是「掃碼 → 上傳或拍攝 → 套用邊框 → 確認列印／下載」，並搭配相簿及現場投影；這也是 EventHub 適合採用的最小互動流程。[拍拍印手機拍印服務](https://linein.cc/zh/wedding/services/mobilebooth)、[拍拍雲](https://linein.cc/zh/event/service/cloud-photo)

### 4.6 Print Queue 即時列印

列印是硬體流程，不應直接由 Guest 控制 printer：

- Guest 只建立 Print Request；Server 驗證資格、配額及檔案狀態後建立 Print Job。
- Print Job 狀態至少包含 `Queued`、`Printing`、`Completed`、`Failed`、`Cancelled`。
- 專用 Print Worker 依序處理，避免多台手機同時呼叫 Windows printer。
- Host 可查看 Preview、份數、紙張尺寸、printer、等待時間及錯誤原因。
- 支援重試、取消、重新列印與人工標記完成，且不得因 retry 重複扣除配額。
- Printer offline、缺紙、卡紙或 spooler error 不可讓 Server 或其他活動功能停止。
- 原始照片與輸出檔保持比例，並明確定義裁切、出血、DPI、色彩與邊框規格。
- 活動前需有 test print、校色、耗材與備援 printer 檢查。

### 4.7 可選互動活動

完成核心模組後，可依實際活動需求選擇：

- 猜照片／看圖猜人：使用已審核照片作為題目來源。
- 人氣照片票選：每人有限票數，與一般 Poll 共用投票規則。
- Word Cloud：文字輸入需有長度限制、敏感詞過濾與 Host moderation。
- Bingo／任務卡：Server 保存任務完成狀態，適合家庭日或闖關。
- 團隊競賽：依桌次或部門累積 Quiz、任務與投票分數。
- 簡單轉盤動畫：只能作為已由 Server 決定結果的揭曉視覺，不建立第二套抽獎邏輯。

## 5. 商用品質的非功能需求

### 5.1 可靠性

- 核心功能在 Internet 中斷時仍可使用。
- Server restart 後可恢復活動、目前流程、答案、分數、抽獎、照片與列印工作。
- 重複 request、SignalR reconnect、Browser refresh 及 button double click 不造成重複結果。
- 所有 I/O 有 timeout、cancellation、可理解的錯誤訊息與必要 logging。

### 5.2 安全與隱私

- Host／Participant Credential 不以明文存入 log 或匯出檔。
- Upload、ID、state、deadline、permission 與資料歸屬全部由 Server 驗證。
- 圖片不依賴 Client file name 決定實際路徑，並防止 path traversal 與偽造 content type。
- Guest 僅能查看被授權的活動與照片；活動結束後可關閉加入、上傳及下載。
- 在加入頁清楚告知蒐集欄位、照片用途、保存期間與刪除方式。

### 5.3 易用性與可及性

- Guest 主要流程控制在三個步驟內，不要求安裝、註冊或輸入長代碼。
- Host 每個畫面都有目前狀態、下一步、成功／失敗結果與恢復提示。
- 手機支援直式小螢幕、常見瀏覽器、觸控尺寸及足夠色彩對比。
- Display 上的關鍵資訊可在活動場地後排閱讀，不以顏色作為唯一狀態提示。

### 5.4 效能基準

第一階段先以單一一般公司活動為目標，並用實測數字取代模糊的「很多人」：

- 200 位已加入 Participant。
- 100 位同時在線。
- Quiz／Poll 開始後 10 秒內收到 100 筆提交，不遺失且不重複。
- 50 個同時照片 upload request 時，Server 仍可操作 Quiz 與 Host。
- 1,000 張活動照片可正常產生縮圖、瀏覽與批次匯出。
- Print Queue 即使 printer failure，其他模組仍維持可用。

上述是初始驗收目標，不代表已經通過；每項都需要建立可重複的測試方法。

## 6. 建議開發順序

| 階段 | 目標 | 主要內容 | 完成判定 |
| --- | --- | --- | --- |
| P0 | 穩定既有核心 | 活動生命週期、Host 恢復、備份／還原、Audit Log、健康狀態、演練模式、完整匯出 | 現有 Quiz 在斷線與 restart 情境可安全完成一場活動 |
| P1 | 完成一般活動互動 | Poll、Lucky Draw、圖片題、活動流程排序 | 尾牙可完成報到、Quiz、投票、抽獎及結果匯出 |
| P2 | 完成照片體驗 | Photo Upload、moderation、thumbnail、Photo Wall、批次下載 | 照片可安全上傳、審核、投影與封存 |
| P3 | 完成拍貼與列印 | Template composer、Mobile Booth、Print Queue、Printer Worker | 可預覽、限額、排隊、失敗重試並輸出一致成品 |
| P4 | 進階玩法 | 照片票選、猜照片、Word Cloud、Bingo、團隊競賽 | 依實際活動需求逐項驗收，不影響核心穩定性 |

不建議一開始就做 AI、360 影片或複雜動畫。對現場活動而言，可恢復性、列印佇列、操作防呆與資料備份，比功能數量更接近真正的商用品質。

## 7. EventHub 現況對照

| 能力 | 現況 | 下一步 |
| --- | --- | --- |
| Event／Join／Participant | 已有基礎生命週期、QR Join、Credential 與 Presence | 補名單匯入、Host 協助恢復、活動清單與匯出 |
| Quiz／Ranking | 已有 Practice／Scored、Server deadline、計分與排行榜 | 補圖片題、計分設定、正式開始後鎖定及報表 |
| Host／Display reconnect | 已有 Current State recovery | 補可視化 health、Audit Log、預覽及演練模式 |
| Poll | 未實作 | P1 建立獨立 state 與投票唯一約束 |
| Lucky Draw | 未實作 | P1 先完成 prize、eligibility、winner、redraw 與 audit |
| Photo Wall | 未實作 | P2 先完成安全 upload、moderation、thumbnail 與 storage |
| Mobile Booth／Print Queue | 未實作 | P3 在 Photo pipeline 穩定後加入，避免 UI 直接控制 printer |
| AI／360／商業營運功能 | 不在目前範圍 | 有明確自用活動需求後再評估 |

## 8. 第一個「可放心帶到現場」的版本

在增加大量新遊戲前，建議第一個里程碑必須同時符合：

- 可建立、準備、開放加入、開始及完成一場活動。
- Host 可從本機安全恢復活動，不依賴手抄 Token。
- Quiz 完整流程、重複提交、deadline、計分與排行榜有自動測試。
- Server restart、Host／Display reconnect、手機 refresh 均可恢復。
- 可執行備份、還原及結果匯出，並能在另一個測試目錄驗證還原結果。
- Host 可看見 Server、Display、Storage 與 SignalR 狀態。
- 所有重要命令有 Audit Log，錯誤不會只顯示「操作失敗」。
- 使用實際活動 LAN，以至少 20 支手機完成一次完整演練。

完成這個里程碑後，再將 Poll、Lucky Draw、Photo Wall 與 Print Queue 逐一加入，會比同時開發所有活動更容易維持 Simple、Clear、Testable、Maintainable。
