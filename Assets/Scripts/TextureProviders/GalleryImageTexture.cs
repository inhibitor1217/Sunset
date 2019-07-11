using UnityEngine;

public class GalleryImageTexture: TextureProvider
{
    private string m_Path = "";

    public override bool Draw()
    {
        return true;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    void Start()
    {
        SetPath(PlayerPrefs.GetString("image_path"));
    }

    public void SetPath(string path)
    {
        if (m_Path != "" && path != m_Path)
        {
            m_Path = path;
            texture = NativeGallery.LoadImageAtPath(m_Path);
            textureShouldUpdate = true;
        }
    }
#else
    void Awake()
    {
        Destroy(gameObject);
    }
#endif
}