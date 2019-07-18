using UnityEngine;

public class EditorSceneManager : MonoBehaviour
{

    private static EditorSceneManager instance;
    public static EditorSceneManager Instance { get { return instance; } }

    [Header("Prefabs")]
    public GameObject LayerPrefab;
    public GameObject StaticTexturePrefab;
    public GameObject GalleryTexturePrefab;

    [Header("References")]
    public RectTransform container;

    // Managers
    private TextureProviderManager m_TextureProviderManager;
    private InputManager m_InputManager;

    // Layers
    private GameObject m_RootLayerObject;
    private RawImageController m_RootLayer;

    // Texture Providers
    private GameObject m_RootStaticTextureObject;
    private StaticTexture m_RootStaticTexture;

    void Awake()
    {
        instance = this;
#if UNITY_ANDROID && !UNITY_EDITOR
        InitScene(PlayerPrefs.GetString("image_path"));
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    public void InitScene(string path)
    {
        InitScene(NativeGallery.LoadImageAtPath(path));
    }
#endif

    public void InitScene(Texture2D rootTexture)
    {
        // Clean up
        if (m_TextureProviderManager)
            Destroy(m_TextureProviderManager);

        if (m_InputManager)
            Destroy(m_InputManager);
        
        if (m_RootLayerObject)
            Destroy(m_RootLayerObject);

        if (m_RootStaticTextureObject)
            Destroy(m_RootStaticTextureObject);


        Debug.Log("Initialize Scene with texture [" + rootTexture.width + ", " + rootTexture.height + "]");

        // Initialize Global Managers
        InputMode.Instance.mode = InputMode.MOVE;
        m_TextureProviderManager = gameObject.AddComponent(typeof(TextureProviderManager)) as TextureProviderManager;
        m_InputManager = gameObject.AddComponent(typeof(InputManager)) as InputManager;
        m_InputManager.container = container;

        // Setup Root Layer
        m_RootLayerObject = GameObject.Instantiate(LayerPrefab);
        m_RootLayerObject.GetComponent<RectTransform>().SetParent(container);
        m_RootLayerObject.GetComponent<RectTransform>().SetAsFirstSibling();
        m_RootLayer = m_RootLayerObject.GetComponent<RawImageController>();
        m_RootLayer.isRoot = true;

        // Setup Root Texture Provider and Reference to Root Layer
        m_RootStaticTextureObject = GameObject.Instantiate(StaticTexturePrefab);
        m_RootStaticTexture = m_RootStaticTextureObject.GetComponent<StaticTexture>();
        m_RootLayer.provider = m_RootStaticTexture;
        m_RootStaticTexture.SetTarget(m_RootLayer);
        m_RootStaticTexture.SetStaticTexture(rootTexture);

        // Update All Texture Providers in the Render Chain
        TextureProviderManager.UpdateEager();
    }

}