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
    void Start()
    {
        EventBus.Instance.Subscribe<newUserMessageEvent>(SendChatConversation);
    }

    void OnDestroy()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<newUserMessageEvent>(SendChatConversation);
        }
    }
    /// <summary>
    /// 發送聊天對話請求
    /// </summary>
    /// <param name="message">使用者訊息</param>
    /// <param name="summary">對話摘要</param>
    /// <param name="messageHistory">訊息歷史</param>
    public bool is_thinking = false;
    public void SendChatConversation(newUserMessageEvent e)
    {
        if (is_thinking) return;

        is_thinking = true;
        MessageHistoryData data  = FileManager.Instance.LoadChatHistory();
        // 防護：避免當歷史訊息小於 10 時發生 GetRange 的例外
        int start = Mathf.Max(0, data.messages.Count - 10);
        List<MessageModel> range = data.messages.GetRange(start, data.messages.Count - start);
        StartCoroutine(ConversationRequestCoroutine(e.userMessage, data.conversationData, data.affinity, range));
    }


    public bool allowInvalidCertificates = false; // 設為 true 以在行動裝置上暫時繞過憑證驗證（不安全，僅測試）
    public int requestTimeoutSeconds = 30;

    public IEnumerator ConversationRequestCoroutine(string msg, string summary, int affinity, List<MessageModel> messageHistory, string character = "")
    {
        string url = "https://twswapi.cloudns.nz:2096/api/conversation";
        if (character != "")
        {
            url += $"?character={UnityWebRequest.EscapeURL(character)}";
        }
        if (GPSManager.instance.latitude != 0 && GPSManager.instance.longitude != 0)
        {
            url += (character == "" ? "?" : "&") + $"latitude={GPSManager.instance.latitude}&longitude={GPSManager.instance.longitude}";
        }
        Debug.Log($"正在請求聊天對話: {url}");

        // 檢查裝置網路狀態
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            is_thinking = false;
            Debug.LogError("無網路連線，請檢查裝置網路設定");
            EventBus.Instance.Publish(new showMessageBoxEvent("無網路連線", Color.red, 2));
            yield break;
        }
        // 建立對話請求資料
        ChatConversationRequestModel requestData = new ChatConversationRequestModel
        {
            summary = summary ?? "",
            message_history = new List<ChatMessageModel>(),
            message = msg,
            affinity = affinity 
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
        request.timeout = requestTimeoutSeconds;
        yield return request.SendWebRequest();
        is_thinking = false;
        Debug.Log($"Request result: {request.result}, responseCode: {request.responseCode}, error: {request.error}");
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseData = request.downloadHandler.text;
            Debug.Log($"[RequestManager] 聊天對話回應成功: {responseData}");

            try
            {
                ChatConversationResponseModel response = JsonUtility.FromJson<ChatConversationResponseModel>(responseData);
                
                if (response != null)
                {
                    Debug.Log($"AI 回覆: {response.reply}");
                    Debug.Log($"更新後的印象: {response.summary}");
                    Debug.Log($"好感度: {response.affinity}");
                    
                    // 發布聊天回應事件
                    if (response.map_url != "")
                    {
                        EventBus.Instance.Publish<mapURLUpdateEvent>(new mapURLUpdateEvent(response.map_url));
                        Debug.Log($"地圖 URL: {response.map_url}");
                    }
                    EventBus.Instance.Publish<newAIMessageEvent>(new newAIMessageEvent(response.reply, msg, response.summary, response.affinity));
                }
            }
            catch (System.Exception ex)
            {
                //MessageBox.Show($"與伺服器連接失敗 請重試");
                EventBus.Instance.Publish(new showMessageBoxEvent("與伺服器連接失敗 請重試", Color.red, 2));
                Debug.LogError($"聊天回應解析失敗: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"聊天對話請求失敗: {request.error}");
            Debug.LogError($"回應碼: {request.responseCode}");

            // 若 responseCode == 0 且有錯誤訊息，可能為 TLS/SSL 或網路層級錯誤
            if (request.responseCode == 0)
            {
                EventBus.Instance.Publish(new showMessageBoxEvent("網路連線失敗或憑證驗證失敗（請確認伺服器憑證）", Color.red, 3));
            }
            else
            {
                EventBus.Instance.Publish(new showMessageBoxEvent("伺服器忙碌中 請重試", Color.red, 2));
            }
        }

        request.Dispose();
    }

    // 測試用：強制接受所有憑證（不安全）
    private class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            // 繞過並接受所有憑證（僅限開發/測試）
            return true;
        }
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

    
}
