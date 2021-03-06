﻿using UnityEngine;
using UnityEngine.UI;

public class EditorGalleryButton : MonoBehaviour
{
    
    private Button m_Button;

#if UNITY_EDITOR
    public Texture2D initTexture;
#endif

    void Awake()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(onClick);
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    void onClick()
    {
        if (InputMode.instance.isBusy())
            return;
            
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery(
            (path) => {
                EditorSceneMaster.instance.InitScene(path);
            }
        );
    }
#else
    void onClick()
    {
        if (InputMode.instance.isBusy())
            return;

        if (initTexture)
            EditorSceneMaster.instance.InitScene(initTexture);
    }
#endif

}
