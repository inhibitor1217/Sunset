using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorGalleryButton : MonoBehaviour
{
    
    private Button m_Button;

    private EditorSceneManager m_SceneManager;

#if UNITY_ANDROID && !UNITY_EDITOR
    void Awake()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(onGalleryButtonClicked);

        m_SceneManager = GameObject.FindWithTag("SceneManager").GetComponent<EditorSceneManager>();
    }

    void onGalleryButtonClicked()
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery(
            (path) => {
                if (path != null)
                {
                    m_SceneManager.LoadImageFromPath(path);
                }
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
