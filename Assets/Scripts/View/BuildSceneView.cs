

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
        EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent("嗨嗨~", Color.black, 5));
    }
    void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<newAIMessageEvent>(OnAIMessage);
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
        SceneManager.LoadScene("ChatScene");
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
                    EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent(part, Color.black, 3));

                }
            }
        }
        else
        {
            EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent(e.aiMessage, Color.black, 5));

        }

    }
    
    public void onClickSendButton()
    {
        if (RequestManager.instance.is_thinking)
        {
            EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent("哎呀~不要這麼急嗎~", Color.black, 1));
        }
        if (messageInputField.text != "")
        {
            EventBus.Instance.Publish<newUserMessageEvent>(new newUserMessageEvent(messageInputField.text));
            messageInputField.text = "";
        }
    }
}