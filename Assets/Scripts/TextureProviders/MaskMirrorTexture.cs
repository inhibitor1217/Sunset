using UnityEngine;

public class MaskMirrorTexture : TextureProvider
{

    private const int SOURCE_INDEX = 0;

    private BlurTexture m_SrcTexture = null;
    public BlurTexture sourceTexture {
        get { return m_SrcTexture; }
        set {
            if (m_SrcTexture == value)
                return;
            
            if (value && value.SeekFreeIndex() == -1)
            {
                Debug.Log("MaskMirrorTexture: Source Texture Pipeline Output is Full.");
                return;
            }

            if (m_SrcTexture)
                TextureProvider.Unlink(m_SrcTexture, this);
            if (value)
                TextureProvider.Link(value, value.SeekFreeIndex(), this, SOURCE_INDEX);
            
            m_SrcTexture = value;
        }
    }

    [SerializeField]
    private Texture2D m_MirrorTexture;

    public override Texture GetTexture()
    {
        return m_MirrorTexture;
    }

    public override bool Draw()
    {
        if (!m_MirrorTexture)
            return false;

        Texture2D src = m_SrcTexture.GetReadableTexture();
        Color32[] colors = src.GetPixels32();
        Color[] mirror = new Color[m_MirrorTexture.width * m_MirrorTexture.height];
        float[] boundary = new float[m_MirrorTexture.width];
        for (int i = 0; i < m_MirrorTexture.width; i++)
            boundary[i] = 1f; // Initialize

        for (int y = m_MirrorTexture.height - 1; y >= 0; y--)
            for (int x = 0; x < m_MirrorTexture.width; x++)
            {
                if (colors[x + y * src.width].r < 0.01)
                {
                    boundary[x] = (float)y / (float)m_MirrorTexture.height;
                    mirror[x + y * m_MirrorTexture.width] = new Color(0, 0, 0, 0);
                }
                else
                    mirror[x + y * m_MirrorTexture.width] = new Color(boundary[x], 0, 0, 1);
            }
        
        m_MirrorTexture.SetPixels(mirror);
        m_MirrorTexture.Apply();

        return true;
    }

    public void Setup()
    {
        Texture srcTex = m_SrcTexture.GetTexture();
        m_MirrorTexture = new Texture2D(srcTex.width, srcTex.height, TextureFormat.R16, false);
    }

}