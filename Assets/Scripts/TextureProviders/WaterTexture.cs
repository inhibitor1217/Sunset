using UnityEngine;

public class WaterTexture : TextureProvider
{

#if UNITY_EDITOR
    public TextureProvider defaultLowTex;
    public TextureProvider defaultHighTex;
    public TextureProvider defaultNoiseTex;
    public TextureProvider defaultMaskTex;
#endif

    private const int LOW_SRC_INDEX = 0;
    private const int HIGH_SRC_INDEX = 1;
    private const int NOISE_SRC_INDEX = 2;
    private const int MASK_SRC_INDEX = 3;

    private TextureProvider m_LowTex = null;
    public TextureProvider lowTexture {
        get { return m_LowTex; }
        set {
            if (m_LowTex == value)
                return;

            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("WaterTexture: Low Texture Pipeline Output is Full.");
                return;
            }

            if (m_LowTex)
                TextureProvider.Unlink(m_LowTex, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, LOW_SRC_INDEX);
            
            m_LowTex = value;
        }
    }

    private TextureProvider m_HighTex = null;
    public TextureProvider highTexture {
        get { return m_HighTex; }
        set {
            if (m_HighTex == value)
                return;
            
            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("WaterTexture: High Texture Pipeline Output is Full.");
                return;
            }

            if (m_HighTex)
                TextureProvider.Unlink(m_HighTex, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, HIGH_SRC_INDEX);

            m_HighTex = value;
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
    private Material m_PCAWaterMaterial;

    new void Awake()
    {
        base.Awake();

        m_PCAWaterMaterial = new Material(Shader.Find("Compute/PCAWater"));
    }

    new void Start()
    {
        base.Start();

#if UNITY_EDITOR
        if (defaultLowTex)
            lowTexture = defaultLowTex;
        if (defaultHighTex)
            highTexture = defaultHighTex;
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

        m_PCAWaterMaterial.SetTexture("_LowTex", m_LowTex.GetTexture());
        m_PCAWaterMaterial.SetTexture("_HighTex", m_HighTex.GetTexture());
        m_PCAWaterMaterial.SetTexture("_MaskTex", m_MaskTex.GetTexture());

        m_RenderTexture.DiscardContents();
        Graphics.Blit(m_NoiseTex.GetTexture(), m_RenderTexture, m_PCAWaterMaterial);

        return true;
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public void Setup()
    {
        if (m_RenderTexture)
            m_RenderTexture.Release();

        Texture noiseTex = m_NoiseTex.GetTexture();

        m_RenderTexture = new RenderTexture(noiseTex.width, noiseTex.height, 0, RenderTextureFormat.ARGB32);
        m_RenderTexture.useMipMap = false;
        m_RenderTexture.wrapMode = TextureWrapMode.Repeat;
        m_RenderTexture.filterMode = FilterMode.Bilinear;
    }

}