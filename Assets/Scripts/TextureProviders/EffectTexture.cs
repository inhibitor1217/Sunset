using UnityEngine;

public class EffectTexture : TextureProvider
{
    public int effectType;

    private const int PALETTE_SRC_INDEX = 0;
    private const int NOISE_SRC_INDEX = 1;
    private const int ENV_SRC_INDEX = 2;

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

    private RenderTexture m_RenderTexture;
    [SerializeField]
    private Material m_WaterMaterial;
    private int m_CalmPass;
    private int m_RiverPass;

    new void Awake()
    {
        base.Awake();

        m_WaterMaterial = new Material(Shader.Find("Compute/WaterEffect"));
        m_CalmPass  = m_WaterMaterial.FindPass("Calm");
        m_RiverPass = m_WaterMaterial.FindPass("River");
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

        m_RenderTexture.DiscardContents();
        Graphics.Blit(m_NoiseTex.GetTexture(), m_RenderTexture, m_WaterMaterial, m_CalmPass);
    }

    void DrawRiver()
    {
        m_RenderTexture.DiscardContents();
        Graphics.Blit(m_NoiseTex.GetTexture(), m_RenderTexture, m_WaterMaterial, m_RiverPass);
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
        m_RenderTexture.wrapMode = TextureWrapMode.Repeat;
        m_RenderTexture.filterMode = FilterMode.Bilinear;
    }

}