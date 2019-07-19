using UnityEngine;

public class MaskTexture : TextureProvider
{

    private Camera m_MaskCamera;
    private RenderTexture m_RenderTexture = null;

    new void OnDestroy()
    {
        base.OnDestroy();

        if (m_RenderTexture)
            m_RenderTexture.Release();
    }

    void Update()
    {
        if (InputMode.Instance.IsModeBrush() && InputManager.Instance.released)
            textureShouldUpdate = true;
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public override bool Draw()
    {
        if (m_RenderTexture)
        {
            return true;
        }

        return false;
    }

    public void SetDimension(Texture texture)
    {
        SetDimension(texture.width, texture.height);
    }

    public void SetDimension(int width, int height)
    {
        m_RenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
        m_RenderTexture.antiAliasing = 4;
        m_RenderTexture.wrapMode = TextureWrapMode.Clamp;
        m_RenderTexture.filterMode = FilterMode.Point;
    }

    public void SetCamera(Camera camera)
    {
        m_MaskCamera = camera;
        m_MaskCamera.targetTexture = m_RenderTexture;
    }

}
