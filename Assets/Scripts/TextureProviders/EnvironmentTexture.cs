using UnityEngine;

public class EnvironmentTexture : TextureProvider
{

    [SerializeField]
    private RenderTexture m_RenderTexture;
    private Material m_EnvMapMaterial;

    private TextureProvider _maskProvider;
    private TextureProvider _paletteProvider;
    
    public TextureProvider maskProvider { 
        set {
            UpdatePipeline(ref _maskProvider, value);
            m_EnvMapMaterial.SetTexture("_MaskTex", value.GetTexture()); 
        } 
    }

    new void Awake()
    {
        base.Awake();

        /* SETUP MATERIALS */
        m_EnvMapMaterial = new Material(Shader.Find("Compute/EnvMap"));
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public override bool Draw()
    {
        if (!m_RenderTexture)
            return false;

        EditorSceneMaster.instance.GetRootTextureProvider().SetFilterMode(FilterMode.Bilinear);

        m_RenderTexture.DiscardContents();
        Graphics.Blit(null, m_RenderTexture, m_EnvMapMaterial);

        EditorSceneMaster.instance.GetRootTextureProvider().SetFilterMode(FilterMode.Point);
        
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

        /* SETUP PROPERTIES */
        m_EnvMapMaterial.SetTexture("_ImgTex", EditorSceneMaster.instance.GetRootTextureProvider().GetTexture());
    }



}