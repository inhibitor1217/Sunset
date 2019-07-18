using UnityEngine;
using UnityEngine.UI;

public class EditorSceneMaster : MonoBehaviour
{

    private static EditorSceneMaster instance;
    public static EditorSceneMaster Instance { get { return instance; } }

    [Header("Prefabs")]
    public GameObject LayerPrefab;
    public GameObject MaskLayerPrefab;
    public GameObject StaticTexturePrefab;
    public GameObject MaskTexturePrefab;
    public GameObject BrushPrefab;
    public GameObject MaskCameraPrefab;

    [Header("References")]
    public RectTransform container;
    public GameObject[] UIButtonObjects;
    public GameObject[] UIToggleObjects;

    // Managers
    private TextureProviderManager m_TextureProviderManager;
    private InputManager m_InputManager;

    // Root Info
    private int width;
    private int height;

    // Layers
    private GameObject m_RootLayerObject;
    private RawImageController m_RootLayer;
    private GameObject m_MaskLayerObject;
    private RawImageController m_MaskLayer;

    // Texture Providers
    private GameObject m_RootStaticTextureObject;
    private StaticTexture m_RootStaticTexture;
    private GameObject[] m_MaskTextureObjects = {};
    private MaskTexture[] m_MaskTextures = {};

    // Brush & Mask
    private GameObject m_BrushObject;
    private BrushController m_Brush;
    private GameObject m_MaskCameraObject;
    private MaskRendererCamera m_MaskCamera;

    public const int EFFECT_WATER = 0;
    public const int EFFECT_SKY = 1;
    public const int MAX_EFFECTS = 2;

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
        // ** Clean up **

        // Managers
        if (m_TextureProviderManager)
            Destroy(m_TextureProviderManager);
        if (m_InputManager)
            Destroy(m_InputManager);
        
        // Layers
        if (m_RootLayerObject)
            Destroy(m_RootLayerObject);
        if (m_MaskLayerObject)
            Destroy(m_MaskLayerObject);

        // Texture Providers
        if (m_RootStaticTextureObject)
            Destroy(m_RootStaticTextureObject);
        foreach (var maskTextureObject in m_MaskTextureObjects)
        {
            if (maskTextureObject)
                Destroy(maskTextureObject);
        }
        m_MaskTextureObjects = new GameObject[MAX_EFFECTS];
        m_MaskTextures = new MaskTexture[MAX_EFFECTS];

        // Brush & Mask
        if (m_BrushObject)
            Destroy(m_BrushObject);
        if (m_MaskCameraObject)
            Destroy(m_MaskCameraObject);

        Debug.Log("Initialize Scene with texture [" + rootTexture.width + ", " + rootTexture.height + "]");

        // Set Root Texture width & height
        width = rootTexture.width;
        height = rootTexture.height;

        // Enable Functional Buttons
        foreach (var button in UIButtonObjects)
            button.GetComponent<Button>().interactable = true;
        foreach (var toggle in UIToggleObjects)
            toggle.GetComponent<Toggle>().interactable = true;

        // Initialize Global Managers
        InputMode.Instance.mode = InputMode.MOVE;
        m_TextureProviderManager = gameObject.AddComponent(typeof(TextureProviderManager)) as TextureProviderManager;
        m_InputManager = gameObject.AddComponent(typeof(InputManager)) as InputManager;
        m_InputManager.container = container;

        // Initialize UI Components
        foreach (var toggle in UIToggleObjects)
            toggle.GetComponent<InputModeToggle>().UpdateColor(false);

        // Setup Root Layer
        m_RootLayerObject = GameObject.Instantiate(LayerPrefab);
        m_RootLayerObject.GetComponent<RectTransform>().SetParent(container);
        m_RootLayerObject.GetComponent<RectTransform>().SetAsFirstSibling();
        m_RootLayer = m_RootLayerObject.GetComponent<RawImageController>();
        m_RootLayer.isRoot = true;

        // Setup Root Texture Provider and Reference to Root Layer
        m_RootStaticTextureObject = GameObject.Instantiate(StaticTexturePrefab);
        m_RootStaticTexture = m_RootStaticTextureObject.GetComponent<StaticTexture>();
        m_RootStaticTexture.SetStaticTexture(rootTexture);
        m_RootStaticTexture.SetTarget(m_RootLayer);

        // TEMPORARY : Just Create Mask Texture
        CreateMask(EFFECT_WATER);
        CreateMask(EFFECT_SKY);

        // Update All Texture Providers in the Render Chain
        TextureProviderManager.UpdateEager();
    }

    public void CreateMask(int maskIndex)
    {
        RemoveMask(maskIndex);

        m_MaskTextureObjects[maskIndex] = GameObject.Instantiate(MaskTexturePrefab);
        m_MaskTextures[maskIndex] = m_MaskTextureObjects[maskIndex].GetComponent<MaskTexture>();
        m_MaskTextures[maskIndex].SetDimension(width, height);
    }

    public void RemoveMask(int maskIndex)
    {
        if (m_MaskTextureObjects[maskIndex])
            Destroy(m_MaskTextureObjects[maskIndex]);
    }

    public void CreateBrush(int maskIndex)
    {
        // Create UI Brush
        m_BrushObject = GameObject.Instantiate(BrushPrefab);
        m_BrushObject.transform.SetParent(m_RootLayerObject.transform);
        m_Brush = m_BrushObject.GetComponent<BrushController>();
        m_Brush.container = container;

        // Create Mask Camera
        m_MaskCameraObject = GameObject.Instantiate(MaskCameraPrefab);
        m_MaskCameraObject.transform.SetParent(m_RootLayerObject.transform);
        m_MaskCameraObject.transform.localPosition = Vector3.zero;
        m_MaskCamera = m_MaskCameraObject.GetComponent<MaskRendererCamera>();

        // Create Mask Layer
        m_MaskLayerObject = GameObject.Instantiate(MaskLayerPrefab);
        m_MaskLayerObject.GetComponent<RectTransform>().SetParent(m_RootLayerObject.GetComponent<RectTransform>());
        m_MaskLayerObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        m_MaskLayer = m_MaskLayerObject.GetComponent<RawImageController>();

        // Setup References
        m_MaskTextures[maskIndex].SetCamera(m_MaskCameraObject.GetComponent<Camera>());
        m_MaskTextures[maskIndex].SetTarget(m_MaskLayer);
        
        m_RootLayer.maskCamera = m_MaskCameraObject.GetComponent<Camera>();
    }

    public void RemoveBrush()
    {
        if (m_BrushObject)
            Destroy(m_BrushObject);
        if (m_MaskCameraObject)
            Destroy(m_MaskCameraObject);
        if (m_MaskLayerObject)
            Destroy(m_MaskLayerObject);

        m_RootLayer.maskCamera = null;
    }

}