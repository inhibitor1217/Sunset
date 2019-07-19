using UnityEngine;

public class MaskRendererCamera : MonoBehaviour
{
    
    private Material m_Material;
    private RenderTexture m_LastFrame;

    void Awake()
    {
        m_Material = new Material(Shader.Find("Compute/MaskRenderer"));
    }

    void OnDestroy()
    {
        if (m_LastFrame)
            RenderTexture.ReleaseTemporary(m_LastFrame);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (m_LastFrame)
            m_Material.SetTexture("_PrevTex", m_LastFrame);
        Graphics.Blit(src, dst, m_Material);
        if (!m_LastFrame)
            m_LastFrame = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
        m_LastFrame.DiscardContents();
        Graphics.Blit(dst, m_LastFrame);
    }

}
