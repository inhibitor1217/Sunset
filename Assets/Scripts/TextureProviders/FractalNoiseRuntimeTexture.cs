using UnityEngine;

public class FractalNoiseRuntimeTexture : TextureProvider
{

    private static string[] NOISE_TYPES = { "VALUE", "VALUE_LINEAR", "VALUE_SPLINE", "PERLIN", "PERLIN_LINEAR" };
    private static string[] FRACTAL_TYPES = { "BASIC", "TURBULENT", "ROCKY" }; 

    [Header("Texture")]
    public int resolution = 256;

    [Header("General")]
    [SerializeField]
    private int _seed = 0;
    public int seed {
        get { return _seed; }
        set {
            _seed = value;
            loadGradients(_seed);
        }
    }
    [SerializeField]
    private int _noiseType = 4;
    public int noiseType {
        get { return _noiseType; }
        set { updateNoiseType(value); }
    }
    [SerializeField]
    private int _fractalType = 0;
    public int fractalType {
        get { return (int)_fractalType; }
        set { updateFractalType(value); }
    }

    [Header("Transform")]
    [SerializeField]
    private Vector2 _offset = Vector2.zero;
    public Vector2 offset {
        get { return _offset; }
        set { updateGlobalOffset(value); }
    }
    [SerializeField]
    private Vector2 _scale = 8f * Vector2.one;
    public Vector2 scale {
        get { return _scale; }
        set { updateGlobalScale(value); }
    }
    // [SerializeField, Range(-360, 360)]
    // private float _rotation = 0f;
    // public float rotation {
    //     get { return _rotation; }
    //     set { updateGlobalRotation(value); }
    // }

    [Header("Complexity")]
    [SerializeField, Range(1, 10)]
    private int _complexity = 6;
    public int complexity {
        get { return _complexity; }
        set { updateComplexity(value); }
    }
    [SerializeField, Range(0, 1)]
    private float _subInfluence = .5f;
    public float subInfluence {
        get { return _subInfluence; }
        set { updateSubInfluence(value); }
    }
    [SerializeField]
    private Vector2 _subScale = 2f * Vector2.one;
    public Vector2 subScale {
        get { return _subScale; }
        set { updateSubScale(value); }
    }
    // [SerializeField, Range(-360, 360)]
    // private float _subRotation = 0f;
    // public float subRotation {
    //     get { return _subRotation; }
    //     set { updateSubRotation(value); }
    // }
    [SerializeField]
    private Vector2 _subOffset = Vector2.zero;
    public Vector2 subOffset {
        get { return _subOffset; }
        set { updateSubOffset(value); }
    }

    [Header("Output")]
    [SerializeField, Range(-2f, 2f)]
    private float _brightness = 0f;
    public float brightness {
        get { return _brightness; }
        set { updateBrightness(value); }
    }
    [SerializeField, Range(0, 10)]
    private float _contrast = 1f;
    public float contrast {
        get { return _contrast; }
        set { updateContrast(value); }
    }

    [Header("Evolution")]
    [SerializeField]
    private bool _enableEvolution = false;
    public bool enableEvolution {
        get { return _enableEvolution; }
        set {
            _enableEvolution = value;
            if (value)
                updateEvolutionSpeed(evolutionSpeed);
            else
                updateEvolutionSpeed(0);
        }
    }
    [SerializeField]
    private float _evolutionSpeed = 0f;
    public float evolutionSpeed {
        get { return _evolutionSpeed; }
        set { updateEvolutionSpeed(value); }
    }
    // [SerializeField]
    // private Vector2 _globalVelocity = Vector2.zero;
    // public Vector2 globalVelocity {
    //     get { return _globalVelocity; }
    //     set { updateGlobalVelocity(value); }
    // }
    // [SerializeField]
    // private Vector2 _subVelocity = Vector2.zero;
    // public Vector2 subVelocity {
    //     get { return _subVelocity; }
    //     set { updateSubVelocity(value); }
    // }


    private Material m_FractalNoiseMaterial;
    [SerializeField]
    private RenderTexture m_RenderTexture;

    new void Awake()
    {
        base.Awake();

        m_FractalNoiseMaterial = new Material(Shader.Find("Compute/FractalNoise"));
        m_RenderTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RHalf);
        m_RenderTexture.wrapMode   = TextureWrapMode.Repeat;
        m_RenderTexture.filterMode = FilterMode.Trilinear;
        m_RenderTexture.useMipMap  = true;
    }

    new void Start()
    {
        base.Start();
        
        noiseType = _noiseType;
        fractalType = _fractalType;
        seed = _seed;
        offset = _offset;
        scale = _scale;
        // rotation = _rotation;
        complexity = _complexity;
        subInfluence = _subInfluence;
        subScale = _subScale;
        subOffset = _subOffset;
        brightness = _brightness;
        contrast = _contrast;
        enableEvolution = _enableEvolution;
        evolutionSpeed = _evolutionSpeed;
        // globalVelocity = _globalVelocity;
        // subVelocity = _subVelocity;
    }

    void Update()
    {
        if (enableEvolution
            && !InputMode.Instance.isBrush()
            && !InputMode.Instance.isBusy())
            textureShouldUpdate = true;

        if (!HasOutput())
            Destroy(gameObject);
    }

    new void OnDestroy()
    {
        base.OnDestroy();
        
        if (m_RenderTexture)
            m_RenderTexture.Release();
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public override bool Draw()
    {
        if (m_FractalNoiseMaterial && m_RenderTexture)
        {
            m_RenderTexture.DiscardContents();
            Graphics.Blit(null, m_RenderTexture, m_FractalNoiseMaterial);

            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        noiseType = _noiseType;
        fractalType = _fractalType;
        seed = _seed;
        offset = _offset;
        scale = _scale;
        // rotation = _rotation;
        complexity = _complexity;
        subInfluence = _subInfluence;
        subScale = _subScale;
        subOffset = _subOffset;
        // subRotation = _subRotation;
        brightness = _brightness;
        contrast = _contrast;
        enableEvolution = _enableEvolution;
        evolutionSpeed = _evolutionSpeed;
        // globalVelocity = _globalVelocity;
        // subVelocity = _subVelocity;
    }
#endif

    void updateNoiseType(int value)
    {
        _noiseType = value;
        if (m_FractalNoiseMaterial)
        {
            for (int i = 0; i < NOISE_TYPES.Length; i++)
            {
                if (i == value)
                    m_FractalNoiseMaterial.EnableKeyword(NOISE_TYPES[i]);
                else
                    m_FractalNoiseMaterial.DisableKeyword(NOISE_TYPES[i]);
            }
            textureShouldUpdate = true;
        }
    }

    void updateFractalType(int value)
    {
        _fractalType = value;
        if (m_FractalNoiseMaterial)
        {
            for (int i = 0; i < FRACTAL_TYPES.Length; i++)
            {
                if (i == value)
                    m_FractalNoiseMaterial.EnableKeyword(FRACTAL_TYPES[i]);
                else
                    m_FractalNoiseMaterial.DisableKeyword(FRACTAL_TYPES[i]);
            }
            textureShouldUpdate = true;
        }
    }

    void loadGradients(int seed)
    {
        Random.InitState(_seed);
        Vector4[] gradients = new Vector4[256];
        for (int i = 0; i < 256; i++)
            gradients[i] = (2f * new Vector4(Random.value, Random.value, Random.value, 1f) - Vector4.one).normalized;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetVectorArray("_Gradients", gradients);
            textureShouldUpdate = true;
        }
    }

    void updateGlobalOffset(Vector2 value)
    {
        _offset = value;
        if (m_FractalNoiseMaterial)
        {
            Vector4 oldValue = m_FractalNoiseMaterial.GetVector("_GlobalOffsetScale");
            m_FractalNoiseMaterial.SetVector("_GlobalOffsetScale", new Vector4(value.x, value.y, oldValue.z, oldValue.w));
            textureShouldUpdate = true;
        }
    }

    void updateGlobalScale(Vector2 value)
    {
        _scale = value;
        if (m_FractalNoiseMaterial)
        {
            Vector4 oldValue = m_FractalNoiseMaterial.GetVector("_GlobalOffsetScale");
            m_FractalNoiseMaterial.SetVector("_GlobalOffsetScale", new Vector4(oldValue.x, oldValue.y, value.x, value.y));
            textureShouldUpdate = true;
        }
    }

    // void updateGlobalRotation(float value)
    // {
    //     _rotation = value;
    //     if (m_FractalNoiseMaterial)
    //     {
    //         m_FractalNoiseMaterial.SetFloat("_GlobalRotation", _rotation);
    //         textureShouldUpdate = true;
    //     }
    // }

    void updateComplexity(int value)
    {
        _complexity = value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetInt("_Complexity", _complexity);
            textureShouldUpdate = true;
        }
    }

    void updateSubInfluence(float value)
    {
        _subInfluence = value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetFloat("_SubInfluence", _subInfluence);
            textureShouldUpdate = true;
        }
    }

    void updateSubScale(Vector2 value)
    {
        _subScale = value;
        if (m_FractalNoiseMaterial)
        {
            Vector4 oldValue = m_FractalNoiseMaterial.GetVector("_SubOffsetScale");
            m_FractalNoiseMaterial.SetVector("_SubOffsetScale", new Vector4(oldValue.x, oldValue.y, _subScale.x, _subScale.y));
            textureShouldUpdate = true;
        }
    }

    // void updateSubRotation(float value)
    // {
    //     _subRotation = value;
    //     if (m_FractalNoiseMaterial)
    //     {
    //         m_FractalNoiseMaterial.SetFloat("_SubRotation", _subRotation);
    //         textureShouldUpdate = true;
    //     }
    // }

    void updateSubOffset(Vector2 value)
    {
        _subOffset = value;
        if (m_FractalNoiseMaterial)
        {
            Vector4 oldValue = m_FractalNoiseMaterial.GetVector("_SubOffsetScale");
            m_FractalNoiseMaterial.SetVector("_SubOffsetScale", new Vector4(_subOffset.x, _subOffset.y, oldValue.z, oldValue.w));
            textureShouldUpdate = true;
        }
    }

    void updateBrightness(float value)
    {
        _brightness = value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetFloat("_Brightness", _brightness);
            textureShouldUpdate = true;
        }
    }

    void updateContrast(float value)
    {
        _contrast = value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetFloat("_Contrast", _contrast);
            textureShouldUpdate = true;
        }
    }

    void updateEvolutionSpeed(float value)
    {
        _evolutionSpeed = value;
        if (m_FractalNoiseMaterial)
        {
            m_FractalNoiseMaterial.SetFloat("_EvolutionSpeed", _evolutionSpeed);
            textureShouldUpdate = true;
        }
    }

    // void updateGlobalVelocity(Vector2 value)
    // {
    //     _globalVelocity = value;
    //     if (m_FractalNoiseMaterial)
    //     {
    //         Vector4 oldValue = m_FractalNoiseMaterial.GetVector("_Velocity");
    //         m_FractalNoiseMaterial.SetVector("_Velocity", new Vector4(value.x, value.y, oldValue.z, oldValue.w));
    //         textureShouldUpdate = true;
    //     }
    // }

    // void updateSubVelocity(Vector2 value)
    // {
    //     _subVelocity = value;
    //     if (m_FractalNoiseMaterial)
    //     {
    //         Vector4 oldValue = m_FractalNoiseMaterial.GetVector("_Velocity");
    //         m_FractalNoiseMaterial.SetVector("_Velocity", new Vector4(oldValue.x, oldValue.y, value.x, value.y));
    //         textureShouldUpdate = true;
    //     }
    // }

}
