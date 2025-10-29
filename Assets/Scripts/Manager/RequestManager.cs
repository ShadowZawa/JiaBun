using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RequestManager : MonoBehaviour
{
    private static RequestManager _instance;
    public static RequestManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("RequestManager");
                _instance = go.AddComponent<RequestManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    public void Awake()
    {
        if (_instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    public void getRestaurant(float latitude, float longitude)
    {
        StartCoroutine(GetRestaurantCoroutine(latitude, longitude));
    }
    
    private IEnumerator GetRestaurantCoroutine(float latitude, float longitude)
    {
        
        UnityWebRequest request = UnityWebRequest.Get($"https://serpapi.com/search.json?engine=google_maps&q=Restaurant&ll=@{latitude},{longitude},1000m");
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Restaurants data received: " + request.downloadHandler.text);
            // Process the restaurant data here
        }
        else
        {
            Debug.LogError("Error fetching restaurant data: " + request.error);
        }
        
        request.Dispose();
    }
    

}
