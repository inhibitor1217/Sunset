using UnityEngine;

public class StaticTexture: TextureProvider
{

    public Texture2D staticTexture;
    
    private Texture2D m_ReadableTexture;
    private RenderTexture m_BlurredTexture;

    private Material m_BlurMaterial;
    private int m_HorizontalBlurPass;
    private int m_VerticalBlurPass;

    new void Awake()
    {
        base.Awake();

        m_BlurMaterial = new Material(Shader.Find("Compute/Blur"));
        m_BlurMaterial.SetFloat("_BlurSize", .05f);
        m_HorizontalBlurPass = m_BlurMaterial.FindPass("Horizontal");
        m_VerticalBlurPass   = m_BlurMaterial.FindPass("Vertical");
    }

    public override bool Draw()
    {
        return true;
    }

    public override Texture GetTexture()
    {
        return staticTexture;
    }

    public override string GetProviderName()
    {
        return "StaticTexture";
    }

    public Texture2D GetReadableTexture()
    {
        if (m_ReadableTexture)
            return m_ReadableTexture;
        
        if (staticTexture == null)
            return null;

        if (staticTexture.isReadable)
            return staticTexture;
        
        int width  = staticTexture.width;
        int height = staticTexture.height;

        // Copy texture to readable texture
        RenderTexture tempBuffer = RenderTexture.GetTemporary(
            width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear
        );
        Graphics.Blit(staticTexture, tempBuffer);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tempBuffer;

        m_ReadableTexture = new Texture2D(width, height);
        m_ReadableTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        m_ReadableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tempBuffer);

        return m_ReadableTexture;
    }

    public Texture GetBlurredTexture()
    {
        if (m_BlurredTexture)
            return m_BlurredTexture;

        m_BlurredTexture = new RenderTexture(staticTexture.width / 4, staticTexture.height / 4, 0, RenderTextureFormat.ARGB32);
        m_BlurredTexture.wrapMode = TextureWrapMode.Mirror;
        m_BlurredTexture.filterMode = FilterMode.Bilinear;
        Graphics.Blit(staticTexture, m_BlurredTexture, m_BlurMaterial, m_HorizontalBlurPass);

        return m_BlurredTexture;
    }

    public void SetStaticTexture(Texture2D texture)
    {
        staticTexture = texture;

        staticTexture.wrapMode = TextureWrapMode.Clamp;
        staticTexture.filterMode = FilterMode.Point;

        m_ReadableTexture = null;
        m_BlurredTexture = null;
        
        textureShouldUpdate = true;
    }

    public void SetFilterMode(FilterMode mode)
    {
        staticTexture.filterMode = mode;
    }

}