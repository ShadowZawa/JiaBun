using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif
using System;

public class NotificationManager : MonoBehaviour
{
    private static NotificationManager instance;
    public static NotificationManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("NotificationManager");
                instance = go.AddComponent<NotificationManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Start()
    {
        InitializeNotifications();
    }

    void InitializeNotifications()
    {
#if UNITY_ANDROID
        // Android 13 (API 33) 以上需要動態請求通知權限
        RequestNotificationPermission();
        
        // 建立 Android 通知頻道
        var channel = new AndroidNotificationChannel()
        {
            Id = "jiabun_daily_channel",
            Name = "甲奔每日提醒",
            Importance = Importance.Default,
            Description = "每日用餐時間提醒",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        
        // 清除之前的所有通知
        AndroidNotificationCenter.CancelAllNotifications();
        AndroidNotificationCenter.CancelAllScheduledNotifications();
        
        Debug.Log("[NotificationManager] Android 通知頻道已建立");
#elif UNITY_IOS
        // iOS 請求通知權限
        var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
        using (var req = new AuthorizationRequest(authorizationOption, true))
        {
            while (!req.IsFinished)
            {
                // 等待授權完成
            }
            Debug.Log($"[NotificationManager] iOS 通知權限: {req.Granted}");
        }
        
        // 清除之前的所有通知
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        iOSNotificationCenter.RemoveAllDeliveredNotifications();
        
        // iOS 直接排程通知
        ScheduleDailyNotifications();
#endif
    }

#if UNITY_ANDROID
    /// <summary>
    /// 請求 Android 通知權限（Android 13+ 需要）
    /// </summary>
    private void RequestNotificationPermission()
    {
        // Android 13 (API 33) 以上需要 POST_NOTIFICATIONS 權限
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Debug.Log("[NotificationManager] 請求通知權限中...");
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
        else
        {
            Debug.Log("[NotificationManager] 通知權限已授予");
        }
        
        // 無論權限狀態如何，都嘗試排程通知
        // Android 12 及以下版本不需要此權限
        ScheduleDailyNotifications();
    }

    /// <summary>
    /// 檢查通知權限狀態的回調
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // 當應用重新獲得焦點時，檢查權限狀態
            if (Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Debug.Log("[NotificationManager] 通知權限已啟用，確保通知已排程");
                // 確保通知已排程
                ScheduleDailyNotifications();
            }
        }
    }
#endif

    public void ScheduleDailyNotifications()
    {
        // 排程每日早上 11:00 的通知
        ScheduleNotificationAt(11, 0, "午餐時間快到囉！", "該吃午餐了～來看看附近有什麼好吃的吧！🍱");
        
        // 排程每日下午 17:00 的通知
        ScheduleNotificationAt(17, 0, "晚餐時間快到囉！", "該吃晚餐了～讓我幫你找找附近的美食！🍜");
        
        Debug.Log("[NotificationManager] 每日通知已排程完成");
    }

    private void ScheduleNotificationAt(int hour, int minute, string title, string message)
    {
        DateTime now = DateTime.Now;
        DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
        
        // 如果今天的時間已過，則排程到明天
        if (scheduledTime <= now)
        {
            scheduledTime = scheduledTime.AddDays(1);
        }

        TimeSpan timeUntilNotification = scheduledTime - now;

#if UNITY_ANDROID
        var notification = new AndroidNotification
        {
            Title = title,
            Text = message,
            SmallIcon = "icon_small",
            LargeIcon = "icon_large",
            FireTime = DateTime.Now.Add(timeUntilNotification),
            RepeatInterval = new TimeSpan(24, 0, 0) // 每 24 小時重複一次
        };

        int notificationId = AndroidNotificationCenter.SendNotification(notification, "jiabun_daily_channel");
        Debug.Log($"[NotificationManager] Android 通知已排程 ID: {notificationId}, 時間: {hour:D2}:{minute:D2}, 首次觸發: {scheduledTime}");
        
#elif UNITY_IOS
        var timeTrigger = new iOSNotificationCalendarTrigger()
        {
            Hour = hour,
            Minute = minute,
            Repeats = true // 每天重複
        };

        var notification = new iOSNotification()
        {
            Identifier = $"daily_notification_{hour}_{minute}",
            Title = title,
            Body = message,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound | PresentationOption.Badge),
            CategoryIdentifier = "jiabun_daily",
            ThreadIdentifier = "jiabun_thread",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
        Debug.Log($"[NotificationManager] iOS 通知已排程，時間: {hour:D2}:{minute:D2}");
#endif
    }

    /// <summary>
    /// 取消所有已排程的通知
    /// </summary>
    public void CancelAllNotifications()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllScheduledNotifications();
        Debug.Log("[NotificationManager] 已取消所有 Android 通知");
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        Debug.Log("[NotificationManager] 已取消所有 iOS 通知");
#endif
    }

    /// <summary>
    /// 重新排程所有通知（可在設定變更時調用）
    /// </summary>
    public void RescheduleNotifications()
    {
        CancelAllNotifications();
        ScheduleDailyNotifications();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // App 回到前景時，清除所有已顯示的通知
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllDisplayedNotifications();
#elif UNITY_IOS
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
#endif
        }
    }
}
