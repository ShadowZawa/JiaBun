using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;


public class RequestManager : MonoBehaviour
{
    private const string ConversationApiUrl = "https://twswapi.cloudns.nz:2096/api/conversation";

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
        if (!TryEnterThinkingState()) return;

        try
        {
            MessageHistoryData data = FileManager.Instance.LoadChatHistory();
            if (data == null)
            {
                ExitThinkingState();
                EventBus.Instance.Publish(new showMessageBoxEvent("讀取聊天紀錄失敗，請稍後重試", Color.red, 2));
                return;
            }

            List<MessageModel> messages = data.messages ?? new List<MessageModel>();
            // 防護：避免當歷史訊息小於 10 時發生 GetRange 的例外
            int start = Mathf.Max(0, messages.Count - 10);
            List<MessageModel> range = messages.GetRange(start, messages.Count - start);
            StartCoroutine(ConversationRequestCoroutine(e.userMessage, data.conversationData, data.affinity, range));
        }
        catch (System.Exception ex)
        {
            ExitThinkingState();
            Debug.LogError($"初始化聊天請求失敗: {ex.Message}");
            EventBus.Instance.Publish(new showMessageBoxEvent("初始化請求失敗，請稍後重試", Color.red, 2));
        }
    }


    public bool allowInvalidCertificates = false; // 設為 true 以在行動裝置上暫時繞過憑證驗證（不安全，僅測試）
    public int requestTimeoutSeconds = 90;

    public IEnumerator ConversationRequestCoroutine(string msg, string summary, int affinity, List<MessageModel> messageHistory, string character = "")
    {
        UnityWebRequest request = null;
        try
        {
            string url = BuildConversationUrl(character);
            Debug.Log($"正在請求聊天對話: {url}");

            // 檢查裝置網路狀態
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogError("無網路連線，請檢查裝置網路設定");
                EventBus.Instance.Publish(new showMessageBoxEvent("無網路連線", Color.red, 2));
                yield break;
            }

            ChatConversationRequestModel requestData = BuildConversationRequest(msg, summary, affinity, messageHistory);
            string jsonData = JsonUtility.ToJson(requestData);
            Debug.Log($"[RequestManager] 發送的 JSON 資料: {jsonData}");

            request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            request.timeout = requestTimeoutSeconds;

            if (allowInvalidCertificates)
            {
                request.certificateHandler = new BypassCertificate();
            }

            yield return request.SendWebRequest();
            Debug.Log($"Request result: {request.result}, responseCode: {request.responseCode}, error: {request.error}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                HandleConversationSuccess(request.downloadHandler.text, msg);
            }
            else
            {
                HandleConversationFailure(request);
            }
        }
        finally
        {
            request?.Dispose();
            ExitThinkingState();
        }
    }

    private bool TryEnterThinkingState()
    {
        if (is_thinking)
        {
            return false;
        }

        is_thinking = true;
        return true;
    }

    private void ExitThinkingState()
    {
        is_thinking = false;
    }

    private string BuildConversationUrl(string character)
    {
        string url = ConversationApiUrl;
        bool hasQuery = false;

        if (!string.IsNullOrEmpty(character))
        {
            url += $"?character={UnityWebRequest.EscapeURL(character)}";
            hasQuery = true;
        }

        if (GPSManager.instance != null && GPSManager.instance.latitude != 0 && GPSManager.instance.longitude != 0)
        {
            url += hasQuery ? "&" : "?";
            url += $"latitude={GPSManager.instance.latitude}&longitude={GPSManager.instance.longitude}";
        }

        return url;
    }

    private ChatConversationRequestModel BuildConversationRequest(string msg, string summary, int affinity, List<MessageModel> messageHistory)
    {
        ChatConversationRequestModel requestData = new ChatConversationRequestModel
        {
            summary = summary ?? string.Empty,
            message_history = new List<ChatMessageModel>(),
            message = msg,
            affinity = affinity
        };

        if (messageHistory == null)
        {
            return requestData;
        }

        foreach (MessageModel msgModel in messageHistory)
        {
            requestData.message_history.Add(ChatMessageModel.FromMessageModel(msgModel));
        }

        return requestData;
    }

    private void HandleConversationSuccess(string responseData, string userMessage)
    {
        Debug.Log($"[RequestManager] 聊天對話回應成功: {responseData}");

        try
        {
            ChatConversationResponseModel response = JsonUtility.FromJson<ChatConversationResponseModel>(responseData);

            if (response == null)
            {
                EventBus.Instance.Publish(new showMessageBoxEvent("伺服器回應格式錯誤", Color.red, 2));
                Debug.LogError("聊天回應解析失敗: response 為 null");
                return;
            }

            Debug.Log($"AI 回覆: {response.reply}");
            Debug.Log($"更新後的印象: {response.summary}");
            Debug.Log($"好感度: {response.affinity}");

            if (!string.IsNullOrEmpty(response.map_url))
            {
                EventBus.Instance.Publish<mapURLUpdateEvent>(new mapURLUpdateEvent(response.map_url));
                Debug.Log($"地圖 URL: {response.map_url}");
            }

            EventBus.Instance.Publish<newAIMessageEvent>(new newAIMessageEvent(response.reply, userMessage, response.summary, response.affinity));
        }
        catch (System.Exception ex)
        {
            EventBus.Instance.Publish(new showMessageBoxEvent("與伺服器連接失敗 請重試", Color.red, 2));
            Debug.LogError($"聊天回應解析失敗: {ex.Message}");
        }
    }

    private void HandleConversationFailure(UnityWebRequest request)
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
