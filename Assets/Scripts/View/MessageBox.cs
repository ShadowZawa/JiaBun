








using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageBox : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    private Queue<MessageData> messageQueue = new Queue<MessageData>();
    private bool isDisplaying = false;
    private Coroutine displayCoroutine; // 儲存當前顯示協程的引用

    private class MessageData
    {
        public string message;
        public Color color;
        public int time;

        public MessageData(string message, Color color, int time)
        {
            this.message = message;
            this.color = color;
            this.time = time;
        }
    }
    
    void Start()
    {
        EventBus.Instance.Subscribe<showMessageBoxEvent>(showMessageBox);
        messageText.text = "";
        
    }
    
    void OnDestroy()
    {
        // 停止所有協程，避免在物件銷毀後繼續執行
        StopAllCoroutines();
        
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<showMessageBoxEvent>(showMessageBox);
        }
    }
    public void nextBox()
    {
        // 直接跳至下一個訊息
        if (isDisplaying && displayCoroutine != null)
        {
            // 停止當前的顯示協程
            StopCoroutine(displayCoroutine);
            
            // 立即開始顯示下一個訊息
            displayCoroutine = StartCoroutine(DisplayMessages());
        }
    }

    void showMessageBox(showMessageBoxEvent e)
    {
        // 檢查物件是否仍然有效
        if (this == null || !this.gameObject.activeInHierarchy)
        {
            return;
        }
        
        messageQueue.Enqueue(new MessageData(e.message, e.color, e.time));
        
        if (!isDisplaying)
        {
            displayCoroutine = StartCoroutine(DisplayMessages());
        }
    }

    private IEnumerator DisplayMessages()
    {
        isDisplaying = true;

        while (messageQueue.Count > 0)
        {
            MessageData data = messageQueue.Dequeue();
            messageText.text = data.message;
            messageText.color = data.color;
            if (messageText.GetComponent<BubbleViewFix>() != null) messageText.GetComponent<BubbleViewFix>().Init();   
            yield return new WaitForSeconds(data.time);
        }

        messageText.text = "";
        isDisplaying = false;
    }
}