# JiaBun Scripts 摘要

更新日期: 2026-05-25

## 判斷方式

- Used: 有場景/Prefab 掛載，或有明確程式碼引用，或由 RuntimeInitializeOnLoadMethod 自動執行。
- Possibly unused: 目前看不到掛載與引用，但仍可能由反射、外部插件、或尚未啟用流程使用。
- Unused candidate: 現階段找不到任何使用痕跡，建議列入清理候選。

## 各腳本用途與使用狀態

| Script | 主要作用 | 使用狀態 | 依據 |
|---|---|---|---|
| EventBus.cs | 事件總線，提供 Publish/Subscribe | Used | 多個 Manager/View 依賴，且有場景掛載 |
| Events/ChatEvents.cs | 聊天事件資料型別 (newAI/newUser/newRestaurantRequest) | Used | 被 ChatManager/RequestManager/View 類型引用 |
| Events/Events.cs | GPS、MessageBox、餐廳資料等事件型別 | Used | 被 RequestManager/GPSManager/BuildSceneView 等引用 |
| Manager/AppInitializer.cs | App 啟動時初始化通知系統 | Used | 使用 RuntimeInitializeOnLoadMethod 自動執行 |
| Manager/ChatManager.cs | 舊聊天場景訊息流程與顯示 | Used | 有場景掛載，且訂閱聊天事件 |
| Manager/FileManager.cs | 聊天歷史保存與讀取 | Used | 有場景掛載，多處呼叫 LoadChatHistory |
| Manager/FirebaseManager.cs | Firebase 註冊/登入流程 | Unused candidate | 未發現掛載，且全專案無其他引用 |
| Manager/GPSManager.cs | 取得定位並發布 GPS 事件 | Used | 有場景掛載，BuildSceneView 訂閱其事件 |
| Manager/NotificationManager.cs | 行動裝置推播初始化 | Used | 有場景掛載，且由 AppInitializer 觸發 |
| Manager/RequestManager.cs | 呼叫後端 API，處理餐廳/對話請求 | Used | 有場景掛載，核心流程多處依賴 |
| Manager/SuitManager.cs | 服裝/造型場景管理 | Used | 於 suitScene 掛載使用 |
| Model/MessageModel.cs | 訊息資料模型與歷史包裝 | Used | 多個 View/Manager 序列化與流程引用 |
| Model/RequestModel.cs | 請求/回應資料模型 (餐廳與聊天) | Used | RequestManager JSON 解析與事件載體使用 |
| View/AffinityView.cs | 顯示親密度/關係值 UI | Used | 有場景掛載，並讀取聊天歷史 |
| View/BubbleViewFix.cs | 聊天泡泡高度/版面修正 | Used | 有場景與 Prefab 掛載，NewChatView 會使用 |
| View/BuildSceneView.cs | 主場景 UI/互動流程整合控制 | Used | 有場景掛載，訂閱多種核心事件 |
| View/ChatSceneView.cs | 聊天場景返回與簡單控制 | Used | 有場景掛載 |
| View/InputFieldFix.cs | 輸入欄位行動裝置行為修正 | Used | MainScene 有掛載（至少兩處） |
| View/MessageBox.cs | 訊息彈窗佇列顯示 | Used | 有場景掛載，訂閱 showMessageBoxEvent |
| View/NewChatView.cs | 新聊天介面渲染與訊息顯示 | Used | newChatScene 掛載，依賴 FileManager/MessageModel |

## 目前可清理候選

1. Manager/FirebaseManager.cs
	- 目前僅在自身檔案中出現類名，未見任何其他引用或掛載。
	- 建議先確認是否有未上線的登入流程；若沒有，可考慮移除或移到實驗資料夾。

## 風險提醒

- Unity 專案可能存在動態掛載/反射呼叫；本次結論基於目前場景、Prefab 與代碼靜態分析。
- 若要做最終刪除，建議先在分支上移除候選腳本並完整跑一次主要流程與打包測試。
