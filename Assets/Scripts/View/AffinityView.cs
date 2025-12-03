
using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEditor.Rendering.Universal;
using UnityEngine;

class AffinityView : MonoBehaviour
{

    public TextMeshProUGUI affinityText;
    void Start()
    {
        EventBus.Instance.Subscribe<newAIMessageEvent>(UpdateAffinityDisplay);
        StartCoroutine(updateAffinityDisplayCoroutine());
    }
     void UpdateAffinityDisplay(newAIMessageEvent e)
    {
        StartCoroutine(updateAffinityDisplayCoroutine());
    }
    IEnumerator updateAffinityDisplayCoroutine()
    {
        yield return new WaitForSeconds(0.5f); // 等待半秒，確保數值已更新
        MessageHistoryData data = FileManager.Instance.LoadChatHistory();
        string affinityName = AffinityToName(data.affinity) + " (" + (data.affinity % 200).ToString() + "/200)";
        affinityText.text = affinityName;
    }

    void OnDestroy()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<newAIMessageEvent>(UpdateAffinityDisplay);
        }
    }
    string AffinityToName(int affinity)
    {
        switch (affinity / 200)
        {
            case 0:
                return "初次相遇";
            case 1:
                return "尋常好友";
            case 2:
                return "推心置腹";
            case 3:
                return "莫逆之交";
            case 4:
                return "靈魂伴侶";
            case 5:
                return "生死之交";
            default:
                if (affinity > 1000)
                {
                    return "命中注定";
                }
                else
                {
                    //<0
                    return "形同陌路";

                }
        }
    }
}