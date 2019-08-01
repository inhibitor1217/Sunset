using UnityEngine;

public class BlurTexture : TextureProvider
{
    
    private const int SOURCE_INDEX = 0;

    private TextureProvider m_SrcTexture = null;
    public TextureProvider sourceTexture {
        get { return m_SrcTexture; }
        set {
            if (m_SrcTexture == value)
                return;
            
            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("AlphaMaskedTexture: Source Texture Pipeline Output is Full.");
                return;
            }

            if (m_SrcTexture)
                TextureProvider.Unlink(m_SrcTexture, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, SOURCE_INDEX);
            
            m_SrcTexture = value;
        }
    }

    private RenderTexture m_RenderTexture;
    private Material m_BlurMaterial;
    private int m_HorizontalPass, m_VerticalPass;

    private const float BLUR_SIZE = .05f;

    new void Awake()
    {
        base.Awake();

        m_BlurMaterial = new Material(Shader.Find("Compute/Blur"));
        m_HorizontalPass = m_BlurMaterial.FindPass("Horizontal");
        m_VerticalPass   = m_BlurMaterial.FindPass("Vertical");
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
        if (!m_RenderTexture)
            return false;

        Texture srcTex = m_SrcTexture.GetTexture();
        RenderTexture temp = RenderTexture.GetTemporary(srcTex.width, srcTex.height, 0, RenderTextureFormat.R8);

        m_RenderTexture.DiscardContents();
        m_BlurMaterial.SetFloat("_BlurSize", BLUR_SIZE);
        Graphics.Blit(m_SrcTexture.GetTexture(), temp, m_BlurMaterial, m_HorizontalPass);
        m_BlurMaterial.SetFloat("_BlurSize", .25f * BLUR_SIZE);
        Graphics.Blit(temp, m_RenderTexture, m_BlurMaterial, m_VerticalPass);

        RenderTexture.ReleaseTemporary(temp);
        
        return true;
    }
    
    public void Setup()
    {
        if (m_RenderTexture)
            m_RenderTexture.Release();

        Texture srcTex = m_SrcTexture.GetTexture();

        m_RenderTexture = new RenderTexture(srcTex.width / 4, srcTex.height / 4, 0, RenderTextureFormat.R8);
        m_RenderTexture.useMipMap = false;
        m_RenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_RenderTexture.filterMode = FilterMode.Bilinear;
    }

}
