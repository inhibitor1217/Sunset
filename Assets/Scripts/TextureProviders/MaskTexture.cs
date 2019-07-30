using UnityEngine;

public class MaskTexture : TextureProvider
{

    private Camera m_MaskCamera;
    private RenderTexture m_RenderTexture = null;

    [HideInInspector]
    public bool dirty = false;
    [HideInInspector]
    public int mode;

    new void OnDestroy()
    {
        base.OnDestroy();

        if (m_RenderTexture)
            m_RenderTexture.Release();
    }

    void Update()
    {
        if (InputMode.Instance.isBrush()
            && InputMode.Instance.isMode(mode)
            && InputManager.Instance.withinContainer 
            && InputManager.Instance.released)
        {
            textureShouldUpdate = true;
            dirty = true;
        }
    }

    public override Texture GetTexture()
    {
        return m_RenderTexture;
    }

    public Texture2D GetReadableTexture()
    {
        int width  = m_RenderTexture.width;
        int height = m_RenderTexture.height;

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = m_RenderTexture;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        RenderTexture.active = prev;

        return tex;
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
        m_RenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.R8);
        m_RenderTexture.useMipMap = false;
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
