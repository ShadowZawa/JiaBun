using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    
    public Button gpsButton;
    public Button getRestaurantButton;
    public Button getConversationButton;
    public TextMeshProUGUI conversationText;


    void Start()
    {
        EventBus.Instance.Subscribe<GPSRecievedEvent>(updateGPSButton);
        EventBus.Instance.Subscribe<RestaurantDataReceivedEvent>(sendConversationRequest);
        EventBus.Instance.Subscribe<RestaurantConversationRecievedEvent>(getConversationResponse);
        gpsButton.onClick.AddListener(onClickGPS);
        getRestaurantButton.onClick.AddListener(onClickGetRestaurant);

    }
    void updateGPSButton(GPSRecievedEvent data)
    {
        gpsButton.GetComponentInChildren<TextMeshProUGUI>().text = $"定位完成";

    }
    void sendConversationRequest(RestaurantDataReceivedEvent data)
    {
        getRestaurantButton.GetComponentInChildren<TextMeshProUGUI>().text = "正在幫您挑選最心儀的餐廳中...";
        RequestManager.instance.getRestaurantConversation(data.places, "初次見面 推薦點大眾口味");
    }
    void getConversationResponse(RestaurantConversationRecievedEvent data)
    {
        getRestaurantButton.GetComponentInChildren<TextMeshProUGUI>().text = "取得餐廳";
        conversationText.text = data.conversation.resultConversation;
    }
    void onClickGPS()
    {
        gpsButton.GetComponentInChildren<TextMeshProUGUI>().text = "定位中...";
        if (!GPSManager.instance.is_locating){
            GPSManager.instance.getLocationRequest();
        }
    }
    void onClickGetRestaurant()
    {
        //if (gettingRestaurant || gettingConversation) return;   
        print("clicked get restaurant");
        RequestManager.instance.getRestaurant(GPSManager.instance.latitude, GPSManager.instance.longitude, 1000);
        getRestaurantButton.GetComponentInChildren<TextMeshProUGUI>().text = "取得餐廳中...";
    }
}
