using UnityEngine;
using TMPro;

public class NewChatView : MonoBehaviour
{
    public GameObject newChatBox;
    public RectTransform chatContent;
    
    [Header("聊天設定")]
    public float userXPosition = 600f; // 使用者訊息 X 位置（靠右）
    public float aiXPosition = 200f; // AI 訊息 X 位置（靠左）
    public float verticalSpacing = 100f; // 每個氣泡間的垂直間距（正數）
    public float bubbleHeightPerLine = 20f; // 每行文字的氣泡高度
    public float maxTextWidth = 160f; // 文字最大寬度
    
    private float lastYPosition = 0f;
    
    void Start()
    {
        ReBuildChat();
    }

    void AddChat(string message, bool isUser)
    {
        // 檢查是否包含 [next_bubble] 標記，若有則拆分
        if (message.Contains("[next_bubble]"))
        {
            string[] parts = message.Split(new string[] { "[next_bubble]" }, System.StringSplitOptions.None);
            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    AddSingleChatBubble(part.Trim(), isUser);
                }
                lastYPosition += verticalSpacing;
            }
            // 拆分後也要更新高度
            //UpdateContentHeight();
        }
        else
        {
            AddSingleChatBubble(message, isUser);
        }
    }
    
    void AddSingleChatBubble(string message, bool isUser)
    {
        GameObject chatBubble = Instantiate(newChatBox, chatContent);
        RectTransform bubbleRect = chatBubble.GetComponent<RectTransform>();
        
        // 設定錨點
        bubbleRect.anchorMin = new Vector2(0.5f, 1f);
        bubbleRect.anchorMax = new Vector2(0.5f, 1f);
        
        // 設定氣泡寬度
        bubbleRect.sizeDelta = new Vector2(maxTextWidth, bubbleRect.sizeDelta.y);
        
        // 取得文字組件並設定內容
        TextMeshProUGUI chatText = chatBubble.GetComponentInChildren<TextMeshProUGUI>();
        if (chatText != null)
        {
            // 設定文字內容
            chatText.text = message;
            
            // 強制更新以計算正確的行數
            Canvas.ForceUpdateCanvases();
            chatText.ForceMeshUpdate();
            
            // 計算行數並調整氣泡高度
            int lineCount = Mathf.Max(1, chatText.textInfo.lineCount);
            float bubbleHeight = bubbleHeightPerLine * lineCount;
            bubbleRect.sizeDelta = new Vector2(maxTextWidth, bubbleHeight);
        }
        
        // 設定位置：使用者靠右，AI 靠左
        float xPos = isUser ? userXPosition : aiXPosition;
        // 計算新氣泡的 Y 位置：上一個氣泡底部 + 間距(verticalSpacing 是負數)
        float yPos = lastYPosition + verticalSpacing;
        bubbleRect.localPosition = new Vector3(xPos, yPos, 0f);
        
        // 更新最後的 Y 位置：當前氣泡的頂部 - 氣泡高度 = 當前氣泡的底部
        lastYPosition = yPos - bubbleRect.sizeDelta.y;
        
        // 每次新增氣泡後更新 content 高度
        UpdateContentHeight();
    }
    
    void UpdateContentHeight()
    {
        float totalHeight = Mathf.Abs(lastYPosition) + 1000f; // 加上底部緩衝空間
        chatContent.sizeDelta = new Vector2(chatContent.sizeDelta.x, totalHeight);
    }

    void ReBuildChat()
    {
        // 清除所有現有氣泡
        foreach (Transform child in chatContent)
        {
            Destroy(child.gameObject);
        }
        
        // 重設位置
        lastYPosition = 0f;
        
        // 載入歷史記錄
        MessageHistoryData data = FileManager.Instance.LoadChatHistory();
        if (data != null && data.messages != null)
        {
            foreach (MessageModel d in data.messages)
            {
                AddChat(d.message, d.sender == MessageModel.SenderType.User);
            }
        }
        
        // 最終更新高度
        UpdateContentHeight();
    }
    
    // 公開方法供外部呼叫
    public void OnNewMessage(string message, bool isUser)
    {
        AddChat(message, isUser);
    }
    
    public void RefreshChat()
    {
        ReBuildChat();
    }

    public void OnClickBackButton()
    {
        // 在這裡處理返回按鈕的邏輯，例如切換場景或關閉視窗
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}
