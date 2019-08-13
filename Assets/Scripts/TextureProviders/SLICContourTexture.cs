using UnityEngine;

public class SLICContourTexture : TextureProvider
{

    private Texture2D m_Texture = null;

    public override bool Draw()
    {
        return true;
    }

    public override Texture GetTexture()
    {
        return m_Texture;
    }

    public override string GetProviderName()
    {
        return "SLICContourTexture";
    }

    public void GenerateTextures(OpenCVSLICData data)
    {
        // m_contourTextures = new Texture2D[data.levels];
        // m_PrevLevel = -1;

        // for (int level = 0; level < data.levels; level++)
        // {
        //     m_contourTextures[level] = new Texture2D(data.getWidth(level), data.getHeight(level), TextureFormat.ARGB32, false);
        //     m_contourTextures[level].SetPixels32(OpenCVUtils.OpenCVContourToColor32(data.outContour[level]));
        //     m_contourTextures[level].Apply();

        //     m_contourTextures[level].wrapMode = TextureWrapMode.Clamp;
        //     m_contourTextures[level].filterMode = FilterMode.Point;
        // }

        m_Texture = new Texture2D(data.width, data.height, TextureFormat.ARGB32, false);
        m_Texture.SetPixels32(OpenCVUtils.OpenCVContourToColor32(data.outContour));
        m_Texture.Apply();
        m_Texture.wrapMode = TextureWrapMode.Clamp;
        m_Texture.filterMode = FilterMode.Point;
    }

    // void Update()
    // {
    //     if (m_contourTextures != null && m_contourTextures.Length > 0)
    //     {
    //         float scaleFactor = 1f - Mathf.Clamp(Mathf.Log10(InputManager.Instance.MultiplicativeScale) / InputManager.MAX_SCALE_LOG, 0f, 1f);
    //         int level = Mathf.Clamp(Mathf.FloorToInt(scaleFactor * m_contourTextures.Length), 0, m_contourTextures.Length - 1);

    //         if (m_PrevLevel != level)
    //         {
    //             m_CurTexture = m_contourTextures[level];
    //             if (m_Target)
    //             {
    //                 m_Target.SetTexture(m_CurTexture);
    //                 m_Target.globalScale = Mathf.Pow(2f, level);
    //             }
    //             m_PrevLevel = level;
    //         }
    //     }
    // }

}