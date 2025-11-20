

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
public class BuildSceneView : MonoBehaviour
{

    public TMP_InputField messageInputField;
    public TextMeshProUGUI gpsButtonText;
    void Start()
    {
        EventBus.Instance.Subscribe<newAIMessageEvent>(OnAIMessage);
        EventBus.Instance.Subscribe<GPSRecievedEvent>(onGetGPS);
        EventBus.Instance.Subscribe<RestaurantConversationRecievedEvent>(onGetRestaurantMessage);
        EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent("嗨嗨~", Color.black, 5));
    }
    void OnDestroy()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<newAIMessageEvent>(OnAIMessage);
            EventBus.Instance.Unsubscribe<GPSRecievedEvent>(onGetGPS);
            EventBus.Instance.Unsubscribe<RestaurantConversationRecievedEvent>(onGetRestaurantMessage);
        }
    }
    void onGetRestaurantMessage(RestaurantConversationRecievedEvent e)
    {
        // 檢查是否包含 \n，如果有則分割
        if (e.conversation.resultConversation.Contains("[next_bubble]"))
        {
            string[] parts = e.conversation.resultConversation.Split(new string[] { "[next_bubble]" }, System.StringSplitOptions.None);
            foreach (string part in parts)
            {
                if (part != null)
                {
                    EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent(part, Color.black, 5));

                }
            }
        }
        else
        {
            EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent(e.conversation.resultConversation, Color.black, 8));

        }
        //EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent(e.conversation.resultConversation, Color.black, 5));
    }
    void onGetGPS(GPSRecievedEvent e)
    {
        //EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent($"定位完成", Color.black, 3));
        gpsButtonText.text = "";
    }
    
    public void OnClickGPSButton()
    {
        GPSManager.instance.getLocationRequest();
    }
    public void onClickChatButton()
    {
        SceneManager.LoadScene("newChatScene");
    }
    public void OnAIMessage(newAIMessageEvent e)
    {

        // 檢查是否包含 \n，如果有則分割
        if (e.aiMessage.Contains("[next_bubble]"))
        {
            string[] parts = e.aiMessage.Split(new string[] { "[next_bubble]" }, System.StringSplitOptions.None);
            foreach (string part in parts)
            {
                if (part != null)
                {
                    EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent(part, Color.black, 5));

                }
            }
        }
        else
        {
            EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent(e.aiMessage, Color.black, 8));

        }

    }
    
    public void onClickSendButton()
    {
        if (messageInputField.text == "[ClearHistory]")
        {
            messageInputField.text = "";
            FileManager.Instance.DeleteChatHistory();
            EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent("聊天記錄已清除", Color.black, 2));
            return;
        }
        if (messageInputField.text.Contains("吃什麼") || messageInputField.text.Contains("吃甚麼") || messageInputField.text.Contains("要吃") || messageInputField.text.Contains("想吃") || messageInputField.text.Contains("吃啥") || messageInputField.text.Contains("餐廳") || messageInputField.text.Contains("午餐") || messageInputField.text.Contains("晚餐") || messageInputField.text.Contains("早餐") || messageInputField.text.Contains("宵夜") || messageInputField.text.Contains("甜點") || messageInputField.text.Contains("附近美食"))
        {
            if (GPSManager.instance.has_located)
            {
                if (RequestManager.instance.is_thinking)
                {
                    EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent("哎呀~ 不要那麼急~", Color.yellow, 2));
                    return;
                }
                if (messageInputField.text == "") return;
                EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent("稍等~我來幫你挑選餐廳", Color.black, 3));
                EventBus.Instance.Publish<newRestaurantRequestEvent>(new newRestaurantRequestEvent(messageInputField.text));
                messageInputField.text = "";
            }
            else
            {
                EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent("我找不到你的位置呢~ 請先點右上角的gps圖標呦", Color.black, 3));
                return;
            }
        }
        if (RequestManager.instance.is_thinking)
        {
            EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent("哎呀~不要這麼急嗎~", Color.black, 1));
        }
        if (messageInputField.text != "")
        {
            if (messageInputField.text == "") return;
            EventBus.Instance.Publish<newUserMessageEvent>(new newUserMessageEvent(messageInputField.text));
            messageInputField.text = "";
        }
    }
}