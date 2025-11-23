using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(LayoutElement))]
public class BubbleViewFix : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    private LayoutElement layoutElement;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        layoutElement = GetComponent<LayoutElement>();
        Init();
    }
    public void Init()
    {
        tmp.ForceMeshUpdate();
        layoutElement.preferredHeight = tmp.preferredHeight;
    }

    void LateUpdate()
    {
        //tmp.ForceMeshUpdate();
        //layoutElement.preferredHeight = tmp.preferredHeight;
    }
}
