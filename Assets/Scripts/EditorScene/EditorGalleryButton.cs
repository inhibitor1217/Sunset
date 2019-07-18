using UnityEngine;
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
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery(
            (path) => {
                EditorSceneManager.Instance.InitScene(path);
            }
        );
    }
#else
    void onClick()
    {
        if (initTexture)
            EditorSceneManager.Instance.InitScene(initTexture);
    }
#endif

}
