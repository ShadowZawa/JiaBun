
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class FileManager : MonoBehaviour
{
    private static FileManager instance;
    public static FileManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("FileManager");
                instance = go.AddComponent<FileManager>();
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
        EventBus.Instance.Subscribe<newAIMessageEvent>(onRecieveMessage);
    }
    private void onRecieveMessage(newAIMessageEvent e)
    {
        MessageHistoryData data = LoadChatHistory();
        data.messages.Add(new MessageModel
        {
            sender = MessageModel.SenderType.User,
            message = e.userMessage,
            timestamp = System.DateTime.Now
        });
        data.messages.Add(new MessageModel {
            sender = MessageModel.SenderType.AI,
            message = e.aiMessage,
            timestamp = System.DateTime.Now
        });
        SaveChatHistory(data.messages, e.conversationData);   
    }
    /// <summary>
    /// 儲存聊天記錄到本地
    /// </summary>
    /// <param name="messageHistory">訊息歷史列表</param>
    /// <param name="conversationData">對話摘要資料</param>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>是否儲存成功</returns>
    public bool SaveChatHistory(List<MessageModel> messageHistory, string conversationData, string fileName = "chat_history.json")
    {
        try
        {
            string savePath = Path.Combine(Application.persistentDataPath, fileName);
            
            MessageHistoryData historyData = new MessageHistoryData
            {
                messages = messageHistory,
                conversationData = conversationData
            };

            string json = JsonUtility.ToJson(historyData, true);
            File.WriteAllText(savePath, json);
            
            Debug.Log($"[FileManager] 聊天記錄已儲存至 {savePath} ({messageHistory.Count} 則訊息)");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FileManager] 儲存聊天記錄失敗: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 從本地載入聊天記錄
    /// </summary>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>訊息歷史資料，如果失敗則返回 null</returns>
    public MessageHistoryData LoadChatHistory(string fileName = "chat_history.json")
    {
        try
        {
            string savePath = Path.Combine(Application.persistentDataPath, fileName);

            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                MessageHistoryData historyData = JsonUtility.FromJson<MessageHistoryData>(json);

                if (historyData != null && historyData.messages != null)
                {
                    Debug.Log($"[FileManager] 從 {savePath} 載入 {historyData.messages.Count} 則歷史訊息");
                    return historyData;
                }
                else
                {
                    Debug.LogWarning($"[FileManager] 檔案存在但內容無法解析或 messages 為 null，會覆寫為空的歷史記錄: {savePath}");
                }
            }
            else
            {
                Debug.Log($"[FileManager] 沒有找到歷史聊天記錄，將建立新的檔案: {savePath}");
            }

            // 如果檔案不存在或內容不可用，建立一個預設的空記錄並儲存
            MessageHistoryData empty = new MessageHistoryData
            {
                messages = new List<MessageModel>(),
                conversationData = ""
            };

            // 使用 SaveChatHistory 寫入檔案（會有內部錯誤處理）
            SaveChatHistory(empty.messages, empty.conversationData, fileName);
            return empty;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FileManager] 載入聊天記錄失敗: {ex.Message}");
        }

        // 若發生不可預期例外，回傳一個空的 MessageHistoryData 作為保護
        return new MessageHistoryData { messages = new List<MessageModel>(), conversationData = "" };
    }

    /// <summary>
    /// 刪除聊天記錄檔案
    /// </summary>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>是否刪除成功</returns>
    public bool DeleteChatHistory(string fileName = "chat_history.json")
    {
        try
        {
            string savePath = Path.Combine(Application.persistentDataPath, fileName);
            
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log($"[FileManager] 聊天記錄已刪除: {savePath}");
                return true;
            }
            else
            {
                Debug.LogWarning($"[FileManager] 找不到要刪除的檔案: {savePath}");
                return false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FileManager] 刪除聊天記錄失敗: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 檢查聊天記錄檔案是否存在
    /// </summary>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>檔案是否存在</returns>
    public bool ChatHistoryExists(string fileName = "chat_history.json")
    {
        string savePath = Path.Combine(Application.persistentDataPath, fileName);
        return File.Exists(savePath);
    }

    /// <summary>
    /// 取得儲存路徑
    /// </summary>
    /// <param name="fileName">檔案名稱</param>
    /// <returns>完整的檔案路徑</returns>
    public string GetSavePath(string fileName = "chat_history.json")
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }
}
