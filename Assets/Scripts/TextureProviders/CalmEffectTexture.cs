using UnityEngine;

public class CalmEffectTexture : TextureProvider
{

    private RenderTexture m_RenderTexture;
    private Material m_WaterMaterial;
    private int m_CalmPass;

    private TextureProvider _paletteProvider;
    private TextureProvider _noiseProvider;
    private TextureProvider _environmentProvider;

    public TextureProvider paletteProvider {
        set {
            UpdatePipeline(ref _paletteProvider, value);
            m_WaterMaterial.SetTexture("_PaletteTex", value.GetTexture());
        }
    }
    public TextureProvider noiseProvider {
        set {
            UpdatePipeline(ref _noiseProvider, value);
            m_WaterMaterial.SetTexture("_NoiseTex", value.GetTexture());
        }
    }
    public TextureProvider environmentProvider {
        set {
            UpdatePipeline(ref _environmentProvider, value);
            m_WaterMaterial.SetTexture("_EnvTex", value.GetTexture());
        }
    }

    new void Awake()
    {
        base.Awake();

        /* SETUP MATERIALS */
        m_WaterMaterial = new Material(Shader.Find("Compute/WaterEffect"));
        m_WaterMaterial.EnableKeyword("USE_MIPMAP");
        m_CalmPass        = m_WaterMaterial.FindPass("Calm");
    }

    public override bool Draw()
    {
        if (!m_RenderTexture)
            return false;
        
        m_RenderTexture.DiscardContents();
        Graphics.Blit(null, m_RenderTexture, m_WaterMaterial, m_CalmPass);

        return true;
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public override string GetProviderName()
    {
        return "CalmEffectTexture";
    }

    new void OnDestroy()
    {
        base.OnDestroy();
        
        if (m_RenderTexture)
            m_RenderTexture.Release();
    }

    public void Setup(int width, int height)
    {
        if (m_RenderTexture)
            m_RenderTexture.Release();

        m_RenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        m_RenderTexture.useMipMap = false;
        m_RenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_RenderTexture.filterMode = FilterMode.Point;

        /* SETUP PROPERTIES */
        m_WaterMaterial.SetTexture("_ImgTex", EditorSceneMaster.instance.GetRootTextureProvider().GetBlurredTexture());

        Subscribe(SharedActions.FIELD__HORIZON,         m_WaterMaterial, "_Horizon",        "Float");
        Subscribe(SharedActions.FIELD__PERSPECTIVE,     m_WaterMaterial, "_Perspective",    "Float");
        Subscribe(SharedActions.FIELD__LIGHT_DIRECTION, m_WaterMaterial, "_LightDirection", "Vector");
    }
}