using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 聊天對話回應事件
/// </summary>

public class newRestaurantRequestEvent
{
    public string userMessage;
    public newRestaurantRequestEvent(string message)
    {
        userMessage = message;
    }

}
public class newAIMessageEvent
{
    public string aiMessage;
    public string userMessage;
    public string conversationData;
    public int affinity;
    public newAIMessageEvent(string aiMessage, string userMessage, string conversationData, int affinity)
    {
        this.aiMessage = aiMessage;
        this.userMessage = userMessage;
        this.conversationData = conversationData;
        this.affinity = affinity;
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
