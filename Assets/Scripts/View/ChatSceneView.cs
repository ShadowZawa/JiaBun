using UnityEngine;

public class ChatSceneView : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void onClickBackButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}
