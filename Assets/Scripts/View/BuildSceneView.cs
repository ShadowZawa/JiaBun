

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BuildSceneView : MonoBehaviour
{

    public TMP_InputField messageInputField;
    public TMP_InputField preferenceInputField;
    public TextMeshProUGUI gpsButtonText;
    public GameObject SettingPanel;

    public UnityEngine.UI.Button GoToButton;
    void Start()
    {
        EventBus.Instance.Subscribe<mapURLUpdateEvent>(OnMapUrlUpdated);
        EventBus.Instance.Subscribe<newAIMessageEvent>(OnAIMessage);
        EventBus.Instance.Subscribe<GPSRecievedEvent>(onGetGPS);
        EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent("嗨嗨~", Color.black, 5));
        InvokeRepeating(nameof(RefreshGoToButton), 1f, 5f);
    }
    void OnDestroy()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<mapURLUpdateEvent>(OnMapUrlUpdated);
            EventBus.Instance.Unsubscribe<newAIMessageEvent>(OnAIMessage);
            EventBus.Instance.Unsubscribe<GPSRecievedEvent>(onGetGPS);
        }
    }
    
    void onGetGPS(GPSRecievedEvent e)
    {
        //EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent($"定位完成", Color.black, 3));
        gpsButtonText.text = "";
    }
    public void onClickSuitButton()
    {
        SceneManager.LoadScene("suitScene");
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

        // 檢查是否包含要分割符號如果有則分割
        if (e.aiMessage.Contains(",,,"))
        {
            string[] parts = e.aiMessage.Split(new string[] { ",,," }, System.StringSplitOptions.None);
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
    public string map_url="";
    void OnMapUrlUpdated(mapURLUpdateEvent e)
    {
        map_url = e.mapURL;
        RefreshGoToButton();
    }

    void RefreshGoToButton()
    {
        if (map_url != "")
        {
            GoToButton.gameObject.SetActive(true);
            GoToButton.onClick.RemoveAllListeners();
            GoToButton.onClick.AddListener(() =>
            {
                Application.OpenURL(map_url);
            });
        }
        else
        {
            GoToButton.gameObject.SetActive(false);
        }
    }
    public void settingPanelToggle()
    {
        SettingPanel.SetActive(!SettingPanel.activeSelf);
    }
    public void onClickLoadPreferenceButton()
    {

        GUIUtility.systemCopyBuffer = FileManager.Instance.LoadChatHistory().conversationData;
        
    }
    public void onClickClearHistoryButton()
    {
        FileManager.Instance.DeleteChatHistory();
    }
    public void onClickSavePreferenceButton()
    {
        if (preferenceInputField.text != "")
        {
            MessageHistoryData data = FileManager.Instance.LoadChatHistory();
            data.conversationData = preferenceInputField.text;
            preferenceInputField.text = "";
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
        if (RequestManager.instance.is_thinking)
        {
            EventBus.Instance.Publish<showMessageBoxEvent>(new showMessageBoxEvent("哎呀~不要這麼急嗎~", Color.black, 1));
            return;
        }
        
            if (messageInputField.text != "")
            {
                if (messageInputField.text == "") return;
                EventBus.Instance.Publish<newUserMessageEvent>(new newUserMessageEvent(messageInputField.text));
                messageInputField.text = "";
            }
    }
}