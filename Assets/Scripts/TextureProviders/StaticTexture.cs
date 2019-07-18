using UnityEngine;

public class StaticTexture: TextureProvider
{

    public Texture2D staticTexture;

    private Texture2D m_Copy;

    public override bool Draw()
    {
        return true;
    }

    public void SetStaticTexture(Texture2D texture)
    {
        staticTexture = texture;

        m_Copy = new Texture2D(staticTexture.width, staticTexture.height, staticTexture.format, true);
        Graphics.CopyTexture(staticTexture, m_Copy);
        
        this.texture = m_Copy;
        
        textureShouldUpdate = true;
    }

    new void Awake()
    {
        base.Awake();
        if (staticTexture)
            SetStaticTexture(staticTexture);
    }

}