using UnityEngine;

public class StaticTexture: TextureProvider
{

    public Texture2D staticTexture;

#if UNITY_ANDROID && !UNITY_EDITOR
    void Awake()
    {
        Destroy(gameObject);
    }
#else
    void Start()
    {
        texture = staticTexture;
    }
#endif

}