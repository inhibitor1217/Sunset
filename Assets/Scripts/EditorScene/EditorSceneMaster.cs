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
    private GameObject m_MaskTextureObject;
    private MaskTexture m_MaskTexture;
    private GameObject m_MaskCameraObject;
    private MaskRendererCamera m_MaskCamera;
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
    private GameObject m_PaletteTextureObject;
    private StaticTexture m_PaletteTexture;

    // Effect
    private GameObject m_EffectLayerObject;
    private RawImageController m_EffectLayer;

    // Constants
    public const int LAYER_WATER = 8;
    public const int MASK_WATER = 0x100;

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
        RemoveMask();

        // Brush
        RemoveBrush();

        // Flow
        RemoveFlow();
        if (m_FlowController)
            Destroy(m_FlowController);

        // SLIC
        RemoveSLIC();

        // PCA
        RemovePCA();

        // Effect
        RemoveEffect();

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
        CreateMask();

        // Initialize SLIC and Invoke
        CreateSLIC();
        InvokeSLIC(InputMode.MOVE);

        // Initialize PCAs
        CreatePCA();

        // Update All Texture Providers in the Render Chain
        TextureProviderManager.UpdateEager();
    }

    public void CreateMask()
    {
        RemoveMask();

        m_MaskTextureObject = GameObject.Instantiate(MaskTexturePrefab);
        m_MaskTextureObject.name = "Mask Texture";
        m_MaskTexture = m_MaskTextureObject.GetComponent<MaskTexture>();
        m_MaskTexture.Setup(width / 2, height / 2);

        m_EnvMapTexture = m_MaskTextureObject.AddComponent<EnvironmentTexture>();
        m_EnvMapTexture.maskProvider = m_MaskTexture;
        m_EnvMapTexture.Setup(width / 2, height / 2);

        m_MaskCameraObject = GameObject.Instantiate(MaskCameraPrefab);
        m_MaskCameraObject.name = "Mask Camera";
        m_MaskCameraObject.transform.SetParent(rootLayerObject.transform);
        m_MaskCameraObject.transform.localPosition = Vector3.back;
        m_MaskCamera = m_MaskCameraObject.GetComponent<MaskRendererCamera>();

        Camera c = m_MaskCameraObject.GetComponent<Camera>();
        c.cullingMask = MASK_WATER;
        c.enabled = false;

        m_MaskTexture.SetCamera(c);
        rootLayer.SetMaskCamera(c);
    }

    public void RemoveMask()
    {
        if (m_MaskTextureObject)
            Destroy(m_MaskTextureObject);
        
        if (m_MaskCameraObject)
            Destroy(m_MaskCameraObject);


        if (rootLayer)
            rootLayer.SetMaskCamera(null);

        m_MaskTextureObject = null;
        m_MaskTexture = null;
        m_MaskCameraObject = null;

        m_EnvMapTexture = null;
    }

    public MaskTexture GetMaskTexture() { return m_MaskTexture; }

    public void CreateBrush()
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

        m_BrushObject.layer = LAYER_WATER;

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
        m_MaskTexture.SetTarget(m_MaskLayer);

        // Activate Camera
        m_MaskCameraObject.GetComponent<Camera>().enabled = true;

        // Disable All Effects
        if (m_EffectLayerObject)
            m_EffectLayerObject.SetActive(false);
    }

    public void RemoveBrush()
    {
        if (m_BrushObject)
            Destroy(m_BrushObject);
        if (m_MaskLayerObject)
            Destroy(m_MaskLayerObject);

        if (m_MaskTexture)
            m_MaskTexture.SetTarget(null);
        
        if (m_MaskCameraObject)
            m_MaskCameraObject.GetComponent<Camera>().enabled = false;

        m_BrushObject = null;
        m_Brush = null;
        m_MaskLayerObject = null;
        m_MaskLayer = null;

        // Enable All Effects
        if (m_EffectLayerObject)
            m_EffectLayerObject.SetActive(true);
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
        m_MaskTexture.SetTarget(m_MaskLayer);
        m_FlowCamera.targetTexture = flowTex;
        m_FlowLayer.SetTexture(flowTex);

        // Disable All Effects
        if (m_EffectLayerObject)
            m_EffectLayerObject.SetActive(false);
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
        if (m_EffectLayerObject)
            m_EffectLayerObject.SetActive(true);
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
        m_MaskCamera.labelTexture = m_SLICLabelTexture;
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

        if (m_MaskCamera)
            m_MaskCamera.labelTexture = null;
    }

    public void CreatePCA()
    {
        if (!m_PaletteTextureObject)
        {
            m_PaletteTextureObject = GameObject.Instantiate(StaticTexturePrefab);
            m_PaletteTextureObject.name = "Static Texture: PCAPalette";
        }
        if (!m_PaletteTexture)
            m_PaletteTexture = m_PaletteTextureObject.GetComponent<StaticTexture>();
    }

    public void InvokePCA(int nextMode)
    {
        if (m_PCAClient && m_PaletteTexture)
        {
            m_PCAClient.paletteTextureProvider  = m_PaletteTexture;
            m_PCAClient.Invoke(
                m_RootStaticTexture,
                m_MaskTexture,
                m_SLICLabelTexture,
                nextMode
            );
        }
    }

    public void RemovePCA()
    {
        if (m_PaletteTextureObject)
            Destroy(m_PaletteTextureObject);
        
        m_PaletteTextureObject = null;
        m_PaletteTexture = null;
    }

    public void Effect_CL01() { CreateEffect(Constants.ModeWaterType.CL01); }
    public void Effect_CL02() { CreateEffect(Constants.ModeWaterType.CL02); }
    public void Effect_RV01() { CreateEffect(Constants.ModeWaterType.RV01); }

    public void CreateEffect(Constants.ModeWaterType effectType)
    {
        if (!m_EffectLayerObject)
        {
            m_EffectLayerObject = GameObject.Instantiate(LayerPrefab);
            m_EffectLayerObject.name = "Layer: Effect";
            m_EffectLayerObject.GetComponent<RectTransform>().SetParent(rootLayerObject.GetComponent<RectTransform>());
            m_EffectLayerObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        if (!m_EffectLayer)
        {
            m_EffectLayer = m_EffectLayerObject.GetComponent<RawImageController>();
            m_EffectLayer.movePosition = false;
            m_EffectLayer.moveScale    = true;
            m_EffectLayer.useGrid      = true;
            m_EffectLayer.globalScale  = 2f;
            m_EffectLayer.material     = new Material(Shader.Find("UI/Grid"));
            m_EffectLayer.material.SetVector("_RootImageSize", new Vector4(1f / (float)width, 1f / (float)height, (float)width, (float)height));
        }

        m_WaterEffectManager.paletteProvider     = m_PaletteTexture;
        m_WaterEffectManager.environmentProvider = m_EnvMapTexture;
        m_WaterEffectManager.target              = m_EffectLayer;
        m_WaterEffectManager.Setup(effectType, width / 2, height / 2);
    }

    public void RemoveEffect()
    {
        if (m_EffectLayerObject)
            Destroy(m_EffectLayerObject);

        m_EffectLayerObject = null;
        m_EffectLayer = null;

        m_WaterEffectManager.Setup(Constants.ModeWaterType.NONE, 0, 0);
    }

}