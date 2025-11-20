using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour
{
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    public static EventBus Instance;
    private Dictionary<Type, List<Delegate>> eventHandlers = new Dictionary<Type, List<Delegate>>();

    public void Subscribe<T>(Action<T> handler)
    {
        Type eventType = typeof(T);

        if (!eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType] = new List<Delegate>();
        }
        
        // 防止重複訂閱
        if (!eventHandlers[eventType].Contains(handler))
        {
            eventHandlers[eventType].Add(handler);
        }
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        Type eventType = typeof(T);
        if (eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType].Remove(handler);
        }
    }

    public void Publish<T>(T eventData)
    {
        Type eventType = typeof(T);
        if (eventHandlers.ContainsKey(eventType))
        {
            // 複製列表以避免在迭代時修改集合
            var handlers = new List<Delegate>(eventHandlers[eventType]);
            foreach (var handler in handlers)
            {
                try
                {
                    ((Action<T>)handler)(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error handling event {eventType}: {e}");
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            eventHandlers.Clear();
            Instance = null;
        }
    }
}