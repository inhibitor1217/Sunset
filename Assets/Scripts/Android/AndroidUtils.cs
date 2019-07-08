using UnityEngine;

public static class AndroidUtils
{

    public static AndroidJavaObject toJavaString(string str)
    {
        return new AndroidJavaObject("java.lang.String", str);
    }

}