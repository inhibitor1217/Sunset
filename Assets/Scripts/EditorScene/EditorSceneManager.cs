using System.Collections;
using UnityEngine;

public class EditorSceneManager : MonoBehaviour
{
    
    public MainImage mainImage;

#if UNITY_ANDROID && !UNITY_EDITOR
    
#else
    public Texture2D defaultTexture;
#endif

    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Load Image from Global Storage
        string path = PlayerPrefs.GetString("image_path");

        if (path != "")
            LoadImageFromPath(path);
#else
        if (defaultTexture && mainImage)
            LoadImageFromTexture(defaultTexture);
#endif
    }

    void Update()
    {
        // Handle Escape Button
        if (Input.GetKey(KeyCode.Escape))
            Application.LoadLevel(0);
    }

    public void LoadImageFromPath(string path)
    {
        StartCoroutine(asyncLoadTextureFromGallery(path));
    }

    public void LoadImageFromTexture(Texture2D texture)
    {
        StartCoroutine(asyncLoadTexture(texture));
    }

    IEnumerator asyncLoadTextureFromGallery(string path)
    {
        yield return new WaitForEndOfFrame();

        if (mainImage)
            mainImage.SetTexture(NativeGallery.LoadImageAtPath(path));
    }

    IEnumerator asyncLoadTexture(Texture2D texture)
    {
        yield return new WaitForEndOfFrame();

        if (mainImage)
            mainImage.SetTexture(texture);
    }

}
