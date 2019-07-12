using UnityEngine;

public class MaskTexture : TextureProvider
{

    public Camera maskCamera;

    private RenderTexture m_RenderTexture = null;
    
    new void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        // TEMP
        SetDimension(852, 480);
    }

    void Update()
    {
        textureShouldUpdate = true;
    }

    public override bool Draw()
    {
        if (m_RenderTexture)
        {
            return true;
        }

        return false;
    }

    public void SetDimension(int width, int height)
    {
        m_RenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
        m_RenderTexture.antiAliasing = 4;

        if (maskCamera)
        {
            maskCamera.targetTexture = m_RenderTexture;
        }
        texture = m_RenderTexture;
    }

}
