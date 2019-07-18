using UnityEngine;

public class SLICLabelTexture : TextureProvider
{

    [SerializeField]
    private Texture2D[] m_LabelTextures = null;

    private int m_PrevLevel = -1;

    public override bool Draw()
    {
        return true;
    }

    public void GenerateTextures(OpenCVSLICClient client)
    {
        m_LabelTextures = new Texture2D[client.NumLevels];
        m_PrevLevel = -1;

        for (int level = 0; level < client.NumLevels; level++)
        {
            m_LabelTextures[level] = new Texture2D(client.TexWidth, client.TexHeight);
            m_LabelTextures[level].SetPixels32(OpenCVUtils.OpenCVLabelToColor32(client.getLabel(level)));
            m_LabelTextures[level].Apply();
        }
    }

    new void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        if (m_LabelTextures != null && m_LabelTextures.Length > 0)
        {
            float scaleFactor = 1f - Mathf.Clamp(Mathf.Log10(InputManager.Instance.MultiplicativeScale) / InputManager.MAX_SCALE_LOG, 0f, 1f);
            int level = Mathf.Clamp(Mathf.FloorToInt(scaleFactor * m_LabelTextures.Length), 0, m_LabelTextures.Length - 1);

            if (m_PrevLevel != level)
            {
                texture = m_LabelTextures[level];
                m_PrevLevel = level;
            }
        }
    }

}