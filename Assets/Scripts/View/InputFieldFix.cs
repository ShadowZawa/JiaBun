
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InputFieldFix : MonoBehaviour
{
    private void OnEnable()
    {
        TMP_InputField field = gameObject.GetComponent<TMP_InputField>();
        field.shouldHideMobileInput = true;
        field.ActivateInputField();
    }
}