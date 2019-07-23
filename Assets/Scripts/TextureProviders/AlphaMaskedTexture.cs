using UnityEngine;

public class AlphaMaskedTexture : TextureProvider
{
    
    private const int SOURCE_SRC_INDEX = 0;
    private const int ALPHA_SRC_INDEX = 1;

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
                TextureProvider.Link(value, value.SeekFreeIndex(), this, SOURCE_SRC_INDEX);
            
            m_SrcTexture = value;
        }
    }

    private TextureProvider m_AlphaTexture = null;
    public TextureProvider alphaTexture {
        get { return m_AlphaTexture; }
        set {
            if (m_AlphaTexture == value)
                return;

            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("AlphaMaskedTexture: Alpha Texture Pipeline Output is Full.");
                return;
            }
            
            if (m_AlphaTexture)
                TextureProvider.Unlink(m_AlphaTexture, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, ALPHA_SRC_INDEX);
            
            m_AlphaTexture = value;
        }
    }

    public TextureProvider defaultSourceTexture = null;
    public TextureProvider defaultAlphaTexture = null;

    private RenderTexture m_RenderTexture;
    private Material m_AlphaMaskMaterial;

    new void Awake()
    {
        base.Awake();

        m_AlphaMaskMaterial = new Material(Shader.Find("Compute/AlphaMask"));
    }

    void Start()
    {
        sourceTexture = defaultSourceTexture;
        alphaTexture = defaultAlphaTexture;
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

        Graphics.SetRenderTarget(m_RenderTexture, 0, CubemapFace.Unknown, 0);
        GL.Clear(false, true, Color.black, 0);
        Graphics.Blit(m_SrcTexture.GetTexture(), m_RenderTexture, m_AlphaMaskMaterial);
        
        return true;
    }
    
    public void Setup()
    {
        if (m_RenderTexture)
            m_RenderTexture.Release();

        Texture srcTex = m_SrcTexture.GetTexture();
        Texture alphaTex = m_AlphaTexture.GetTexture();

        m_RenderTexture = new RenderTexture(srcTex.width, srcTex.height, 0, RenderTextureFormat.ARGBHalf);
        m_RenderTexture.useMipMap = false;
        m_RenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_RenderTexture.filterMode = FilterMode.Point;
        
        m_AlphaMaskMaterial.SetTexture("_AlphaTex", alphaTex);
    }

}