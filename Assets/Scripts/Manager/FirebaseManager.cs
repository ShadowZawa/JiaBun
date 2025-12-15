using UnityEngine;
using Firebase.Auth;
using Unity.VisualScripting;

public class FirebaseManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private FirebaseAuth _auth;

    void Start()
    {
        _auth = FirebaseAuth.DefaultInstance;
    }
    public void Register(string email, string password)
    {
        _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Firebase Register Failed: " + task.Exception);
                return;
            }
            FirebaseUser user = task.Result.User;
            Debug.LogFormat("Firebase Register Succeeded: {0} ({1})", user.DisplayName, user.UserId);
        });
    }

    public void Login(string email, string password)
    {
        _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Firebase Login Failed: " + task.Exception);
                return;
            }
            FirebaseUser user = task.Result.User;
            Debug.LogFormat("Firebase Login Succeeded: {0} ({1})", user.DisplayName, user.UserId);
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
