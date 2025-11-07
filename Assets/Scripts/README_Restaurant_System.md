# 餐廳資料請求系統使用說明

## 系統概述

本系統整合了 GPS 定位和餐廳資料請求功能，當用戶點擊 GPS 按鈕獲取位置後，系統會自動從 API 獲取附近的餐廳資料。

## 核心組件

### 1. RequestModel.cs - 資料模型

#### RequestPlacesModel
包含餐廳清單的主要資料結構
```csharp
public class RequestPlacesModel
{
    public List<PlaceModel> places;  // 餐廳清單
}
```

#### PlaceModel
單一餐廳的詳細資訊
```csharp
public class PlaceModel
{
    public string displayName;        // 餐廳名稱
    public float? rating;             // 評分（可能為 null）
    public string googleMapsUri;      // Google Maps 連結
    public bool? openNow;             // 是否營業中（可能為 null）
    public string startPrice;         // 起始價格
    public string endPrice;           // 結束價格
    public string currencyCode;       // 貨幣代碼
}
```

#### PlaceModel 實用方法
```csharp
place.GetStartPriceAsFloat()    // 取得起始價格（數字）
place.GetEndPriceAsFloat()      // 取得結束價格（數字）
place.GetPriceRangeText()       // 取得價格範圍文字
place.IsOpen()                  // 檢查是否營業中
place.GetRatingText()           // 取得評分文字
```

### 2. RequestManager.cs - 請求管理器

負責向 API 請求餐廳資料並解析回應。

#### 主要方法

**獲取餐廳資料**
```csharp
RequestManager.instance.getRestaurant(latitude, longitude, radius);
```
- `latitude`: 緯度
- `longitude`: 經度
- `radius`: 搜尋半徑（公尺），預設 1000

**取得當前餐廳清單**
```csharp
List<PlaceModel> places = RequestManager.instance.GetCurrentPlaces();
```

**取得特定餐廳**
```csharp
PlaceModel place = RequestManager.instance.GetPlace(index);
```

**取得餐廳數量**
```csharp
int count = RequestManager.instance.GetPlacesCount();
```

**清除資料**
```csharp
RequestManager.instance.ClearPlacesData();
```

### 3. RestaurantEvents.cs - 餐廳事件

定義餐廳相關的事件常數：
- `RestaurantEvents.DATA_RECEIVED`: 餐廳資料接收成功
- `RestaurantEvents.DATA_FAILED`: 餐廳資料接收失敗

## 系統流程

```
用戶點擊 GPS 按鈕
↓
UIManager 開始 GPS 定位
↓
GPS 定位成功 → 發布 GPSEvents.LOCATION_SUCCESS
↓
UIManager 接收 GPS 事件
↓
UIManager 自動調用 RequestManager.getRestaurant()
↓
RequestManager 向 API 請求餐廳資料
↓
成功接收並解析 JSON → 發布 RestaurantEvents.DATA_RECEIVED
↓
UIManager 或其他組件接收餐廳資料並顯示
```

## 使用範例

### 基本使用（在 UIManager 中）

```csharp
void Start()
{
    // 訂閱 GPS 成功事件
    EventBus.Instance.Subscribe(GPSEvents.LOCATION_SUCCESS, OnGPSLocationSuccess);
    
    // 訂閱餐廳資料接收事件
    EventBus.Instance.Subscribe(RestaurantEvents.DATA_RECEIVED, OnRestaurantDataReceived);
}

void OnGPSLocationSuccess(object data)
{
    GPSLocationData locationData = (GPSLocationData)data;
    
    // 自動使用 GPS 位置獲取餐廳
    RequestManager.instance.getRestaurant(
        locationData.latitude, 
        locationData.longitude
    );
}

void OnRestaurantDataReceived(object data)
{
    RequestPlacesModel placesData = (RequestPlacesModel)data;
    
    Debug.Log($"找到 {placesData.places.Count} 間餐廳");
    
    // 顯示餐廳資訊
    foreach (PlaceModel place in placesData.places)
    {
        Debug.Log($"{place.displayName} - {place.GetRatingText()}");
    }
}
```

### 自訂餐廳顯示組件

```csharp
public class RestaurantList : MonoBehaviour
{
    void Start()
    {
        // 訂閱餐廳資料事件
        EventBus.Instance.Subscribe(RestaurantEvents.DATA_RECEIVED, OnRestaurantDataReceived);
    }
    
    void OnRestaurantDataReceived(object data)
    {
        RequestPlacesModel placesData = (RequestPlacesModel)data;
        DisplayRestaurants(placesData.places);
    }
    
    void DisplayRestaurants(List<PlaceModel> places)
    {
        // 清除舊列表
        ClearList();
        
        // 顯示每間餐廳
        foreach (PlaceModel place in places)
        {
            // 創建餐廳 UI 項目
            GameObject item = Instantiate(restaurantItemPrefab, listContainer);
            
            // 設定餐廳資訊
            RestaurantItem itemScript = item.GetComponent<RestaurantItem>();
            itemScript.SetData(
                place.displayName,
                place.GetRatingText(),
                place.GetPriceRangeText(),
                place.IsOpen()
            );
        }
    }
}
```

### 手動獲取餐廳資料

```csharp
// 使用固定座標
RequestManager.instance.getRestaurant(22.5666278f, 120.3068823f, 1000);

// 使用 GPS 位置
if (GPSManager.instance != null)
{
    float lat = GPSManager.instance.latitude;
    float lon = GPSManager.instance.longitude;
    RequestManager.instance.getRestaurant(lat, lon, 2000);
}
```

### 訪問已儲存的餐廳資料

```csharp
// 取得所有餐廳
List<PlaceModel> allPlaces = RequestManager.instance.GetCurrentPlaces();

// 取得第一間餐廳
PlaceModel firstPlace = RequestManager.instance.GetPlace(0);

// 檢查餐廳數量
int count = RequestManager.instance.GetPlacesCount();
if (count > 0)
{
    Debug.Log($"目前有 {count} 間餐廳資料");
}
```

## API 端點

```
GET https://twswapi.cloudns.nz:3002/api/getnearby/
參數:
  - latitude: 緯度
  - longitude: 經度
  - radius: 搜尋半徑（公尺）
```

## JSON 回應格式

```json
{
    "places": [
        {
            "displayName": "餐廳名稱",
            "rating": 4.2,
            "googleMapsUri": "https://maps.google.com/...",
            "openNow": true,
            "startPrice": "400",
            "endPrice": "600",
            "currencyCode": "TWD"
        }
    ]
}
```

## 注意事項

1. **Nullable 值處理**：`rating`、`openNow`、`startPrice`、`endPrice` 可能為 null
2. **價格格式**：價格以字串格式儲存，使用 `GetStartPriceAsFloat()` 轉換為數字
3. **事件訂閱**：記得在 `OnDestroy()` 中取消訂閱事件
4. **單例模式**：RequestManager 使用單例模式，通過 `RequestManager.instance` 訪問

## 疑難排解

### 問題：收不到餐廳資料
- 檢查網路連線
- 確認 API 端點是否正確
- 查看 Console 的錯誤訊息

### 問題：JSON 解析失敗
- 確認 API 回應格式符合 RequestPlacesModel
- 檢查是否有特殊字元導致解析錯誤

### 問題：事件沒有觸發
- 確認已訂閱對應的事件
- 檢查 EventBus 是否正確初始化
- 查看 Console 的事件發布日誌

## 擴展功能建議

1. **加入快取機制**：避免重複請求相同位置的資料
2. **加入搜尋篩選**：依評分、價格、營業狀態篩選
3. **加入排序功能**：依距離、評分、價格排序
4. **加入收藏功能**：儲存喜愛的餐廳
5. **加入詳細頁面**：顯示餐廳完整資訊和地圖
