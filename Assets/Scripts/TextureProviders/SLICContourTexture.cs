using UnityEngine;

public class SLICContourTexture : TextureProvider
{

    [SerializeField]
    private Texture2D[] m_contourTextures = null;

    private int m_PrevLevel = -1;

    public override bool Draw()
    {
        return true;
    }

    public void GenerateTextures(OpenCVSLICClient client)
    {
        m_contourTextures = new Texture2D[client.NumLevels];
        m_PrevLevel = -1;

        for (int level = 0; level < client.NumLevels; level++)
        {
            m_contourTextures[level] = new Texture2D(client.TexWidth, client.TexHeight);
            m_contourTextures[level].SetPixels32(OpenCVUtil.OpenCVMatToColor32(client.getContour(level)));
            m_contourTextures[level].Apply();
        }
    }

    new void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        if (m_contourTextures != null && m_contourTextures.Length > 0)
        {
            float scaleFactor = 1f - Mathf.Clamp(Mathf.Log10(InputManager.Instance.MultiplicativeScale) / InputManager.MAX_SCALE_LOG, 0f, 1f);
            int level = Mathf.Clamp(Mathf.FloorToInt(scaleFactor * m_contourTextures.Length), 0, m_contourTextures.Length - 1);

            if (m_PrevLevel != level)
            {
                texture = m_contourTextures[level];
                m_PrevLevel = level;
            }
        }
    }

}