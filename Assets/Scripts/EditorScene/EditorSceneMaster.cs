using UnityEngine;

public class EditorSceneMaster : MonoBehaviour
{

    public static EditorSceneMaster instance { get; private set; }


    [Header("Prefabs")]
    public GameObject LayerPrefab;
    public GameObject StaticTexturePrefab;
    public GameObject MaskTexturePrefab;
    public GameObject SLICLabelTexturePrefab;
    public GameObject SLICContourTexturePrefab;
    public GameObject BrushPrefab;
    public GameObject FlowPrefab;
    public GameObject MaskCameraPrefab;
    public GameObject FlowCameraPrefab;

    [Header("References")]
    public RectTransform container;

    // Managers
    private TextureProviderManager m_TextureProviderManager;
    private InputManager m_InputManager;
    private WaterEffectManager m_WaterEffectManager;
    private Store m_Store;
    private static ActionModule[] actionModules = {
        SharedActions.instance,
        WaterEffectActions.instance,
    };

    // Root Info
    public int width { get; private set; }
    public int height { get; private set; }

    // Layers
    public GameObject rootLayerObject { get; private set; }
    public RawImageController rootLayer { get; private set; }
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
    private GameObject m_EnvMapTextureObject;
    private EnvironmentTexture m_EnvMapTexture;

    // Brush
    private GameObject m_BrushObject;
    private BrushController m_Brush;

    // Flow
    private FlowController m_FlowController;
    private GameObject m_FlowCameraObject;
    private Camera m_FlowCamera;
    private GameObject m_FlowLayerObject;
    private RawImageController m_FlowLayer;

    // SLIC
    private OpenCVSLICClient m_SLICClient;
    private GameObject m_SLICLabelTextureObject;
    private SLICLabelTexture m_SLICLabelTexture;

    // PCA
    private OpenCVPCAClient m_PCAClient;
    private GameObject[] m_PaletteTextureObjects = new GameObject[MAX_EFFECTS];
    private StaticTexture[] m_PaletteTextures = new StaticTexture[MAX_EFFECTS];

    // Effect
    private GameObject[] m_EffectLayerObjects = new GameObject[MAX_EFFECTS];
    private RawImageController[] m_EffectLayers = new RawImageController[MAX_EFFECTS];

    // Constants
    public const int EFFECT_WATER = 0;
    public const int EFFECT_SKY = 1;
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

        m_TextureProviderManager = GetComponent<TextureProviderManager>();
        m_InputManager = GetComponent<InputManager>();
        m_WaterEffectManager = GetComponent<WaterEffectManager>();
        m_Store = GetComponent<Store>();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    void Start()
    {
        InputMode.Instance.mode = InputMode.BUSY;
        MessagePanel.Instance.ShowMessage("이미지 불러오는 중..", "");
        InitScene(PlayerPrefs.GetString("image_path"));
    }
#endif

    public StaticTexture GetRootTextureProvider()
    {
        return m_RootStaticTexture;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    public void InitScene(string path)
    {
        Texture2D tex = NativeGallery.LoadImageAtPath(path);

        int width = 1;
        int height = 1;

        while (width < 2048)
        {
            if (2 * width < tex.width || 2 * width - tex.width < tex.width - width)
                width *= 2;
            else
                break;
        }
        while (height < 2048)
        {
            if (2 * height < tex.height || 2 * height - tex.height < tex.height - height)
                height *= 2;
            else
                break;
        }

        Texture2D resizedTex = new Texture2D(width, height);

        RenderTexture temp = RenderTexture.GetTemporary(
            width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear
        );
        Graphics.Blit(tex, temp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = temp;

        resizedTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        resizedTex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temp);

        InputMode.Instance.SetModeWithoutSideEffect(0);
        MessagePanel.Instance.Disable();

        InitScene(resizedTex);
    }
#endif

    public void InitScene(Texture2D rootTexture)
    {
        // ** Clean up **

        // Managers
        m_TextureProviderManager.Init();
        m_InputManager.Init();
        m_WaterEffectManager.Init();
        m_Store.Init(Store.MergeInitialStates(actionModules), Store.MergeReducers(actionModules));

        // Compute Clients
        if (m_SLICClient)
            Destroy(m_SLICClient);
        if (m_PCAClient)
            Destroy(m_PCAClient);
        
        // Root Layer
        if (rootLayerObject)
            Destroy(rootLayerObject);
        if (m_RootStaticTextureObject)
            Destroy(m_RootStaticTextureObject);

        // Mask Components
        RemoveMask(EFFECT_WATER);
        RemoveMask(EFFECT_SKY);

        // Brush
        RemoveBrush(EFFECT_WATER);
        RemoveBrush(EFFECT_SKY);

        // Flow
        RemoveFlow();
        if (m_FlowController)
            Destroy(m_FlowController);

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

        // Initialize Compute Clients
        m_SLICClient = gameObject.AddComponent(typeof(OpenCVSLICClient)) as OpenCVSLICClient;
        m_PCAClient  = gameObject.AddComponent(typeof(OpenCVPCAClient))  as OpenCVPCAClient;

        // Setup Root Layer
        rootLayerObject = GameObject.Instantiate(LayerPrefab);
        rootLayerObject.name = "Layer: Root";
        rootLayerObject.GetComponent<RectTransform>().SetParent(container);
        rootLayerObject.GetComponent<RectTransform>().SetAsFirstSibling();
        rootLayer = rootLayerObject.GetComponent<RawImageController>();
        rootLayer.movePosition = true;
        rootLayer.moveScale    = true;
        rootLayer.useGrid      = true;
        rootLayer.material     = new Material(Shader.Find("UI/Grid"));
        rootLayer.material.SetVector("_RootImageSize", new Vector4(1f / (float)width, 1f / (float)height, (float)width, (float)height));
        m_InputManager.image = rootLayerObject.GetComponent<RectTransform>();

        // Setup Root Texture Provider and Reference to Root Layer
        m_RootStaticTextureObject = GameObject.Instantiate(StaticTexturePrefab);
        m_RootStaticTextureObject.name = "Static Texture: Root";
        m_RootStaticTexture = m_RootStaticTextureObject.GetComponent<StaticTexture>();
        m_RootStaticTexture.SetStaticTexture(rootTexture);
        m_RootStaticTexture.SetTarget(rootLayer);

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
        m_MaskTextures[maskIndex].Setup(width / 2, height / 2);

        if (maskIndex == EFFECT_WATER)
        {
            m_EnvMapTexture = m_MaskTextureObjects[maskIndex].AddComponent<EnvironmentTexture>();
            m_EnvMapTexture.maskProvider = m_MaskTextures[maskIndex];
            m_EnvMapTexture.Setup(width / 2, height / 2);
        }

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
        m_MaskCameraObjects[maskIndex].transform.SetParent(rootLayerObject.transform);
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
        rootLayer.SetMaskCamera(c, maskIndex);
    }

    public void RemoveMask(int maskIndex)
    {
        if (m_MaskTextureObjects[maskIndex])
            Destroy(m_MaskTextureObjects[maskIndex]);
        
        if (m_MaskCameraObjects[maskIndex])
            Destroy(m_MaskCameraObjects[maskIndex]);


        if (rootLayer)
            rootLayer.SetMaskCamera(null, maskIndex);

        m_MaskTextureObjects[maskIndex] = null;
        m_MaskTextures[maskIndex] = null;
        m_MaskCameraObjects[maskIndex] = null;
        m_MaskCameras[maskIndex] = null;

        if (maskIndex == EFFECT_WATER)
        {
            m_EnvMapTexture = null;
        }
    }

    public MaskTexture GetMaskTexture(int maskIndex) { return m_MaskTextures[maskIndex]; }

    public void CreateBrush(int maskIndex)
    {
        // Create UI Brush
        if (!m_BrushObject)
        {
            m_BrushObject = GameObject.Instantiate(BrushPrefab);
            m_BrushObject.name = "Brush";
            m_BrushObject.transform.SetParent(rootLayerObject.transform);
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
            m_MaskLayerObject = GameObject.Instantiate(LayerPrefab);
            m_MaskLayerObject.name = "Layer: Mask";
            m_MaskLayerObject.GetComponent<RectTransform>().SetParent(rootLayerObject.GetComponent<RectTransform>());
            m_MaskLayerObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        if (!m_MaskLayer)
        {
            m_MaskLayer = m_MaskLayerObject.GetComponent<RawImageController>();
            m_MaskLayer.globalScale = 2f;
            m_MaskLayer.movePosition = false;
            m_MaskLayer.moveScale    = true;
            m_MaskLayer.material     = new Material(Shader.Find("UI/Mask"));
            m_MaskLayer.material.SetColor("_Color", new Color(1, 0, 0, .3f));
        }

        // Setup References
        m_MaskTextures[maskIndex].SetTarget(m_MaskLayer);

        // Activate Camera
        m_MaskCameraObjects[maskIndex].GetComponent<Camera>().enabled = true;

        // Disable All Effects
        if (m_EffectLayerObjects[EFFECT_WATER])
            m_EffectLayerObjects[EFFECT_WATER].SetActive(false);
        if (m_EffectLayerObjects[EFFECT_SKY])
            m_EffectLayerObjects[EFFECT_SKY].SetActive(false);
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

        // Enable All Effects
        if (m_EffectLayerObjects[EFFECT_WATER])
            m_EffectLayerObjects[EFFECT_WATER].SetActive(true);
        if (m_EffectLayerObjects[EFFECT_SKY])
            m_EffectLayerObjects[EFFECT_SKY].SetActive(true);
    }

    public void CreateFlow()
    {
        if (!m_FlowController)
        {
            m_FlowController = gameObject.AddComponent<FlowController>();
            m_FlowController.FlowPrefab = FlowPrefab;
        }
        if (!m_MaskLayerObject)
        {
            m_MaskLayerObject = GameObject.Instantiate(LayerPrefab);
            m_MaskLayerObject.name = "Layer: Mask";
            m_MaskLayerObject.GetComponent<RectTransform>().SetParent(rootLayerObject.GetComponent<RectTransform>());
            m_MaskLayerObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        if (!m_MaskLayer)
        {
            m_MaskLayer = m_MaskLayerObject.GetComponent<RawImageController>();
            m_MaskLayer.globalScale = 2f;
            m_MaskLayer.movePosition = false;
            m_MaskLayer.moveScale    = true;
            m_MaskLayer.material     = new Material(Shader.Find("UI/Mask"));
            m_MaskLayer.material.SetColor("_Color", new Color(54f/255f, 81f/255f, 91f/255f, .9f));
        }
        if (!m_FlowCameraObject)
        {
            m_FlowCameraObject = GameObject.Instantiate(FlowCameraPrefab);
            m_FlowCameraObject.name = "Flow Camera";
            m_FlowCameraObject.transform.SetParent(container);
            m_FlowCameraObject.transform.localPosition = Vector3.back;
        }
        if (!m_FlowCamera)
        {
            m_FlowCamera = m_FlowCameraObject.GetComponent<Camera>();
            m_FlowCamera.aspect = container.rect.width / container.rect.height;
            m_FlowCamera.orthographicSize = .5f * container.rect.height;
        }
        if (!m_FlowLayerObject)
        {
            m_FlowLayerObject = GameObject.Instantiate(LayerPrefab);
            m_FlowLayerObject.name = "Layer: Flow";

            RectTransform rt = m_FlowLayerObject.GetComponent<RectTransform>();
            rt.SetParent(container);
            rt.anchoredPosition = Vector3.zero;
        }
        if (!m_FlowLayer)
        {
            m_FlowLayer = m_FlowLayerObject.GetComponent<RawImageController>();
            m_FlowLayer.movePosition = false;
            m_FlowLayer.moveScale    = false;
            m_FlowLayer.useGrid      = false;
            m_FlowLayer.material     = new Material(Shader.Find("UI/Grid"));
        }

        RenderTexture flowTex = new RenderTexture((int)container.rect.width, (int)container.rect.height, 0, RenderTextureFormat.ARGB32);

        // Setup References
        m_MaskTextures[EFFECT_WATER].SetTarget(m_MaskLayer);
        m_FlowCamera.targetTexture = flowTex;
        m_FlowLayer.SetTexture(flowTex);

        // Disable All Effects
        if (m_EffectLayerObjects[EFFECT_WATER])
            m_EffectLayerObjects[EFFECT_WATER].SetActive(false);
        if (m_EffectLayerObjects[EFFECT_SKY])
            m_EffectLayerObjects[EFFECT_SKY].SetActive(false);
    }

    public void RemoveFlow()
    {
        if (m_FlowCamera)
            m_FlowCamera.targetTexture.Release();

        if (m_MaskLayerObject)
            Destroy(m_MaskLayerObject);
        if (m_FlowCameraObject)
            Destroy(m_FlowCameraObject);
        if (m_FlowLayerObject)
            Destroy(m_FlowLayerObject);

        m_MaskLayerObject  = null;
        m_MaskLayer        = null;
        m_FlowCameraObject = null;
        m_FlowCamera       = null;
        m_FlowLayerObject  = null;
        m_FlowLayer        = null;

        // Invoke Flow Texture Draw
        if (m_FlowController)
            m_WaterEffectManager.CreateFlow(m_FlowController);

        // Enable All Effects
        if (m_EffectLayerObjects[EFFECT_WATER])
            m_EffectLayerObjects[EFFECT_WATER].SetActive(true);
        if (m_EffectLayerObjects[EFFECT_SKY])
            m_EffectLayerObjects[EFFECT_SKY].SetActive(true);
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

    public void Effect_CL01() { CreateEffect(EFFECT_WATER, WaterEffectManager.CL01); }
    public void Effect_CL02() { CreateEffect(EFFECT_WATER, WaterEffectManager.CL02); }
    public void Effect_RV01() { CreateEffect(EFFECT_WATER, WaterEffectManager.RV01); }

    public void CreateEffect(int maskIndex, int effectType)
    {
        if (!m_EffectLayerObjects[maskIndex])
        {
            m_EffectLayerObjects[maskIndex] = GameObject.Instantiate(LayerPrefab);
            m_EffectLayerObjects[maskIndex].name = "Layer: Effect " + maskIndexToString(maskIndex);
            m_EffectLayerObjects[maskIndex].GetComponent<RectTransform>().SetParent(rootLayerObject.GetComponent<RectTransform>());
            m_EffectLayerObjects[maskIndex].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        if (!m_EffectLayers[maskIndex])
        {
            m_EffectLayers[maskIndex] = m_EffectLayerObjects[maskIndex].GetComponent<RawImageController>();
            m_EffectLayers[maskIndex].movePosition = false;
            m_EffectLayers[maskIndex].moveScale    = true;
            m_EffectLayers[maskIndex].useGrid      = true;
            m_EffectLayers[maskIndex].globalScale  = 2f;
            m_EffectLayers[maskIndex].material     = new Material(Shader.Find("UI/Grid"));
            m_EffectLayers[maskIndex].material.SetVector("_RootImageSize", new Vector4(1f / (float)width, 1f / (float)height, (float)width, (float)height));
        }

        if (maskIndex == EFFECT_WATER)
        {
            m_WaterEffectManager.paletteProvider     = m_PaletteTextures[maskIndex];
            m_WaterEffectManager.environmentProvider = m_EnvMapTexture;
            m_WaterEffectManager.target              = m_EffectLayers[maskIndex];
            m_WaterEffectManager.Setup(effectType, width / 2, height / 2);
        }
    }

    public void RemoveEffect(int maskIndex)
    {
        if (m_EffectLayerObjects[maskIndex])
            Destroy(m_EffectLayerObjects[maskIndex]);

        m_EffectLayerObjects[maskIndex] = null;
        m_EffectLayers[maskIndex] = null;

        if (maskIndex == EFFECT_WATER)
        {
            m_WaterEffectManager.Setup(WaterEffectManager.NONE, 0, 0);
        }
    }

}