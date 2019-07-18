using UnityEngine;

public class MaskRendererCamera
 : MonoBehaviour
{
    
    private Material m_Material;

    void Awake()
    {
        m_Material = new Material(Shader.Find("Compute/MaskRenderer"));
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        RenderTexture temp = RenderTexture.GetTemporary(src.width, src.height, src.depth, src.format);
        Graphics.Blit(dst, temp);
        m_Material.SetTexture("_PrevTex", temp);
        Graphics.Blit(src, dst, m_Material);
        RenderTexture.ReleaseTemporary(temp);
    }

}
