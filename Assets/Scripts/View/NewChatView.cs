using UnityEngine;
using TMPro;

public class NewChatView : MonoBehaviour
{
    public GameObject newChatBox;
    public RectTransform chatContent;

    private float cumulativeHeight = -300f; // 已累積的總高度（含間距）
    private float bottomPadding = 100f; // 最底部的緩衝高度

    void Start()
    {
        ReBuildChat();
    }

    void AddChat(string message, bool isUser)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (message.Contains("[next_bubble]"))
        {
            string[] parts = message.Split(new string[] { "[next_bubble]" }, System.StringSplitOptions.None);
            foreach (string part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    AddSingleChatBubble(part.Trim(), isUser);
                }
            }
        }
        else
        {
            AddSingleChatBubble(message.Trim(), isUser);
        }
        UpdateContentHeight();
    }

    void AddSingleChatBubble(string message, bool isUser)
    {
        GameObject chatBubble = Instantiate(newChatBox, chatContent);
        RectTransform bubbleRect = chatBubble.GetComponent<RectTransform>();

        // 錨點與 pivot 設為上方中心，方便用負值往下排
        bubbleRect.anchorMin = new Vector2(0f, 1f);
        bubbleRect.anchorMax = new Vector2(0f, 1f);
        bubbleRect.pivot = new Vector2(0f, 1f);

        // 文字元件
        bubbleRect.sizeDelta = new Vector2(170, bubbleRect.sizeDelta.y);
        TextMeshProUGUI chatText = chatBubble.GetComponentInChildren<TextMeshProUGUI>();
        if (chatText != null)
        {
            chatText.text = message;
            chatText.ForceMeshUpdate();
            if (chatText.TryGetComponent<BubbleViewFix>(out var fixer))
            {
                fixer.Init();
            }
        }

        // 強制更新 Canvas 以確保所有佈局組件完成計算
        Canvas.ForceUpdateCanvases();

        float xPos = isUser ? 250 : 0;
        float yPos = -cumulativeHeight; // 往下排列使用負值
        bubbleRect.anchoredPosition = new Vector2(xPos, yPos);

        // 累加實際的 bubble 高度（已被自動設定） + 間距
        float actualHeight = bubbleRect.sizeDelta.y;
        cumulativeHeight += actualHeight*3 + 100f;
    }

    void UpdateContentHeight()
    {
        float required = cumulativeHeight + bottomPadding;
        chatContent.sizeDelta = new Vector2(chatContent.sizeDelta.x, required);
    }

    void ReBuildChat()
    {
        foreach (Transform child in chatContent)
        {
            Destroy(child.gameObject);
        }
        cumulativeHeight = 0f;

        MessageHistoryData data = FileManager.Instance.LoadChatHistory();
        if (data != null && data.messages != null)
        {
            foreach (MessageModel d in data.messages)
            {
                AddChat(d.message, d.sender == MessageModel.SenderType.User);
            }
        }

        UpdateContentHeight();
    }

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
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}
