using UnityEngine;

public class FractalNoiseRuntimeTexture : TextureProvider
{

    public ActionModule subscribeTarget;

    private static string[] NOISE_TYPES = { "VALUE", "VALUE_LINEAR", "VALUE_SPLINE", "PERLIN", "PERLIN_LINEAR" };
    private static string[] FRACTAL_TYPES = { "BASIC", "TURBULENT", "ROCKY" }; 

    public const int NUM_FIELDS = 10;
    public const int INDEX__FRACTAL_TYPE    = 0;
    public const int INDEX__NOISE_TYPE      = 1;
    public const int INDEX__SEED            = 2;
    public const int INDEX__GLOBAL_SCALE    = 3;
    public const int INDEX__SUB_INFLUENCE   = 4;
    public const int INDEX__SUB_SCALE       = 5;
    public const int INDEX__BRIGHTNESS      = 6;
    public const int INDEX__CONTRAST        = 7;
    public const int INDEX__EVOLUTION_SPEED = 8;
    public const int INDEX__AMPLITUDE       = 9;

    [SerializeField]
    private Material m_FractalNoiseMaterial;
    [SerializeField]
    private Material m_GradientMaterial;  
    [SerializeField]  
    private RenderTexture m_RenderTexture;

    public string[] fieldNames;

    private readonly float[] HASH_ARRAY = {
        151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
        140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
        247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
        57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
        74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
        60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
        65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
        200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
        52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
        207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
        119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
        129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
        218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
        81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
        184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
        222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
    };

    new void Awake()
    {
        base.Awake();

        /* SETUP MATERIALS */
        m_FractalNoiseMaterial = new Material(Shader.Find("Compute/FractalNoise"));
        m_GradientMaterial     = new Material(Shader.Find("Compute/Gradient"));
    }

    void Update()
    {
        if (Store.instance.GetValue<float>(fieldNames[INDEX__EVOLUTION_SPEED]) > 0
            && !InputMode.instance.isBrush()
            && !InputMode.instance.isBusy()
            && !InputMode.instance.isFlow())
            textureShouldUpdate = true;
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
        if (m_RenderTexture)
        {
            RenderTexture noiseRaw = RenderTexture.GetTemporary(m_RenderTexture.width, m_RenderTexture.height, 0, RenderTextureFormat.RFloat);
            Graphics.Blit(null, noiseRaw, m_FractalNoiseMaterial);

            m_RenderTexture.DiscardContents();
            Graphics.Blit(noiseRaw, m_RenderTexture, m_GradientMaterial);

            RenderTexture.ReleaseTemporary(noiseRaw);

            return true;
        }

        return false;
    }

    public override string GetProviderName()
    {
        return "FractalNoiseRuntimeTexture";
    }

    public void Setup()
    {
        m_RenderTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
        m_RenderTexture.wrapMode   = TextureWrapMode.Repeat;
        m_RenderTexture.filterMode = FilterMode.Trilinear;
        m_RenderTexture.useMipMap  = true;

        /* SUBMIT STATIC HASH ARRAY */
        Texture2D hashTex = new Texture2D(256, 1, TextureFormat.RFloat, false);
        hashTex.wrapMode = TextureWrapMode.Repeat;
        hashTex.filterMode = FilterMode.Point;
        Color[] hashColors = new Color[256];
        for (int i = 0; i < 256; i++)
            hashColors[i] = new Color(HASH_ARRAY[i] / 256f, 0, 0, 0);
        hashTex.SetPixels(hashColors);
        hashTex.Apply();
        m_FractalNoiseMaterial.SetTexture("_HashTex", hashTex);

        /* SETUP SUBSCRIPTIONS */
        Subscribe(fieldNames[INDEX__NOISE_TYPE],
            (state) => {
                int value = (int)state[WaterEffectActions.FIELD__NOISE_TYPE];
                for (int i = 0; i < NOISE_TYPES.Length; i++)
                {
                    if (i == value)
                        m_FractalNoiseMaterial.EnableKeyword(NOISE_TYPES[i]);
                    else
                        m_FractalNoiseMaterial.DisableKeyword(NOISE_TYPES[i]);
                }
                textureShouldUpdate = true;
            });

        Subscribe(fieldNames[INDEX__FRACTAL_TYPE],
            (state) => {
                int value = (int)state[WaterEffectActions.FIELD__FRACTAL_TYPE];
                for (int i = 0; i < FRACTAL_TYPES.Length; i++)
                {
                    if (i == value)
                        m_FractalNoiseMaterial.EnableKeyword(FRACTAL_TYPES[i]);
                    else
                        m_FractalNoiseMaterial.DisableKeyword(FRACTAL_TYPES[i]);
                }
                textureShouldUpdate = true;
            });

        Subscribe(fieldNames[INDEX__SEED],
            (state) => {
                int value = (int)state[WaterEffectActions.FIELD__SEED];
                Texture2D gradientTex = new Texture2D(256, 1, TextureFormat.ARGB32, false);
                gradientTex.wrapMode = TextureWrapMode.Repeat;
                gradientTex.filterMode = FilterMode.Point;
                Color[] graidentColors = new Color[256];
                Random.InitState(value);
                for (int i = 0; i < 256; i++)
                {
                    Vector3 dir = (2 * new Vector3(Random.value, Random.value, Random.value) - Vector3.one).normalized;
                    graidentColors[i] = new Color(.5f * dir.x + .5f, .5f * dir.y + .5f, .5f * dir.z + .5f, 1);
                }
                gradientTex.SetPixels(graidentColors);
                gradientTex.Apply();
                m_FractalNoiseMaterial.SetTexture("_GradientTex", gradientTex);
                textureShouldUpdate = true;
            });

        Subscribe(fieldNames[INDEX__GLOBAL_SCALE],
            (state) => {
                Vector2 value = (Vector2)state[WaterEffectActions.FIELD__GLOBAL_SCALE];
                m_FractalNoiseMaterial.SetVector("_GlobalScale", new Vector4(1f/value.x, 1f/value.y, value.x, value.y));
                textureShouldUpdate = true;
            });
        Subscribe(fieldNames[INDEX__SUB_INFLUENCE]  , m_FractalNoiseMaterial, "_SubInfluence"  , "Float" );
        Subscribe(fieldNames[INDEX__SUB_SCALE],
            (state) => {
                Vector2 value = (Vector2)state[WaterEffectActions.FIELD__SUB_SCALE];
                m_FractalNoiseMaterial.SetVector("_SubScale", new Vector4(1f/value.x, 1f/value.y, value.x, value.y));
                textureShouldUpdate = true;
            });
        Subscribe(fieldNames[INDEX__BRIGHTNESS]     , m_FractalNoiseMaterial, "_Brightness"    , "Float" );
        Subscribe(fieldNames[INDEX__CONTRAST]       , m_FractalNoiseMaterial, "_Contrast"      , "Float" );
        Subscribe(fieldNames[INDEX__EVOLUTION_SPEED], m_FractalNoiseMaterial, "_EvolutionSpeed", "Float" );
        Subscribe(fieldNames[INDEX__AMPLITUDE]      , m_GradientMaterial    , "_Amplitude"     , "Float" );
    }

}
