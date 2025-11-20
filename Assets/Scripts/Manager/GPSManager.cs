using System.Collections;
using UnityEngine;

public class GPSManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private static GPSManager _instance;
    public static GPSManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GPSManager");
                _instance = go.AddComponent<GPSManager>();
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
    public float latitude;
    public float longitude;
    public bool has_located = false;
    public bool is_locating = false;
    void Start()
    {
        getLocationRequest();    
    }
    public void getLocationRequest()
    {
        if (is_locating) return;
        StartCoroutine(GetLocation());
    }

    public IEnumerator GetLocation()
    {
        print("Getting Location...");
        is_locating = true;

        // Uncomment if you want to test with Unity Remote
        
#if UNITY_EDITOR
        yield return new WaitWhile(() => !UnityEditor.EditorApplication.isRemoteConnected);
        yield return new WaitForSecondsRealtime(5f);
#endif


#if UNITY_EDITOR
        // if in editor
#elif UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.CoarseLocation)) {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.CoarseLocation);
        }

        // check permission
        if (!UnityEngine.Input.location.isEnabledByUser) {
            is_locating = false;
            Debug.LogFormat("Android and Location not enabled");
            yield break;
        }

#elif UNITY_IOS
        if (!UnityEngine.Input.location.isEnabledByUser) {
            is_locating = false;
            Debug.LogFormat("IOS and Location not enabled");
            yield break;
        }
#endif
        // setup loc
        UnityEngine.Input.location.Start(500f, 500f);

        // setup max wait time
        int maxWait = 15;
        while (UnityEngine.Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSecondsRealtime(1);
            maxWait--;
        }

        // check if reach wait max
#if UNITY_EDITOR
        int editorMaxWait = 15;
        while (UnityEngine.Input.location.status == LocationServiceStatus.Stopped && editorMaxWait > 0)
        {
            yield return new WaitForSecondsRealtime(1);
            editorMaxWait--;
        }
#endif

        // check if reach max wait
        if (maxWait < 1)
        {
            is_locating = false;
            Debug.LogFormat("Timed out");
            yield break;
        }

        // fail
        if (UnityEngine.Input.location.status != LocationServiceStatus.Running)
        {
            is_locating = false;
            Debug.LogFormat("Unable to determine device location. Failed with status {0}", UnityEngine.Input.location.status);
            yield break;
        }
        else
        {
            Debug.LogFormat("Location service live. status {0}", UnityEngine.Input.location.status);
            // print loc
            Debug.LogFormat("Location: "
                + UnityEngine.Input.location.lastData.latitude + " "
                + UnityEngine.Input.location.lastData.longitude + " "
                + UnityEngine.Input.location.lastData.altitude + " "
                + UnityEngine.Input.location.lastData.horizontalAccuracy + " "
                + UnityEngine.Input.location.lastData.timestamp);

            latitude = (float)UnityEngine.Input.location.lastData.latitude;
            longitude = (float)UnityEngine.Input.location.lastData.longitude;
            print(latitude.ToString() + ", " + longitude.ToString());
            has_located = true;
            // 發布 GPS 位置獲取成功事件
            GPSRecievedEvent locationData = new GPSRecievedEvent(
                latitude, 
                longitude, 
                UnityEngine.Input.location.lastData.altitude,
                UnityEngine.Input.location.lastData.horizontalAccuracy,
                UnityEngine.Input.location.lastData.timestamp
            );
            EventBus.Instance.Publish(locationData);
        }
        is_locating = false;
        // stop
        UnityEngine.Input.location.Stop();
    }
    LocationInfo li;


}
