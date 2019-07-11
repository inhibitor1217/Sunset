using UnityEngine;

public class StaticTexture: TextureProvider
{

    public Texture2D staticTexture;

    public override bool Draw()
    {
        return true;
    }

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