using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class ChatManager : MonoBehaviour
{
    public GameObject chatItemPrefab;
    public GameObject replyItemPrefab;
    public RectTransform contentRect;
    public ScrollRect scrollRect;
    public TMP_InputField messageInputField;


    private float lastPos = 0f;
    private float stepVertical = -100f;
    void Start()
    {
        BuildChat();
        EventBus.Instance.Subscribe<newAIMessageEvent>(getMessage);
    }   
    public void AddChat(string content, bool isUser)
    {
        GameObject itemPrefab = isUser ? chatItemPrefab : replyItemPrefab;
        GameObject chatItem = Instantiate(itemPrefab, contentRect);
        chatItem.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
        chatItem.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
        TextMeshProUGUI chatText = chatItem.GetComponentInChildren<TextMeshProUGUI>();
        chatText.text = content;
        chatText.ForceMeshUpdate();
        int lineCount = chatText.textInfo.lineCount;
        chatItem.GetComponent<RectTransform>().sizeDelta = new Vector2(chatItem.GetComponent<RectTransform>().sizeDelta.x, 100f * lineCount);
        float xPos = isUser ? 600f : 200f;
        float yPos = stepVertical + lastPos;
        chatItem.transform.localPosition = new Vector3(xPos, yPos, 0f);
        lastPos = yPos - 100f * lineCount;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, -lastPos-1000);
        

    }
    public void BuildChat(){
        // 移除所有子物件
        foreach (Transform child in contentRect)
        {
            Destroy(child.gameObject);
        }
        
        // 重設位置
        lastPos = 0f;
        
        MessageHistoryData data = FileManager.Instance.LoadChatHistory();
        foreach (MessageModel d in data.messages){
            // 檢查是否包含 \n，如果有則分割
            if (d.message.Contains("[next_bubble]"))
            {
                string[] parts = d.message.Split(new string[] { "[next_bubble]" }, System.StringSplitOptions.None);
                foreach (string part in parts)
                {
                    if (part != null)
                    {
                        AddChat(part, d.sender == MessageModel.SenderType.User);
                    }
                }
            }
            else
            {
                AddChat(d.message, d.sender == MessageModel.SenderType.User);
            }
        }
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void SendMessage(){
        if (RequestManager.instance.is_thinking) {
            EventBus.Instance.Publish(new showMessageBoxEvent("請稍後，AI 正在思考中...", Color.yellow, 2));
            return;
        }
        string message = messageInputField.text;
        if (!string.IsNullOrEmpty(message)){
            EventBus.Instance.Publish(new newUserMessageEvent(message));
            AddChat(message, true);
            messageInputField.text = "";
        }
    }
    public void getMessage(newAIMessageEvent e){
        if (e.aiMessage.Contains("[next_bubble]"))
        {
            string[] parts = e.aiMessage.Split(new string[] { "[next_bubble]" }, System.StringSplitOptions.None);
            foreach (string part in parts)
            {
                if (part != null)
                {
                    AddChat(part, false);
                }
            }
        }
        else
        {
            AddChat(e.aiMessage, false); 
        }
    }

}
