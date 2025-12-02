using UnityEngine;

/// <summary>
/// 應用程式初始化器，確保所有必要的 Manager 在啟動時被初始化
/// </summary>
public class AppInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        // 初始化通知管理器
        var notificationManager = NotificationManager.Instance;
        Debug.Log("[AppInitializer] NotificationManager 已初始化");
    }
}
