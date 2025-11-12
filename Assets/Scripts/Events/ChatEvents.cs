using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 聊天對話回應事件
/// </summary>
public class ChatConversationReceivedEvent
{
    public ChatConversationResponseModel response;
    public ChatConversationReceivedEvent(ChatConversationResponseModel model)
    {
        response = model;
    }
}
public class newAIMessageEvent
{
    public string aiMessage;
    public newAIMessageEvent(string message)
    {
        aiMessage = message;
    }
}
