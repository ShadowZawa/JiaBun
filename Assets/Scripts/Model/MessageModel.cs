

using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class MessageModel
{
    public enum SenderType
    {
        User,
        AI
    }
    public SenderType sender;
    public string message;
    public string timestampString; // 用於序列化的時間字串

    [System.NonSerialized]
    private System.DateTime _timestamp;

    public System.DateTime timestamp
    {
        get
        {
            if (_timestamp == default(System.DateTime) && !string.IsNullOrEmpty(timestampString))
            {
                System.DateTime.TryParse(timestampString, out _timestamp);
            }
            return _timestamp;
        }
        set
        {
            _timestamp = value;
            timestampString = value.ToString("o"); // ISO 8601 格式
        }
    }

    /// <summary>
    /// 取得格式化的時間字串
    /// </summary>
    public string GetFormattedTimestamp()
    {
        return timestamp.ToString("yyyy-MM-dd HH:mm");
    }
}

/// <summary>
/// 用於序列化訊息列表的包裝類別
/// </summary>
[System.Serializable]
public class MessageHistoryData
{
    public List<MessageModel> messages = new List<MessageModel>();
    public string conversationData = "";
}
