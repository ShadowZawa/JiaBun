using System.Collections;
using UnityEngine;

public class GPSManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GPSManager instance;

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }
    public float latitude;
    public float longitude;
    void Start()
    {
        StartCoroutine(GetLocation());
    }

    public IEnumerator GetLocation()
    {
        print("Getting Location...");
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
            Debug.LogFormat("Android and Location not enabled");
            yield break;
        }

#elif UNITY_IOS
        if (!UnityEngine.Input.location.isEnabledByUser) {
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
            Debug.LogFormat("Timed out");
            yield break;
        }

        // fail
        if (UnityEngine.Input.location.status != LocationServiceStatus.Running)
        {
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
            // TODO success do something with location
        }

        // stop
        UnityEngine.Input.location.Stop();
    }
    LocationInfo li;


}
