using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Button gpsButton;

    void Start()
    {
        EventBus.Instance.Subscribe<GPSRecievedEvent>(updateGPSButton);
        gpsButton.onClick.AddListener(onClickGPS);
    }
    void updateGPSButton(GPSRecievedEvent data){
        gpsButton.GetComponentInChildren<TextMeshProUGUI>().text = $"定位完成";

    }
    void onClickGPS(){
        gpsButton.GetComponentInChildren<TextMeshProUGUI>().text = "定位中...";
        GPSManager.instance.getLocationRequest();
    }
}
