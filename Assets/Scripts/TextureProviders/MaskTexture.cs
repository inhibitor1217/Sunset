using UnityEngine;

public class MaskTexture : TextureProvider
{

    public Camera maskCamera;
    public int referenceWidth;
    public int referenceHeight;

    private RenderTexture m_RenderTexture = null;
    
    new void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        SetDimension(referenceWidth, referenceHeight);
    }

    void Update()
    {
        if (InputMode.Instance.IsModeBrush())
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

    public void SetDimension(Texture texture)
    {
        SetDimension(texture.width, texture.height);
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
