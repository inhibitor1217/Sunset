using UnityEngine;

public class EnvironmentTexture : TextureProvider
{

    [SerializeField]
    private RenderTexture m_RenderTexture;
    private Material m_EnvMapMaterial;
    private Material m_BlurMaterial;
    private int m_HorizontalBlurPass;

    new void Awake()
    {
        base.Awake();

        /* SETUP MATERIALS */
        m_EnvMapMaterial = new Material(Shader.Find("Compute/EnvMap"));
        m_BlurMaterial = new Material(Shader.Find("Compute/Blur"));
        m_BlurMaterial.SetFloat("_BlurSize", .005f);
        m_HorizontalBlurPass = m_BlurMaterial.FindPass("Horizontal");

        /* SETUP PROPERTIES */
        m_EnvMapMaterial.SetTexture("_ImgTex", EditorSceneMaster.Instance.GetRootTextureProvider().GetTexture());

        AddProperty("MaskTexture",        "PROVIDER");
        SubscribeProperty("MaskTexture", m_EnvMapMaterial, "_MaskTex");
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public override bool Draw()
    {
        if (!m_RenderTexture)
            return false;
            
        RenderTexture envMapRaw = RenderTexture.GetTemporary(m_RenderTexture.width, m_RenderTexture.height, 0, m_RenderTexture.format);
        m_RenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_RenderTexture.filterMode = FilterMode.Bilinear;

        // FilterMode prev = imgTex.filterMode;
        // imgTex.filterMode = FilterMode.Bilinear;
        envMapRaw.DiscardContents();
        Graphics.Blit(null, envMapRaw, m_EnvMapMaterial);
        // imgTex.filterMode = prev;

        m_RenderTexture.DiscardContents();
        Graphics.Blit(envMapRaw, m_RenderTexture, m_BlurMaterial, m_HorizontalBlurPass);

        RenderTexture.ReleaseTemporary(envMapRaw);

        return true;
    }

    public override string GetProviderName()
    {
        return "EnvironmentTexture";
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
        m_RenderTexture.filterMode = FilterMode.Bilinear;
    }



}