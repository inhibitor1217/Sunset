using UnityEngine;

public class EditorSceneMaster : MonoBehaviour
{

    private static EditorSceneMaster instance;
    public static EditorSceneMaster Instance { get { return instance; } }

    [Header("Prefabs")]
    public GameObject LayerPrefab;
    public GameObject MaskLayerPrefab;
    public GameObject StaticTexturePrefab;
    public GameObject MaskTexturePrefab;
    public GameObject EffectTexturePrefab;
    public GameObject SLICLabelTexturePrefab;
    public GameObject SLICContourTexturePrefab;
    public GameObject FractalNoiseRuntimeTexturePrefab;
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
    private BlurTexture[] m_BlurredMaskTextures = new BlurTexture[MAX_EFFECTS];
    private GameObject[] m_MaskCameraObjects = new GameObject[MAX_EFFECTS];
    private MaskRendererCamera[] m_MaskCameras = new MaskRendererCamera[MAX_EFFECTS];

    // Brush
    private GameObject m_BrushObject;
    private BrushController m_Brush;

    // SLIC
    private OpenCVSLICClient m_SLICClient;
    private GameObject m_SLICLabelTextureObject;
    private SLICLabelTexture m_SLICLabelTexture;

    // PCA
    private OpenCVPCAClient m_PCAClient;
    private GameObject[] m_PaletteTextureObjects = new GameObject[MAX_EFFECTS];
    private StaticTexture[] m_PaletteTextures = new StaticTexture[MAX_EFFECTS];

    // Effect
    private GameObject m_FractalNoiseRuntimeTextureObject;
    private FractalNoiseRuntimeTexture m_FractalNoiseRuntimeTexture;
    private GameObject[] m_EffectTextureObjects = new GameObject[MAX_EFFECTS];
    private EffectTexture[] m_EffectTextures = new EffectTexture[MAX_EFFECTS];
    private GameObject[] m_EffectLayerObjects = new GameObject[MAX_EFFECTS];
    private RawImageController[] m_EffectLayers = new RawImageController[MAX_EFFECTS];

    public const int EFFECT_WATER = 0;
    public const int EFFECT_SKY = 1;
    public const int WATER_TYPE_CALM = 0;
    public const int LAYER_WATER = 8;
    public const int LAYER_SKY = 9;
    public const int MASK_WATER = 0x100;
    public const int MASK_SKY = 0x200;
    public const int MAX_EFFECTS = 2;
    public static string maskIndexToString(int maskIndex)
    {
        switch (maskIndex)
        {
        case EFFECT_WATER:
            return "WATER";
        case EFFECT_SKY:
            return "SKY";
        default:
            return "";
        }
    }

    void Awake()
    {
        instance = this;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    void Start()
    {
        InitScene(PlayerPrefs.GetString("image_path"));
    }
#endif

    public Rect GetRootRect()
    {
        return m_RootLayer.GetRect();
    }

    public Vector2 RelativeCoordsToRootRect(Vector2 pos)
    {
        return m_RootLayer.RelativeCoords(pos);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    public void InitScene(string path)
    {
        Texture2D tex = NativeGallery.LoadImageAtPath(path);
        
        if (tex.width <= 2048 && tex.height <= 2048)
            InitScene(tex);
        else
        {
            Texture2D resizedTex;
            if (tex.width > tex.height)
                resizedTex = new Texture2D(2048, Mathf.FloorToInt((float)tex.height * (2048f / (float)tex.width)));
            else
                resizedTex = new Texture2D(Mathf.FloorToInt((float)tex.width * (2048f / (float)tex.height)), 2048);

            RenderTexture temp = RenderTexture.GetTemporary(
                resizedTex.width, resizedTex.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear
            );
            Graphics.Blit(tex, temp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = temp;

            resizedTex.ReadPixels(new Rect(0, 0, resizedTex.width, resizedTex.height), 0, 0);
            resizedTex.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temp);

            InitScene(resizedTex);
        }
    }
#endif

    public void InitScene(Texture2D rootTexture)
    {
        if (InputMode.Instance.isBusy())
            return;

        // ** Clean up **

        // Managers
        if (m_TextureProviderManager)
            Destroy(m_TextureProviderManager);
        if (m_InputManager)
            Destroy(m_InputManager);

        // Compute Clients
        if (m_SLICClient)
            Destroy(m_SLICClient);
        if (m_PCAClient)
            Destroy(m_PCAClient);
        
        // Root Layer
        if (m_RootLayerObject)
            Destroy(m_RootLayerObject);
        if (m_RootStaticTextureObject)
            Destroy(m_RootStaticTextureObject);

        // Mask Components
        RemoveMask(EFFECT_WATER);
        RemoveMask(EFFECT_SKY);

        // Brush
        RemoveBrush(EFFECT_WATER);
        RemoveBrush(EFFECT_SKY);

        // SLIC
        RemoveSLIC();

        // PCA
        RemovePCA(EFFECT_WATER);
        RemovePCA(EFFECT_SKY);

        // Effect
        RemoveEffect(EFFECT_WATER);
        RemoveEffect(EFFECT_SKY);

        Debug.Log("Initialize Scene with texture [" + rootTexture.width + ", " + rootTexture.height + "]");

        // Set Root Texture width & height
        width = rootTexture.width;
        height = rootTexture.height;

        // Initialize Global Managers
        m_TextureProviderManager = gameObject.AddComponent(typeof(TextureProviderManager)) as TextureProviderManager;
        m_InputManager = gameObject.AddComponent(typeof(InputManager)) as InputManager;
        m_InputManager.container = container;

        // Initialize Compute Clients
        m_SLICClient = gameObject.AddComponent(typeof(OpenCVSLICClient)) as OpenCVSLICClient;
        m_PCAClient  = gameObject.AddComponent(typeof(OpenCVPCAClient))  as OpenCVPCAClient;

        // Setup Root Layer
        m_RootLayerObject = GameObject.Instantiate(LayerPrefab);
        m_RootLayerObject.name = "Layer: Root";
        m_RootLayerObject.GetComponent<RectTransform>().SetParent(container);
        m_RootLayerObject.GetComponent<RectTransform>().SetAsFirstSibling();
        m_RootLayer = m_RootLayerObject.GetComponent<RawImageController>();
        m_RootLayer.isRoot = true;

        // Setup Root Texture Provider and Reference to Root Layer
        m_RootStaticTextureObject = GameObject.Instantiate(StaticTexturePrefab);
        m_RootStaticTextureObject.name = "Static Texture: Root";
        m_RootStaticTexture = m_RootStaticTextureObject.GetComponent<StaticTexture>();
        m_RootStaticTexture.SetStaticTexture(rootTexture);
        m_RootStaticTexture.SetTarget(m_RootLayer);

        // Initialize Mask Components
        CreateMask(EFFECT_WATER);
        CreateMask(EFFECT_SKY);

        // Initialize SLIC and Invoke
        CreateSLIC();
        InvokeSLIC(InputMode.MOVE);

        // Initialize PCAs
        CreatePCA(EFFECT_WATER);
        CreatePCA(EFFECT_SKY);

        // Update All Texture Providers in the Render Chain
        TextureProviderManager.UpdateEager();
    }

    public void CreateMask(int maskIndex)
    {
        RemoveMask(maskIndex);

        m_MaskTextureObjects[maskIndex] = GameObject.Instantiate(MaskTexturePrefab);
        m_MaskTextureObjects[maskIndex].name = "Mask Texture: " + maskIndexToString(maskIndex);
        m_MaskTextures[maskIndex] = m_MaskTextureObjects[maskIndex].GetComponent<MaskTexture>();
        m_MaskTextures[maskIndex].Setup(width, height);
        m_BlurredMaskTextures[maskIndex] = m_MaskTextureObjects[maskIndex].GetComponent<BlurTexture>();
        m_BlurredMaskTextures[maskIndex].sourceTexture = m_MaskTextures[maskIndex];
        m_BlurredMaskTextures[maskIndex].Setup();
        switch (maskIndex)
        {
        case EFFECT_WATER:
            m_MaskTextures[maskIndex].mode = InputMode.WATER;
            break;
        case EFFECT_SKY:
            m_MaskTextures[maskIndex].mode = InputMode.SKY;
            break;
        default:
            break;
        }

        m_MaskCameraObjects[maskIndex] = GameObject.Instantiate(MaskCameraPrefab);
        m_MaskCameraObjects[maskIndex].name = "Mask Camera: " + maskIndexToString(maskIndex);
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

    public bool isMaskDirty(int maskIndex) { return m_MaskTextures[maskIndex] ? m_MaskTextures[maskIndex].dirty : false; }
    public void setMaskDirty(int maskIndex, bool value) { if (m_MaskTextures[maskIndex]) m_MaskTextures[maskIndex].dirty = value; }

    public void CreateBrush(int maskIndex)
    {
        // Create UI Brush
        if (!m_BrushObject)
        {
            m_BrushObject = GameObject.Instantiate(BrushPrefab);
            m_BrushObject.name = "Brush";
            m_BrushObject.transform.SetParent(m_RootLayerObject.transform);
        }
        if (!m_Brush)
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
        if (!m_MaskLayerObject)
        {
            m_MaskLayerObject = GameObject.Instantiate(MaskLayerPrefab);
            m_MaskLayerObject.name = "Layer: Mask";
            m_MaskLayerObject.GetComponent<RectTransform>().SetParent(m_RootLayerObject.GetComponent<RectTransform>());
            m_MaskLayerObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        if (!m_MaskLayer)
        {
            m_MaskLayer = m_MaskLayerObject.GetComponent<RawImageController>();
        }

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

    public void CreateSLIC()
    {
        if (!m_SLICLabelTextureObject)
        {
            m_SLICLabelTextureObject = GameObject.Instantiate(SLICLabelTexturePrefab);
            m_SLICLabelTextureObject.name = "SLIC Label Texture";
        }
        if (!m_SLICLabelTexture)
            m_SLICLabelTexture = m_SLICLabelTextureObject.GetComponent<SLICLabelTexture>();

        // Setup References
        m_MaskCameras[EFFECT_WATER].labelTexture = m_SLICLabelTexture;
        m_MaskCameras[EFFECT_SKY]  .labelTexture = m_SLICLabelTexture;
    }

    public void InvokeSLIC(int nextMode)
    {
        m_SLICClient.labelTextureProvider = m_SLICLabelTexture;
        m_SLICClient.Invoke(m_RootStaticTexture, nextMode);
    }

    public void RemoveSLIC()
    {
        if (m_SLICLabelTextureObject)
            Destroy(m_SLICLabelTextureObject);

        m_SLICClient = null;
        m_SLICLabelTextureObject = null;
        m_SLICLabelTexture = null;

        if (m_MaskCameras[EFFECT_WATER])
            m_MaskCameras[EFFECT_WATER].labelTexture = null;
        if (m_MaskCameras[EFFECT_SKY])
            m_MaskCameras[EFFECT_SKY].labelTexture = null;
    }

    public void CreatePCA(int maskIndex)
    {
        if (!m_PaletteTextureObjects[maskIndex])
        {
            m_PaletteTextureObjects[maskIndex] = GameObject.Instantiate(StaticTexturePrefab);
            m_PaletteTextureObjects[maskIndex].name = "Static Texture: PCAPalette " + maskIndexToString(maskIndex);
        }
        if (!m_PaletteTextures[maskIndex])
            m_PaletteTextures[maskIndex] = m_PaletteTextureObjects[maskIndex].GetComponent<StaticTexture>();
    }

    public void InvokePCA(int maskIndex, int nextMode)
    {
        if (m_PCAClient && m_PaletteTextures[maskIndex])
        {
            m_PCAClient.paletteTextureProvider  = m_PaletteTextures[maskIndex];
            m_PCAClient.Invoke(
                m_RootStaticTexture,
                m_MaskTextures[maskIndex],
                m_SLICLabelTexture,
                nextMode
            );
        }
    }

    public void RemovePCA(int maskIndex)
    {
        if (m_PaletteTextureObjects[maskIndex])
            Destroy(m_PaletteTextureObjects[maskIndex]);
        
        m_PaletteTextureObjects[maskIndex] = null;
        m_PaletteTextures[maskIndex] = null;
    }

    public void Calm() { CreateEffect(EFFECT_WATER, WATER_TYPE_CALM); }

    public void CreateEffect(int maskIndex, int effectType)
    {
        // Initialize Noise
        if (!m_FractalNoiseRuntimeTextureObject)
        {
            m_FractalNoiseRuntimeTextureObject = GameObject.Instantiate(FractalNoiseRuntimeTexturePrefab);
            m_FractalNoiseRuntimeTextureObject.name = "Fractal Noise Runtime Texture";
        }
        if (!m_FractalNoiseRuntimeTexture)
        {
            m_FractalNoiseRuntimeTexture = m_FractalNoiseRuntimeTextureObject.GetComponent<FractalNoiseRuntimeTexture>();
        }

        if (!m_EffectTextureObjects[maskIndex])
        {
            m_EffectTextureObjects[maskIndex] = GameObject.Instantiate(EffectTexturePrefab);
            m_EffectTextureObjects[maskIndex].name = "Effect Texture: " + maskIndexToString(maskIndex);
        }
        if (!m_EffectTextures[maskIndex])
            m_EffectTextures[maskIndex] = m_EffectTextureObjects[maskIndex].GetComponent<EffectTexture>();
        if (!m_EffectLayerObjects[maskIndex])
        {
            m_EffectLayerObjects[maskIndex] = GameObject.Instantiate(LayerPrefab);
            m_EffectLayerObjects[maskIndex].name = "Layer: Effect " + maskIndexToString(maskIndex);
            m_EffectLayerObjects[maskIndex].GetComponent<RectTransform>().SetParent(m_RootLayerObject.GetComponent<RectTransform>());
            m_EffectLayerObjects[maskIndex].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        if (!m_EffectLayers[maskIndex])
        {
            m_EffectLayers[maskIndex] = m_EffectLayerObjects[maskIndex].GetComponent<RawImageController>();
        }

        if (maskIndex == EFFECT_WATER)
        {
            switch (effectType)
            {
            case WATER_TYPE_CALM:
                m_FractalNoiseRuntimeTexture.noiseType = 4;
                m_FractalNoiseRuntimeTexture.fractalType = 0;
                m_FractalNoiseRuntimeTexture.scale = new Vector2(32, 128);
                m_FractalNoiseRuntimeTexture.brightness = -.3f;
                m_FractalNoiseRuntimeTexture.contrast = 2f;

                m_EffectTextures[maskIndex].noiseTexture = m_FractalNoiseRuntimeTexture;
                m_EffectTextures[maskIndex].paletteTexture = m_PaletteTextures[maskIndex];
                m_EffectTextures[maskIndex].maskTexture = m_BlurredMaskTextures[maskIndex];
                m_EffectTextures[maskIndex].Setup(width, height);
                break;
            default:
                RemoveEffect(maskIndex);
                break;
            }
        }

        m_EffectTextures[maskIndex].SetTarget(m_EffectLayers[maskIndex]);
    }

    public void RemoveEffect(int maskIndex)
    {
        if (m_EffectTextureObjects[maskIndex])
            Destroy(m_EffectTextureObjects[maskIndex]);
        if (m_EffectLayerObjects[maskIndex])
            Destroy(m_EffectLayerObjects[maskIndex]);

        m_EffectTextureObjects[maskIndex] = null;
        m_EffectTextures[maskIndex] = null;
        m_EffectLayerObjects[maskIndex] = null;
        m_EffectLayers[maskIndex] = null;
    }

}