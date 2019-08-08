using UnityEngine;

public class EnvironmentTexture : TextureProvider
{

    private const int SRC_IMG_INDEX = 0;
    private const int SRC_MASK_INDEX = 1;

    private TextureProvider m_ImgTexture = null;
    public TextureProvider imageTexture {
        get { return m_ImgTexture; }
        set {
            if (m_ImgTexture == value)
                return;
            
            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("EnvironmentTexture: Image Texture Pipeline Output is Full.");
                return;
            }

            if (m_ImgTexture)
                TextureProvider.Unlink(m_ImgTexture, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, SRC_IMG_INDEX);
            
            m_ImgTexture = value;
        }
    }

    private TextureProvider m_MaskTexture = null;
    public TextureProvider maskTexture {
        get { return m_MaskTexture; }
        set {
            if (m_MaskTexture == value)
                return;
            
            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("EnvironmentTexture: Mask Texture Pipeline Output is Full.");
                return;
            }

            if (m_MaskTexture)
                TextureProvider.Unlink(m_MaskTexture, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, SRC_MASK_INDEX);
            
            m_MaskTexture = value;
        }
    }

    [SerializeField]
    private RenderTexture m_RenderTexture;
    private Material m_EnvMapMaterial;
    private Material m_BlurMaterial;
    private int m_HorizontalBlurPass;

    new void Awake()
    {
        base.Awake();

        m_EnvMapMaterial = new Material(Shader.Find("Compute/EnvMap"));

        m_BlurMaterial = new Material(Shader.Find("Compute/Blur"));
        m_BlurMaterial.SetFloat("_BlurSize", .02f);
        m_HorizontalBlurPass = m_BlurMaterial.FindPass("Horizontal");
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public override bool Draw()
    {
        if (!m_RenderTexture)
            return false;

        m_EnvMapMaterial.SetTexture("_MaskTex", m_MaskTexture.GetTexture());

        Texture imgTex = m_ImgTexture.GetTexture();

        FilterMode prev = imgTex.filterMode;
        imgTex.filterMode = FilterMode.Bilinear;
        m_RenderTexture.DiscardContents();
        Graphics.Blit(imgTex, m_RenderTexture, m_EnvMapMaterial);
        imgTex.filterMode = prev;

        Graphics.Blit(m_RenderTexture, m_RenderTexture, m_BlurMaterial, m_HorizontalBlurPass);

        return true;
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