# 通知系統設定指南

## 功能說明
NotificationManager 會在每天的固定時間推送通知：
- **上午 11:00** - 午餐提醒
- **下午 17:00** - 晚餐提醒

## 已完成的設定

### 1. NotificationManager.cs
- ✅ 實現每日定時通知功能
- ✅ 支援 Android 和 iOS 平台
- ✅ 自動重複排程（24小時循環）
- ✅ Singleton 模式，DontDestroyOnLoad

### 2. AppInitializer.cs
- ✅ 自動在應用啟動時初始化 NotificationManager
- ✅ 使用 RuntimeInitializeOnLoadMethod 確保早期載入

### 3. Package Dependencies
- ✅ Unity Mobile Notifications (v2.4.2) 已安裝

## 需要手動完成的設定

### Android 設定

#### 1. 建立通知圖示（可選）

**可以不設定圖示嗎？**
✅ **可以！** 如果不放 `icon_small.png`，Android 會自動使用應用程式的啟動圖示（Application Icon）。

**如果要自訂通知圖示**：

**步驟 1：準備圖示檔案**

使用線上工具快速生成（推薦）：
1. 前往 [Android Asset Studio](https://romannurik.github.io/AndroidAssetStudio/icons-notification.html)
2. 上傳你的 Logo 或選擇內建圖示
3. 調整為白色前景 + 透明背景
4. 下載生成的所有尺寸 ZIP 檔

**步驟 2：建立資料夾**

**最簡單方法：只放一個圖示**
```
Assets/Plugins/Android/res/drawable/icon_small.png
```
建議 96x96 或 192x192 像素，Android 會自動縮放。

**完整方法：多尺寸（可選）**
```
Assets/Plugins/Android/res/
├── drawable-mdpi/icon_small.png (24x24)
├── drawable-hdpi/icon_small.png (36x36)
├── drawable-xhdpi/icon_small.png (48x48)
├── drawable-xxhdpi/icon_small.png (72x72)
└── drawable-xxxhdpi/icon_small.png (96x96)
```

**圖示設計要求**：
- ✅ 純白色（#FFFFFF）圖示
- ✅ 背景完全透明（Alpha = 0）
- ✅ 簡單的線條或輪廓圖（避免過於複雜）
- ✅ PNG 格式
- ✅ 檔案名稱必須是 `icon_small.png`（小寫，無特殊字元）
- ❌ 不可使用彩色圖示（Android 8.0+ 會自動變成單色）

**步驟 4：手動建立圖示（如果不使用線上工具）**

使用 Photoshop/GIMP/任何繪圖軟體：
1. 建立新檔案（建議 96x96 像素）
2. 背景設為透明
3. 使用白色（#FFFFFF）繪製簡單圖示
4. 匯出為 PNG 格式，命名為 `icon_small.png`

**範例圖示建議**：
- 🍽️ 餐盤或餐具圖示（符合「佳奔」用餐主題）
- 🔔 鈴鐺圖示（通用提醒）
- 📍 定位圖示（地點相關）
- 🥘 碗或湯匙圖示

**驗證圖示是否正確**：
1. 在 Unity Project 視窗確認檔案存在
2. Build 到 Android 裝置
3. 收到通知時檢查圖示是否正確顯示（應為白色剪影）

**常見問題**：
**常見問題**：
- 如果不放圖示，會使用應用程式的主圖示（可能顯示為彩色方塊）
- 自訂圖示檔名必須是 `icon_small.png`（完全小寫）
- 自訂圖示必須是白色 + 透明背景
- 如果 Unity 無法識別，嘗試重新匯入資料夾（右鍵 > Reimport）
#### 2. 權限設定（已自動處理）
Mobile Notifications 套件會自動添加以下權限到 AndroidManifest.xml：
```xml
<uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
<uses-permission android:name="android.permission.RECEIVE_BOOT_COMPLETED" />
```

### iOS 設定

#### 1. 在 Unity Build Settings
- Player Settings > iOS > Other Settings
- 確認 Target minimum iOS Version >= 10.0

#### 2. 在 Xcode (Build 後)
不需要額外設定，通知權限會在首次啟動時自動請求。

### 測試通知

#### 在 Unity Editor 測試（僅 Android）
```csharp
// 在任何腳本中調用
NotificationManager.Instance.ScheduleDailyNotifications();
```

#### 在真機測試
1. Build 到 Android/iOS 裝置
2. 啟動應用程式
3. 關閉應用程式
4. 等待到設定的時間（11:00 或 17:00）查看通知

#### 快速測試（修改時間）
在 `NotificationManager.cs` 的 `ScheduleDailyNotifications()` 中暫時修改：
```csharp
// 測試用：1分鐘後觸發
ScheduleNotificationAt(DateTime.Now.Hour, DateTime.Now.Minute + 1, "測試標題", "測試內容");
```

## API 使用方法

### 取消所有通知
```csharp
NotificationManager.Instance.CancelAllNotifications();
```

### 重新排程通知
```csharp
NotificationManager.Instance.RescheduleNotifications();
```

### 修改通知時間
編輯 `NotificationManager.cs` 中的 `ScheduleDailyNotifications()` 方法：
```csharp
ScheduleNotificationAt(hour, minute, title, message);
```

## 常見問題

### Q: 為什麼收不到通知？
A: 
1. 確認裝置通知權限已開啟
2. 確認應用程式已關閉或在背景執行
3. 檢查系統時間設定是否正確
4. Android: 確認通知頻道未被靜音

### Q: 如何自訂通知內容？
A: 編輯 `ScheduleDailyNotifications()` 中的 title 和 message 參數

### Q: 可以新增更多通知時間嗎？
A: 可以，在 `ScheduleDailyNotifications()` 中添加更多 `ScheduleNotificationAt()` 調用

### Q: iOS 通知不顯示？
A: 
1. 確認已授予通知權限
2. iOS 預設前景時不顯示通知，請關閉應用測試
3. 檢查 iPhone 設定 > 通知 中是否允許應用通知

## 進階設定

### 自訂通知聲音（Android）
將音訊檔案放到 `Assets/Plugins/Android/res/raw/`，然後在通知中設定：
```csharp
notification.Sound = "custom_sound"; // 不含副檔名
```

### 自訂通知分類（iOS）
```csharp
var category = new iOSNotificationCategory(
    "custom_category",
    new iOSNotificationAction[] { /* actions */ }
);
iOSNotificationCenter.SetNotificationCategories(new iOSNotificationCategory[] { category });
```
## 檔案清單
- ✅ `Assets/Scripts/Manager/NotificationManager.cs`
- ✅ `Assets/Scripts/Manager/AppInitializer.cs`
- ⚠️ `Assets/Plugins/Android/res/drawable/icon_small.png` (可選，不放則使用應用程式圖示)
- ⚠️ `Assets/Plugins/Android/res/drawable-*/icon_small.png` (需手動建立)
