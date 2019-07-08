using UnityEngine;

public class AndroidToast
{

    private static AndroidToast instance = null;
    public static AndroidToast Instance
    {
        get
        {
            if (instance == null)
                instance = new AndroidToast();
            return instance;
        }
    }

    private AndroidJavaClass m_ToastClass;
    private AndroidJavaObject m_toastObject = null;

    public AndroidToast()
    {
        m_ToastClass = new AndroidJavaClass("android.widget.Toast");
    }

    public void makeText(
        AndroidJavaObject currentActivity, 
        AndroidJavaObject applicationContext, 
        string msg
    )
    {
        AndroidJavaObject msgObject = AndroidUtils.toJavaString(msg);

        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
            m_toastObject = m_ToastClass.CallStatic<AndroidJavaObject>(
                "makeText", applicationContext, msgObject, m_ToastClass.GetStatic<int>("LENGTH_SHORT")
            );
            m_toastObject.Call("show");
        }));
    }

    public void cancel(AndroidJavaObject currentActivity)
    {
        if (m_toastObject != null)
        {
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                m_toastObject.Call("cancel");
            }));
        }
    }

}
