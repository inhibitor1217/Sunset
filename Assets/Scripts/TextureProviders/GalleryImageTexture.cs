using UnityEngine;

public class GalleryImageTexture: TextureProvider
{
#if UNITY_ANDROID && !UNITY_EDITOR
    void Start()
    {
        LoadTexture(PlayerPrefs.GetString("image_path"));
    }

    public void LoadTexture(string path)
    {
        if (path != "")
            texture = NativeGallery.LoadImageAtPath(path);
    }
#else
    void Awake()
    {
        Destroy(gameObject);
    }
#endif
}