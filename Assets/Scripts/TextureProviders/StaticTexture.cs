using UnityEngine;

public class StaticTexture: TextureProvider
{

    public Texture2D staticTexture;
    
    private Texture2D m_ReadableTexture;

    public override bool Draw()
    {
        return true;
    }

    public override Texture GetTexture()
    {
        return staticTexture;
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

    public void SetStaticTexture(Texture2D texture)
    {
        staticTexture = texture;

        staticTexture.wrapMode = TextureWrapMode.Clamp;
        staticTexture.filterMode = FilterMode.Point;

        m_ReadableTexture = null;
        
        textureShouldUpdate = true;
    }

}