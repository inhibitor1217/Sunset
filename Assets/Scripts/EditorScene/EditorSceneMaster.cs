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
    public GameObject SLICLabelTexturePrefab;
    public GameObject BrushPrefab;
    public GameObject MaskCameraPrefab;

    [Header("References")]
    public RectTransform container;

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

    // Mask Components
    private GameObject[] m_MaskTextureObjects = new GameObject[MAX_EFFECTS];
    private MaskTexture[] m_MaskTextures = new MaskTexture[MAX_EFFECTS];
    private GameObject[] m_MaskCameraObjects = new GameObject[MAX_EFFECTS];
    private MaskRendererCamera[] m_MaskCameras = new MaskRendererCamera[MAX_EFFECTS];

    // Brush
    private GameObject m_BrushObject;
    private BrushController m_Brush;

    // SLIC
    private OpenCVSLICClient m_SLICClient;
    private GameObject m_SLICLabelTextureObject;
    private SLICLabelTexture m_SLICLabelTexture;

    public const int EFFECT_WATER = 0;
    public const int EFFECT_SKY = 1;
    public const int LAYER_WATER = 8;
    public const int LAYER_SKY = 9;
    public const int MASK_WATER = 0x100;
    public const int MASK_SKY = 0x200;
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
        if (InputMode.Instance.mode == InputMode.Mode.BUSY)
            return;

        // ** Clean up **

        // Managers
        if (m_TextureProviderManager)
            Destroy(m_TextureProviderManager);
        if (m_InputManager)
            Destroy(m_InputManager);
        
        // Root Layer
        if (m_RootLayerObject)
            Destroy(m_RootLayerObject);
        if (m_RootStaticTextureObject)
            Destroy(m_RootStaticTextureObject);

        // Mask Components
        for (int i = 0; i < MAX_EFFECTS; i++)
            RemoveMask(i);

        // Brush
        for (int i = 0; i < MAX_EFFECTS; i++)
            RemoveBrush(i);

        // SLIC
        RemoveSLIC();

        Debug.Log("Initialize Scene with texture [" + rootTexture.width + ", " + rootTexture.height + "]");

        // Set Root Texture width & height
        width = rootTexture.width;
        height = rootTexture.height;

        // Initialize Global Managers
        InputMode.Instance.mode = InputMode.Mode.MOVE;
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
        m_RootStaticTexture.SetStaticTexture(rootTexture);
        m_RootStaticTexture.SetTarget(m_RootLayer);

        // Initialize Mask Components
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

        m_MaskCameraObjects[maskIndex] = GameObject.Instantiate(MaskCameraPrefab);
        m_MaskCameraObjects[maskIndex].transform.SetParent(m_RootLayerObject.transform);
        m_MaskCameraObjects[maskIndex].transform.localPosition = Vector3.back;
        m_MaskCameras[maskIndex] = m_MaskCameraObjects[maskIndex].GetComponent<MaskRendererCamera>();

        Camera c = m_MaskCameraObjects[maskIndex].GetComponent<Camera>();
        switch (maskIndex)
        {
        case EFFECT_WATER:
            c.cullingMask = MASK_WATER;
            break;
        case EFFECT_SKY:
            c.cullingMask = MASK_SKY;
            break;
        }
        c.enabled = false;

        m_MaskTextures[maskIndex].SetCamera(c);
        m_RootLayer.SetMaskCamera(c, maskIndex);
    }

    public void RemoveMask(int maskIndex)
    {
        if (m_MaskTextureObjects[maskIndex])
            Destroy(m_MaskTextureObjects[maskIndex]);
        
        if (m_MaskCameraObjects[maskIndex])
            Destroy(m_MaskCameraObjects[maskIndex]);

        if (m_RootLayer)
            m_RootLayer.SetMaskCamera(null, maskIndex);

        m_MaskTextureObjects[maskIndex] = null;
        m_MaskTextures[maskIndex] = null;
        m_MaskCameraObjects[maskIndex] = null;
        m_MaskCameras[maskIndex] = null;
    }

    public void CreateBrush(int maskIndex)
    {
        // Create UI Brush
        m_BrushObject = GameObject.Instantiate(BrushPrefab);
        m_BrushObject.transform.SetParent(m_RootLayerObject.transform);
        m_Brush = m_BrushObject.GetComponent<BrushController>();

        switch (maskIndex)
        {
        case EFFECT_WATER:
            m_BrushObject.layer = LAYER_WATER;
            break;
        case EFFECT_SKY:
            m_BrushObject.layer = LAYER_SKY;
            break;
        }

        // Create Mask Layer
        m_MaskLayerObject = GameObject.Instantiate(MaskLayerPrefab);
        m_MaskLayerObject.GetComponent<RectTransform>().SetParent(m_RootLayerObject.GetComponent<RectTransform>());
        m_MaskLayerObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        m_MaskLayer = m_MaskLayerObject.GetComponent<RawImageController>();

        // Setup References
        m_MaskTextures[maskIndex].SetTarget(m_MaskLayer);

        // Activate Camera
        m_MaskCameraObjects[maskIndex].GetComponent<Camera>().enabled = true;
    }

    public void RemoveBrush(int maskIndex)
    {
        if (m_BrushObject)
            Destroy(m_BrushObject);
        if (m_MaskLayerObject)
            Destroy(m_MaskLayerObject);

        if (m_MaskTextures[maskIndex])
            m_MaskTextures[maskIndex].SetTarget(null);
        
        if (m_MaskCameraObjects[maskIndex])
            m_MaskCameraObjects[maskIndex].GetComponent<Camera>().enabled = false;

        m_BrushObject = null;
        m_Brush = null;
        m_MaskLayerObject = null;
        m_MaskLayer = null;
    }

    public void CreateSLIC(InputMode.Mode nextMode)
    {
        if (!m_SLICClient)
            m_SLICClient = gameObject.AddComponent(typeof(OpenCVSLICClient)) as OpenCVSLICClient;
        if (!m_SLICLabelTextureObject)
            m_SLICLabelTextureObject = GameObject.Instantiate(SLICLabelTexturePrefab);
        if (!m_SLICLabelTexture)
            m_SLICLabelTexture = m_SLICLabelTextureObject.GetComponent<SLICLabelTexture>();

        m_SLICClient.labelTextureProvider = m_SLICLabelTexture;
        m_SLICClient.Invoke(m_RootStaticTexture.GetTexture() as Texture2D, nextMode);
    }

    public void RemoveSLIC()
    {
        if (m_SLICClient)
            Destroy(m_SLICClient);
        if (m_SLICLabelTextureObject)
            Destroy(m_SLICLabelTextureObject);

        m_SLICClient = null;
        m_SLICLabelTextureObject = null;
        m_SLICLabelTexture = null;
    }

}