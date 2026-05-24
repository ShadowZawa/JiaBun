using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SuitManager : MonoBehaviour
{

    /*
    Resources/suits/default - 預設時裝
    Resources/suits/suit1 - 時裝1
    */
    private Image _charaImage;
    void Start()
    {
        _charaImage = GetComponent<Image>();
        loadSkin();
    }
    public void onClickBack()
    {
        SceneManager.LoadScene("MainScene");
    }
    public void setSkin(string name)
    {
        string savePath = Path.Combine(Application.persistentDataPath, "skin.txt");
        try
        {
            File.WriteAllText(savePath, name);
            Debug.Log($"[SuitManager] 已儲存時裝: {name} 至 {savePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SuitManager] 儲存時裝失敗: {ex.Message}");
        }
        loadSkin();
    }
    public void loadSkin()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "skin.txt");
        if (File.Exists(savePath))
        {
            
            try
            {
                string skinName = File.ReadAllText(savePath);
                Debug.Log($"[SuitManager] 已載入時裝: {skinName}");
                _charaImage.sprite = Resources.Load<Sprite>($"suits/{skinName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SuitManager] 載入時裝失敗: {ex.Message}");
            }
        }
        else
        {
            //不存在則創建 且使用預設時裝
            setSkin("default");
        }
    }
}
