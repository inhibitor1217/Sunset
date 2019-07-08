using UnityEngine;

public class AndroidContextManager : MonoBehaviour
{

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaClass m_unityPlayer;

    private AndroidJavaObject m_currentActivity;
    public AndroidJavaObject CurrentActivity { get { return m_currentActivity; } }
    
    private AndroidJavaObject m_applicationContext;
    public AndroidJavaObject ApplicationContext { get { return m_applicationContext; } }
#endif

    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        m_unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        m_currentActivity = m_unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        m_applicationContext = m_currentActivity.Call<AndroidJavaObject>("getApplicationContext");
#else
        Destroy(gameObject);
#endif
    }
}
