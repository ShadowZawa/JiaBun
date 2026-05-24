using System.Collections.Generic;



/// <summary>
/// 聊天對話請求的資料模型
/// </summary>
[System.Serializable]
public class ChatConversationRequestModel
{
    public string summary;
    public List<ChatMessageModel> message_history;
    public string message;
    public int affinity;
}

/// <summary>
/// 聊天訊息模型（用於 API 請求）
/// </summary>
[System.Serializable]
public class ChatMessageModel
{
    public string sender;
    public string message;
    public string timestampString;

    /// <summary>
    /// 從 MessageModel 轉換
    /// </summary>
    public static ChatMessageModel FromMessageModel(MessageModel msg)
    {
        return new ChatMessageModel
        {
            sender = msg.sender.ToString(),
            message = msg.message,
            timestampString = msg.timestampString
        };
    }
}

/// <summary>
/// 聊天對話回應的資料模型
/// </summary>
[System.Serializable]
public class ChatConversationResponseModel
{
    public string reply;
    public string map_url="";
    public string summary;
    public int affinity;
}