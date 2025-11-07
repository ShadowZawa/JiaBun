using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class RequestManager : MonoBehaviour
{
    private static RequestManager _instance;
    public static RequestManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("RequestManager");
                _instance = go.AddComponent<RequestManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // 儲存當前的餐廳資料
    private RequestPlacesModel currentPlacesData;
    
    /// <summary>
    /// 取得當前的餐廳清單
    /// </summary>
    public List<PlaceModel> GetCurrentPlaces()
    {
        return currentPlacesData?.places;
    }

    /// <summary>
    /// 取得特定索引的餐廳資料
    /// </summary>
    public PlaceModel GetPlace(int index)
    {
        if (currentPlacesData?.places != null && index >= 0 && index < currentPlacesData.places.Count)
        {
            return currentPlacesData.places[index];
        }
        return null;
    }

    /// <summary>
    /// 取得餐廳數量
    /// </summary>
    public int GetPlacesCount()
    {
        return currentPlacesData?.places?.Count ?? 0;
    }

    public void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Start()
    {
        // 測試用：啟動時自動取得餐廳資料
        // getRestaurant(22.5666278f, 120.3068823f);
    }

    /// <summary>
    /// 根據經緯度取得附近的餐廳
    /// </summary>
    /// <param name="latitude">緯度</param>
    /// <param name="longitude">經度</param>
    /// <param name="radius">搜尋半徑（公尺），預設 1000</param>
    public void getRestaurant(float latitude, float longitude, int radius = 1000)
    {
        StartCoroutine(GetRestaurantCoroutine(latitude, longitude, radius));
    }
    
    private IEnumerator GetRestaurantCoroutine(float latitude, float longitude, int radius)
    {
        string url = $"https://twswapi.cloudns.nz:3002/api/getnearby/?latitude={latitude}&longitude={longitude}&radius={radius}";
        Debug.Log($"[RequestManager] 正在請求餐廳資料: {url}");

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonData = request.downloadHandler.text;
            Debug.Log($"[RequestManager] 成功接收到餐廳資料: {jsonData}");
            
            // 解析 JSON 資料
            try
            {
                currentPlacesData = JsonUtility.FromJson<RequestPlacesModel>(jsonData);
                
                if (currentPlacesData?.places != null)
                {
                    Debug.Log($"[RequestManager] 成功解析 {currentPlacesData.places.Count} 間餐廳");
                    
                    // 輸出餐廳資訊（除錯用）
                    LogPlacesInfo();
                    
                    // 發布事件通知其他組件
                    EventBus.Instance.Publish<RestaurantDataReceivedEvent>(new RestaurantDataReceivedEvent(currentPlacesData.places));
                }
                else
                {
                    Debug.LogWarning("[RequestManager] 餐廳清單為空");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RequestManager] JSON 解析失敗: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"[RequestManager] 請求餐廳資料失敗: {request.error}");
        }
        
        request.Dispose();
    }

    /// <summary>
    /// 輸出餐廳資訊到 Console（除錯用）
    /// </summary>
    private void LogPlacesInfo()
    {
        if (currentPlacesData?.places == null) return;

        Debug.Log("========== 餐廳清單 ==========");
        for (int i = 0; i < currentPlacesData.places.Count; i++)
        {
            PlaceModel place = currentPlacesData.places[i];
            Debug.Log($"[{i}] {place.displayName}");
            Debug.Log($"    評分: {place.GetRatingText()}");
            Debug.Log($"    營業狀態: {(place.IsOpen() ? "營業中" : "未營業或未知")}");
            Debug.Log($"    價格範圍: {place.GetPriceRangeText()}");
            Debug.Log($"    Google Maps: {place.googleMapsUri}");
        }
        Debug.Log("==============================");
    }

    /// <summary>
    /// 清除當前的餐廳資料
    /// </summary>
    public void ClearPlacesData()
    {
        currentPlacesData = null;
        Debug.Log("[RequestManager] 餐廳資料已清除");
    }
}
