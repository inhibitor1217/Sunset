using UnityEngine;

public class EffectTexture : TextureProvider
{

    private RenderTexture m_RenderTexture;
    [SerializeField]
    private Material m_WaterMaterial;

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
    }

    public override bool Draw()
    {
        if (!m_RenderTexture)
            return false;

        m_RenderTexture.DiscardContents();
        Graphics.Blit(null, m_RenderTexture, m_WaterMaterial);

        return true;
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public override string GetProviderName()
    {
        return "EffectTexture";
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
        m_WaterMaterial.SetTexture("_ImgTex", EditorSceneMaster.instance.GetRootTextureProvider().GetTexture());
        m_WaterMaterial.SetTexture("_ImgBlurTex", EditorSceneMaster.instance.GetRootTextureProvider().GetBlurredTexture());
  
        Subscribe(SharedActions.FIELD__HORIZON,         m_WaterMaterial, "_Horizon", "Float");
        Subscribe(SharedActions.FIELD__PERSPECTIVE,     m_WaterMaterial, "_Perspective", "Float");
        Subscribe(
            WaterEffectActions.FIELD__ROTATION,
            (state) => {
                float rotation = (float)state[WaterEffectActions.FIELD__ROTATION];
                m_WaterMaterial.SetVector("_Rotation", new Vector4(Mathf.Cos(rotation), -Mathf.Sin(rotation), Mathf.Sin(rotation), Mathf.Cos(rotation)));
                textureShouldUpdate = true;
            });
        Subscribe(WaterEffectActions.FIELD__VERTICAL_BLUR_WIDTH, m_WaterMaterial, "_VerticalBlurWidth", "Float");
        Subscribe(WaterEffectActions.FIELD__VERTICAL_BLUR_STRENGTH, m_WaterMaterial, "_VerticalBlurStrength", "Float");
        Subscribe(WaterEffectActions.FIELD__DISTORTION_STRENGTH, m_WaterMaterial, "_DistortionStrength", "Float");
        Subscribe(WaterEffectActions.FIELD__TONE_STRENGTH, m_WaterMaterial, "_ToneStrength", "Float");
    }
}