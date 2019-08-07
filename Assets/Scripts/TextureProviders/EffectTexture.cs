using UnityEngine;

public class EffectTexture : TextureProvider
{
    public int effectType;

    private const int PALETTE_SRC_INDEX = 0;
    private const int NOISE_SRC_INDEX = 1;
    private const int ENV_SRC_INDEX = 2;

    [SerializeField, Range(-360, 360)]
    private float _rotation = 0f;
    public float rotation
    {
        get { return _rotation; }
        set {
            _rotation = value;
            if (m_WaterMaterial)
            {
                float rotationRadians = Mathf.Deg2Rad * _rotation;
                m_WaterMaterial.SetVector(
                    "_Rotation", 
                    new Vector4(Mathf.Cos(rotationRadians), -Mathf.Sin(rotationRadians), Mathf.Sin(rotationRadians), Mathf.Cos(rotationRadians))
                );
            }
        }
    }

    [SerializeField, Range(0, 5)]
    private float _speed = .1f;
    public float speed
    {
        get { return _speed; }
        set {
            _speed = value;
            if (m_WaterMaterial)
            {
                m_WaterMaterial.SetFloat("_Speed", _speed);
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        rotation = _rotation;
        speed    = _speed;
    }
#endif

    private TextureProvider m_PaletteTex = null;
    public TextureProvider paletteTexture {
        get { return m_PaletteTex; }
        set {
            if (m_PaletteTex == value)
                return;

            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("EffectTexture: Palette Texture Pipeline Output is Full.");
                return;
            }

            if (m_PaletteTex)
                TextureProvider.Unlink(m_PaletteTex, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, PALETTE_SRC_INDEX);
            
            m_PaletteTex = value;
        }
    }

    private TextureProvider m_NoiseTex = null;
    public TextureProvider noiseTexture {
        get { return m_NoiseTex; }
        set {
            if (m_NoiseTex == value)
                return;
            
            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("EffectTexture: Noise Texture Pipeline Output is Full.");
                return;
            }

            if (m_NoiseTex)
                TextureProvider.Unlink(m_NoiseTex, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, NOISE_SRC_INDEX);

            m_NoiseTex = value;
        }
    }

    private TextureProvider m_EnvTex = null;
    public TextureProvider environmentTexture {
        get { return m_EnvTex; }
        set {
            if (m_EnvTex == value)
                return;
            
            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("EffectTexture: Environment Texture Pipeline Output is Full.");
                return;
            }

            if (m_EnvTex)
                TextureProvider.Unlink(m_EnvTex, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, ENV_SRC_INDEX);

            m_EnvTex = value;
        }
    }

    [SerializeField]
    private RenderTexture m_RenderTexture;
    [SerializeField]
    private Material m_WaterMaterial;
    private int m_PerspectivePass;
    private int m_CalmPass;
    private int m_RiverPass;
    private Material m_GradientMaterial;

    new void Awake()
    {
        base.Awake();

        m_WaterMaterial = new Material(Shader.Find("Compute/WaterEffect"));
        m_PerspectivePass = m_WaterMaterial.FindPass("Perspective");
        m_CalmPass        = m_WaterMaterial.FindPass("Calm");
        m_RiverPass       = m_WaterMaterial.FindPass("River");

        speed    = _speed;
        rotation = _rotation;

        m_GradientMaterial = new Material(Shader.Find("Compute/Gradient"));
    }

    public override bool Draw()
    {
        if (!m_RenderTexture)
            return false;

        switch(effectType)
        {
        case EditorSceneMaster.WATER_TYPE_CALM:
            DrawCalm();
            break;
        case EditorSceneMaster.WATER_TYPE_RIVER:
            DrawRiver();
            break;
        default:
            break;
        }

        return true;
    }

    void DrawCalm()
    {
        m_WaterMaterial.SetTexture("_ImgTex", EditorSceneMaster.Instance.GetRootTextureProvider().GetBlurredTexture());
        m_WaterMaterial.SetTexture("_PaletteTex", m_PaletteTex.GetTexture());
        m_WaterMaterial.SetTexture("_EnvTex", m_EnvTex.GetTexture());

        Texture noiseTex = m_NoiseTex.GetTexture();

        RenderTexture noisePerspective = RenderTexture.GetTemporary(m_RenderTexture.width, m_RenderTexture.height, 0, RenderTextureFormat.RFloat);
        noisePerspective.filterMode = FilterMode.Bilinear;
        noisePerspective.wrapMode   = TextureWrapMode.Repeat;
        Graphics.Blit(noiseTex, noisePerspective, m_WaterMaterial, m_PerspectivePass);

        m_RenderTexture.DiscardContents();
        Graphics.Blit(noisePerspective, m_RenderTexture, m_WaterMaterial, m_CalmPass);

        RenderTexture.ReleaseTemporary(noisePerspective);
    }

    void DrawRiver()
    {
        m_WaterMaterial.SetTexture("_ImgTex", EditorSceneMaster.Instance.GetRootTextureProvider().GetBlurredTexture());
        m_WaterMaterial.SetTexture("_PaletteTex", m_PaletteTex.GetTexture());
        m_WaterMaterial.SetTexture("_EnvTex", m_EnvTex.GetTexture());

        Texture noiseTex = m_NoiseTex.GetTexture();

        RenderTexture noiseGradient = RenderTexture.GetTemporary(noiseTex.width, noiseTex.height, 0, RenderTextureFormat.ARGBFloat);
        noiseGradient.filterMode = FilterMode.Bilinear;
        noiseGradient.wrapMode   = TextureWrapMode.Repeat;
        noiseGradient.useMipMap  = true;
        Graphics.Blit(noiseTex, noiseGradient, m_GradientMaterial);

        RenderTexture noisePerspective = RenderTexture.GetTemporary(m_RenderTexture.width, m_RenderTexture.height, 0, RenderTextureFormat.ARGBFloat);
        noisePerspective.filterMode = FilterMode.Bilinear;
        noisePerspective.wrapMode   = TextureWrapMode.Repeat;
        Graphics.Blit(noiseGradient, noisePerspective, m_WaterMaterial, m_PerspectivePass);

        m_RenderTexture.DiscardContents();
        Graphics.Blit(noisePerspective, m_RenderTexture, m_WaterMaterial, m_RiverPass);

        RenderTexture.ReleaseTemporary(noiseGradient);
        RenderTexture.ReleaseTemporary(noisePerspective);
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public void Setup(int width, int height)
    {
        if (m_RenderTexture)
            m_RenderTexture.Release();

        m_RenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        m_RenderTexture.useMipMap = false;
        m_RenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_RenderTexture.filterMode = FilterMode.Point;
    }

}