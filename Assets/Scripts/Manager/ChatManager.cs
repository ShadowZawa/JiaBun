using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public GameObject chatItemPrefab;
    public GameObject replyItemPrefab;
    public Transform contentTransform;
    public TMP_InputField messageInputField;
    public Button sendButton;

    [Header("訊息尺寸設定")]
    public float baseItemHeight = 20f; // 基礎訊息高度（padding等）
    public float lineHeight = 42f; // 每行文字的高度
    public float itemSpacing = 10f; // 訊息之間的間距
    public float bottomPadding = 20f; // Content 底部額外邊距
    public float topOffset = 50f; // 第一則訊息的起始 Y 位置
    public float contentHeightScale = 0.74f; // Content 高度縮放比例 (4000/5420 ≈ 0.74)

    [Header("訊息位置設定")]
    public float userMessageXPosition = 160f; // 使用者訊息的 X 位置（靠右）
    public float replyMessageXPosition = -160f; // AI 回覆的 X 位置（靠左）

    [Header("儲存設定")]
    public string saveFileName = "chat_history.json"; // 儲存檔案名稱

    private float totalContentHeight = 0f; // Content 的總高度
    private float currentYPosition = 0f; // 當前 Y 軸位置

    private void Start()
    {
        // 載入之前的聊天記錄
        LoadChatHistory();

        // 訂閱餐廳對話回應事件
        EventBus.Instance.Subscribe<RestaurantConversationRecievedEvent>(OnConversationReceived);
        EventBus.Instance.Subscribe<newAIMessageEvent>(recievedAIMessage);
    } 
    private void recievedAIMessage(newAIMessageEvent e)
    {
        CreateReplyItem(e.aiMessage);
        LoadChatHistory();
    }

    private void OnDestroy()
    {
        // 取消訂閱事件
        EventBus.Instance.Unsubscribe<RestaurantConversationRecievedEvent>(OnConversationReceived);
        EventBus.Instance.Unsubscribe<newAIMessageEvent>(recievedAIMessage);
        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(SendMessage);
        }
    }

    public void SendMessage()
    {
        string message = messageInputField.text;
        if (RequestManager.instance.is_thinking)
        {
            Debug.Log("AI正在思考中zzz");
            return;         
        }
        if (!string.IsNullOrEmpty(message))
        {
            // 從檔案載入最新資料
            MessageHistoryData historyData = FileManager.Instance.LoadChatHistory(saveFileName);
            List<MessageModel> messageHistory = historyData?.messages ?? new List<MessageModel>();
            string conversationData = historyData?.conversationData ?? "";
            
            // 創建使用者訊息並加入歷史記錄
            MessageModel userMessage = new MessageModel
            {
                sender = MessageModel.SenderType.User,
                message = message,
                timestamp = System.DateTime.Now
            };
            messageHistory.Add(userMessage);

            CreateChatItem(message);
            messageInputField.text = "";
            
            // 儲存更新後的歷史記錄
            FileManager.Instance.SaveChatHistory(messageHistory, conversationData, saveFileName);
            
            RequestManager.instance.SendChatConversation(message, conversationData, messageHistory.GetRange(0, Mathf.Min(10, messageHistory.Count)));
        }
    }

    /// <summary>
    /// 創建使用者訊息的聊天項目
    /// </summary>
    private void CreateChatItem(string message)
    {
        if (chatItemPrefab == null || contentTransform == null)
        {
            Debug.LogError("[ChatManager] chatItemPrefab 或 contentTransform 未設定");
            return;
        }

        // 先計算當前的Y位置（在實例化之前）
        int currentIndex = contentTransform.childCount;
        float yPos = CalculateTotalHeightBeforeIndex(currentIndex);

        GameObject chatItem = Instantiate(chatItemPrefab, contentTransform);
        RectTransform itemRect = chatItem.GetComponent<RectTransform>();
        
        // 設定錨點為頂部拉伸（左上到右上）
        if (itemRect != null)
        {
            itemRect.anchorMin = new Vector2(0.5f, 1);
            itemRect.anchorMax = new Vector2(0.5f, 1);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
        }
        
        // 設定訊息文字
        TextMeshProUGUI messageText = chatItem.GetComponentInChildren<TextMeshProUGUI>();
        if (messageText != null)
        {
            messageText.text = message;
            
            // 強制更新文字組件以計算行數
            Canvas.ForceUpdateCanvases();
            messageText.ForceMeshUpdate();
            
            // 計算文字行數
            int lineCount = messageText.textInfo.lineCount;

            // 根據行數調整項目高度
            float itemHeight = CalculateItemHeight(lineCount);
            AdjustItemSize(chatItem, itemHeight);
            
            // 設定位置：使用者訊息靠右 (x = 400)，Y 軸從 0 開始向下
            if (itemRect != null)
            {
                itemRect.anchoredPosition = new Vector2(userMessageXPosition, -yPos);
            }

            Debug.Log($"[ChatManager] 創建使用者訊息: {message} (索引: {currentIndex}, 行數: {lineCount}, 高度: {itemHeight}, Y位置: {yPos})");
        }

        // 更新 Content 高度
        UpdateContentHeight();

        // 自動滾動到底部
        Canvas.ForceUpdateCanvases();
        StartCoroutine(ScrollToBottom());
    }

    /// <summary>
    /// 創建 AI 回覆的項目
    /// </summary>
    private void CreateReplyItem(string reply)
    {
        if (replyItemPrefab == null || contentTransform == null)
        {
            Debug.LogError("[ChatManager] replyItemPrefab 或 contentTransform 未設定");
            return;
        }

        // 檢查是否包含 \n，如果有則分割成多個 ReplyItem
        if (reply.Contains("\n"))
        {
            string[] parts = reply.Split(new string[] { "\n" }, System.StringSplitOptions.None);
            
            foreach (string part in parts)
            {
                // 跳過空字串（但保留只有空白的字串）
                if (part == null) continue;
                
                CreateSingleReplyItem(part);
            }
        }
        else
        {
            CreateSingleReplyItem(reply);
        }

        // 更新 Content 高度
        UpdateContentHeight();

        // 自動滾動到底部
        Canvas.ForceUpdateCanvases();
        StartCoroutine(ScrollToBottom());
    }

    /// <summary>
    /// 創建單一 AI 回覆項目（內部方法）
    /// </summary>
    private void CreateSingleReplyItem(string reply)
    {
        // 先計算當前的Y位置（在實例化之前）
        int currentIndex = contentTransform.childCount;
        float yPos = CalculateTotalHeightBeforeIndex(currentIndex);

        GameObject replyItem = Instantiate(replyItemPrefab, contentTransform);
        RectTransform itemRect = replyItem.GetComponent<RectTransform>();
        
        // 設定錨點為頂部拉伸（左上到右上）
        if (itemRect != null)
        {
            itemRect.anchorMin = new Vector2(0.5f, 1);
            itemRect.anchorMax = new Vector2(0.5f, 1);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
        }

        
        // 設定回覆文字
        TextMeshProUGUI replyText = replyItem.GetComponentInChildren<TextMeshProUGUI>();
        if (replyText != null)
        {
            replyText.text = reply;
            
            // 強制更新文字組件以計算行數
            Canvas.ForceUpdateCanvases();
            replyText.ForceMeshUpdate();
            
            // 計算文字行數
            int lineCount = replyText.textInfo.lineCount;

            // 根據行數調整項目高度
            float itemHeight = CalculateItemHeight(lineCount);
            AdjustItemSize(replyItem, itemHeight);
            
            // 設定位置：AI 回覆靠左 (x = 40)，Y 軸從 0 開始向下
            if (itemRect != null)
            {
                itemRect.anchoredPosition = new Vector2(replyMessageXPosition, -yPos);
            }

            Debug.Log($"[ChatManager] 創建 AI 回覆: {reply} (索引: {currentIndex}, 行數: {lineCount}, 高度: {itemHeight}, Y位置: {yPos})");
        }
    }

    /// <summary>
    /// 根據行數計算項目高度
    /// </summary>
    private float CalculateItemHeight(int lineCount)
    {
        // 基礎高度 + (行數 * 每行高度)
        return baseItemHeight + (lineCount * lineHeight);
    }

    /// <summary>
    /// 計算指定索引之前所有項目的總高度（包含間距）
    /// </summary>
    private float CalculateTotalHeightBeforeIndex(int index)
    {
        if (contentTransform == null || index < 0) return topOffset;
        
        float totalHeight = topOffset; // 從 topOffset 開始
        
        for (int i = 0; i < index; i++)
        {
            if (i < contentTransform.childCount)
            {
                RectTransform childRect = contentTransform.GetChild(i).GetComponent<RectTransform>();
                if (childRect != null)
                {
                    totalHeight += childRect.sizeDelta.y + itemSpacing;
                }
            }
        }
        
        return totalHeight;
    }

    /// <summary>
    /// 調整項目的尺寸
    /// </summary>
    private void AdjustItemSize(GameObject item, float height)
    {
        RectTransform rectTransform = item.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 設定新的高度，保持寬度不變
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y = height;
            rectTransform.sizeDelta = sizeDelta;
        }
    }

    /// <summary>
    /// 更新 Content Transform 的高度
    /// </summary>
    private void UpdateContentHeight()
    {
        if (contentTransform == null) return;

        float totalHeight = topOffset; // 從 topOffset 開始
        int childCount = contentTransform.childCount;

        // 計算所有子物件的總高度
        for (int i = 0; i < childCount; i++)
        {
            RectTransform childRect = contentTransform.GetChild(i).GetComponent<RectTransform>();
            if (childRect != null)
            {
                totalHeight += childRect.sizeDelta.y;
                
                // 加上間距（除了最後一個項目）
                if (i < childCount - 1)
                {
                    totalHeight += itemSpacing;
                }
            }
        }

        // 加上底部邊距
        if (childCount > 0)
        {
            totalHeight += bottomPadding;
        }

        // 更新 Content 的高度 - 套用縮放比例
        RectTransform contentRect = contentTransform.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            Vector2 sizeDelta = contentRect.sizeDelta;
            sizeDelta.y = totalHeight * contentHeightScale;
            contentRect.sizeDelta = sizeDelta;
            
            totalContentHeight = totalHeight * contentHeightScale;
            Debug.Log($"[ChatManager] Content 高度已更新: {totalHeight * contentHeightScale} (原始: {totalHeight}, 子物件數: {childCount})");
        }
    }

    /// <summary>
    /// 當收到 AI 對話回應時的處理
    /// </summary>
    private void OnConversationReceived(RestaurantConversationRecievedEvent eventData)
    {
        if (eventData.conversation != null)
        {
            // 從檔案載入最新資料
            MessageHistoryData historyData = FileManager.Instance.LoadChatHistory(saveFileName);
            List<MessageModel> messageHistory = historyData?.messages ?? new List<MessageModel>();
            
            // 創建 AI 回覆訊息並加入歷史記錄
            MessageModel aiMessage = new MessageModel
            {
                sender = MessageModel.SenderType.AI,
                message = eventData.conversation.resultConversation,
                timestamp = System.DateTime.Now
            };
            messageHistory.Add(aiMessage);

            // 更新對話資料
            string conversationData = eventData.conversation.newData;

            // 顯示 AI 回覆
            CreateReplyItem(eventData.conversation.resultConversation);

            // 儲存聊天記錄
            FileManager.Instance.SaveChatHistory(messageHistory, conversationData, saveFileName);

            // 如果有推薦的餐廳索引，可以進一步處理
            if (!string.IsNullOrEmpty(eventData.conversation.resultIndex))
            {
                Debug.Log($"[ChatManager] AI 推薦餐廳索引: {eventData.conversation.resultIndex}");
            }
        }
        else
        {
            Debug.LogError("[ChatManager] 收到空的對話回應");
            CreateReplyItem("抱歉，無法取得回應，請稍後再試。");
        }
    }

    /// <summary>
    /// 滾動到對話底部
    /// </summary>
    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        
        ScrollRect scrollRect = contentTransform.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    /// <summary>
    /// 清除所有對話記錄
    /// </summary>
    public void ClearChat()
    {
        // 清空檔案中的資料
        FileManager.Instance.SaveChatHistory(new List<MessageModel>(), "", saveFileName);
        
        totalContentHeight = 0f;
        currentYPosition = 0f; // 重設 Y 軸位置
        
        // 清除 UI 中的所有聊天項目
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
        
        // 重設 Content 高度
        UpdateContentHeight();
        
        Debug.Log("[ChatManager] 對話記錄已清除");
    }

    /// <summary>
    /// 儲存聊天記錄到本地
    /// </summary>
    private void SaveChatHistory()
    {
        // 從檔案載入最新資料
        MessageHistoryData historyData = FileManager.Instance.LoadChatHistory(saveFileName);
        List<MessageModel> messageHistory = historyData?.messages ?? new List<MessageModel>();
        string conversationData = historyData?.conversationData ?? "";
        
        // 立即存回（確保資料持久化）
        FileManager.Instance.SaveChatHistory(messageHistory, conversationData, saveFileName);
    }

    /// <summary>
    /// 從本地載入聊天記錄
    /// </summary>
    private void LoadChatHistory()
    {
        MessageHistoryData historyData = FileManager.Instance.LoadChatHistory(saveFileName);
        
        if (historyData != null && historyData.messages != null)
        {
            Debug.Log($"[ChatManager] 載入 {historyData.messages.Count} 則歷史訊息");
            
            // 重建聊天 UI
            RebuildChatUI();
        }
        else
        {
            Debug.Log("[ChatManager] 沒有找到歷史聊天記錄");
        }
    }

    /// <summary>
    /// 根據歷史記錄重建聊天 UI
    /// </summary>
    private void RebuildChatUI()
    {
        // 清除現有的 UI
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
        
        currentYPosition = 0f;
        
        // 從檔案載入最新資料
        MessageHistoryData historyData = FileManager.Instance.LoadChatHistory(saveFileName);
        List<MessageModel> messageHistory = historyData?.messages ?? new List<MessageModel>();
        
        // 重建每則訊息
        foreach (MessageModel msg in messageHistory)
        {
            if (msg.sender == MessageModel.SenderType.User)
            {
                CreateChatItem(msg.message);
            }
            else
            {
                CreateReplyItem(msg.message);
            }
        }
        
        Debug.Log($"[ChatManager] 聊天 UI 已重建 ({messageHistory.Count} 則訊息)");
        
        // 滾動到底部
        StartCoroutine(ScrollToBottom());
    }

    /// <summary>
    /// 取得訊息歷史記錄
    /// </summary>
    public List<MessageModel> GetMessageHistory()
    {
        MessageHistoryData historyData = FileManager.Instance.LoadChatHistory(saveFileName);
        return historyData?.messages ?? new List<MessageModel>();
    }
}
