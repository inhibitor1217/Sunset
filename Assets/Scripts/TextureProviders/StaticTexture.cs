using UnityEngine;

public class StaticTexture: TextureProvider
{

    public Texture2D staticTexture;

    private Texture2D m_Copy;

    public override bool Draw()
    {
        return true;
    }

    public override Texture GetTexture()
    {
        return m_Copy;
    }

    public void SetStaticTexture(Texture2D texture)
    {
        staticTexture = texture;

        m_Copy = new Texture2D(staticTexture.width, staticTexture.height, staticTexture.format, false);
        Graphics.CopyTexture(staticTexture, m_Copy);

        m_Copy.wrapMode = TextureWrapMode.Clamp;
        m_Copy.filterMode = FilterMode.Point;
        
        textureShouldUpdate = true;
    }

}