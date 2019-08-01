using UnityEngine;

public class EffectTexture : TextureProvider
{

#if UNITY_EDITOR
    public TextureProvider defaultPaletteTex;
    public TextureProvider defaultNoiseTex;
    public TextureProvider defaultMaskTex;
#endif

    private const int PALETTE_SRC_INDEX = 0;
    private const int NOISE_SRC_INDEX = 1;
    private const int MASK_SRC_INDEX = 2;

    private TextureProvider m_PaletteTex = null;
    public TextureProvider paletteTexture {
        get { return m_PaletteTex; }
        set {
            if (m_PaletteTex == value)
                return;

            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("WaterTexture: Palette Texture Pipeline Output is Full.");
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
                Debug.Log("WaterTexture: Noise Texture Pipeline Output is Full.");
                return;
            }

            if (m_NoiseTex)
                TextureProvider.Unlink(m_NoiseTex, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, NOISE_SRC_INDEX);

            m_NoiseTex = value;
        }
    }

    private TextureProvider m_MaskTex = null;
    public TextureProvider maskTexture {
        get { return m_MaskTex; }
        set {
            if (m_MaskTex == value)
                return;
            
            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("WaterTexture: Mask Texture Pipeline Output is Full.");
                return;
            }

            if (m_MaskTex)
                TextureProvider.Unlink(m_MaskTex, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, MASK_SRC_INDEX);

            m_MaskTex = value;
        }
    }

    private RenderTexture m_RenderTexture;
    [SerializeField]
    private Material m_PCAWaterMaterial;

    new void Awake()
    {
        base.Awake();

        m_PCAWaterMaterial = new Material(Shader.Find("Compute/PCAEffect"));
    }

    new void Start()
    {
        base.Start();

#if UNITY_EDITOR
        if (defaultPaletteTex)
            paletteTexture = defaultPaletteTex;
        if (defaultNoiseTex)
            noiseTexture = defaultNoiseTex;
        if (defaultMaskTex)
            maskTexture = defaultMaskTex;
#endif
    }

    public override bool Draw()
    {
        if (!m_RenderTexture)
            return false;

        m_PCAWaterMaterial.SetTexture("_PaletteTex", m_PaletteTex.GetTexture());
        m_PCAWaterMaterial.SetTexture("_MaskTex", m_MaskTex.GetTexture());

        m_RenderTexture.DiscardContents();
        Graphics.Blit(m_NoiseTex.GetTexture(), m_RenderTexture, m_PCAWaterMaterial);

        return true;
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