using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 聊天對話回應事件
/// </summary>

public class newAIMessageEvent
{
    public string aiMessage;
    public string userMessage;
    public string conversationData;
    public newAIMessageEvent(string aiMessage, string userMessage, string conversationData)
    {
        this.aiMessage = aiMessage;
        this.userMessage = userMessage;
        this.conversationData = conversationData;
    }

}
public class newUserMessageEvent
{
    public string userMessage;
    public newUserMessageEvent(string message)
    {
        userMessage = message;
    }
}
