using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorGalleryButton : MonoBehaviour
{
    
    private Button m_Button;

    public GalleryImageTexture textureProvider;

#if UNITY_ANDROID && !UNITY_EDITOR
    void Awake()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(onGalleryButtonClicked);
    }

    void onGalleryButtonClicked()
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery(
            (path) => {
                textureProvider.SetPath(path);
            }
        );
    }
#else
    void Awake()
    {
        Destroy(gameObject);
    }
#endif

}
