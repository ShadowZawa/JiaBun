

using UnityEngine;
using UnityEngine.SceneManagement;
public class BuildSceneView : MonoBehaviour
{
    void Start()
    {
    }

    public void onClickChatButton()
    {
        SceneManager.LoadScene("ChatScene");
    }
}