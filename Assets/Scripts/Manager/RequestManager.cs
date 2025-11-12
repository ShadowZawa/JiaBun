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

    /// <summary>
    /// 發送聊天對話請求
    /// </summary>
    /// <param name="message">使用者訊息</param>
    /// <param name="summary">對話摘要</param>
    /// <param name="messageHistory">訊息歷史</param>
    public bool is_thinking = false;
    public void SendChatConversation(string message, string summary, List<MessageModel> messageHistory, string character = "")
    {
        if (is_thinking) return;
        StartCoroutine(ConversationRequestCoroutine(message, summary, messageHistory, character));
    }

    public IEnumerator ConversationRequestCoroutine(string msg, string summary, List<MessageModel> messageHistory, string character = "")
    {
        string url = "https://twswapi.cloudns.nz:2096/api/conversation";
        if (character != "")
        {
            url += $"?character={UnityWebRequest.EscapeURL(character)}";
        }
        Debug.Log($"[RequestManager] 正在請求聊天對話: {url}");

        // 建立對話請求資料
        ChatConversationRequestModel requestData = new ChatConversationRequestModel
        {
            summary = summary ?? "",
            message_history = new List<ChatMessageModel>(),
            message = msg
        };

        // 轉換訊息歷史
        if (messageHistory != null)
        {
            foreach (MessageModel msgModel in messageHistory)
            {
                requestData.message_history.Add(ChatMessageModel.FromMessageModel(msgModel));
            }
        }

        // 序列化為 JSON
        string jsonData = JsonUtility.ToJson(requestData);
        Debug.Log($"[RequestManager] 發送的 JSON 資料: {jsonData}");

        // 創建 POST 請求
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
        
        yield return request.SendWebRequest();
        is_thinking = false;
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseData = request.downloadHandler.text;
            Debug.Log($"[RequestManager] 聊天對話回應成功: {responseData}");

            try
            {
                ChatConversationResponseModel response = JsonUtility.FromJson<ChatConversationResponseModel>(responseData);
                
                if (response != null)
                {
                    Debug.Log($"[RequestManager] AI 回覆: {response.reply}");
                    Debug.Log($"[RequestManager] 更新後的摘要: {response.summary}");
                    
                    // 發布聊天回應事件
                    EventBus.Instance.Publish<ChatConversationReceivedEvent>(new ChatConversationReceivedEvent(response));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RequestManager] 聊天回應解析失敗: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"[RequestManager] 聊天對話請求失敗: {request.error}");
            Debug.LogError($"[RequestManager] 回應碼: {request.responseCode}");
        }

        request.Dispose();
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

    /// <summary>
    /// 根據經緯度取得附近的餐廳
    /// </summary>
    /// <param name="latitude">緯度</param>
    /// <param name="longitude">經度</param>
    /// <param name="radius">搜尋半徑（公尺），預設 1000</param>
    public void getRestaurant(float latitude, float longitude, int radius = 1000)
    {
        StartCoroutine(GetRestaurantCoroutine(latitude, longitude, radius));
        /*RequestPlacesModel testModel = new RequestPlacesModel
        {
            places = new List<PlaceModel>{
                new PlaceModel
                {
                    displayName = "測試餐廳 A",
                    rating = 4.5f,
                    googleMapsUri = "https://maps.google.com/?q=Test+Restaurant+A",
                    openNow = true,
                    startPrice = "10",
                    endPrice = "50",
                    currencyCode = "TWD"
                },
                new PlaceModel
                {
                    displayName = "測試餐廳 B",
                    rating = 4.0f,
                    googleMapsUri = "https://maps.google.com/?q=Test+Restaurant+B",
                    openNow = false,
                    startPrice = "20",
                    endPrice = "100",
                    currencyCode = "TWD"
                }
            }
        };
        currentPlacesData = testModel;
        EventBus.Instance.Publish<RestaurantDataReceivedEvent>(new RestaurantDataReceivedEvent(testModel));
        */
    }

    /// <summary>
    /// 向 AI 發送對話請求，取得餐廳推薦
    /// </summary>
    /// <param name="model">餐廳資料模型</param>
    /// <param name="previousData">先前的對話資料（可選）</param>
    public void getRestaurantConversation(RequestPlacesModel model, string previousData = "")
    {
        StartCoroutine(GetRestaurantConversationCoroutine(model, previousData));
    }

    public IEnumerator GetRestaurantConversationCoroutine(RequestPlacesModel model, string previousData = "")
    {
        

        // 建立對話請求資料
        ConversationRequestModel requestData = new ConversationRequestModel
        {
            PreviousData = previousData,
            choice = new List<ConversationChoice>()
        };

        // 轉換餐廳資料為對話選項格式
        if (model?.places != null)
        {
            for (int i = 0; i < model.places.Count; i++)
            {
                ConversationChoice choice = new ConversationChoice
                {
                    ID = $"choice{i + 1}",
                    displayName = model.places[i].displayName
                };
                requestData.choice.Add(choice);
            }
        }

        string jsonData = JsonUtility.ToJson(requestData);
        UnityWebRequest request = new UnityWebRequest("https://twswapi.cloudns.nz:2096/api/conversation/", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseData = request.downloadHandler.text;
            Debug.Log($"[RequestManager] AI 對話回應成功: {responseData}");

            try
            {
                ConversationResponseModel response = JsonUtility.FromJson<ConversationResponseModel>(responseData);
                if (response != null)
                {
                    Debug.Log($"[RequestManager] AI 推薦餐廳 Index: {response.resultIndex}");
                    Debug.Log($"[RequestManager] AI 回應: {response.resultConversation}");
                    Debug.Log($"[RequestManager] 新的對話資料: {response.newData}");
                    EventBus.Instance.Publish<RestaurantConversationRecievedEvent>(new RestaurantConversationRecievedEvent(response));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RequestManager] 對話回應解析失敗: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"[RequestManager] AI 對話請求失敗: {request.error}");
            Debug.LogError($"[RequestManager] 回應碼: {request.responseCode}");
        }

        request.Dispose();
    }

    private IEnumerator GetRestaurantCoroutine(float latitude, float longitude, int radius)
    {
        string url = $"https://twswapi.cloudns.nz:2096/api/getnearby/?latitude={latitude}&longitude={longitude}&radius={radius}";
        Debug.Log($"[RequestManager] 正在請求餐廳資料: {url}");

        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");
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
                    EventBus.Instance.Publish<RestaurantDataReceivedEvent>(new RestaurantDataReceivedEvent(currentPlacesData));
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
