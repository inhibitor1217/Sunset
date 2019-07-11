using UnityEngine;

public class StaticTexture: TextureProvider
{

    public Texture2D staticTexture;

    private Texture2D m_Copy;

    public override bool Draw()
    {
        return true;
    }

    void Awake()
    {
        base.Awake();
        m_Copy = new Texture2D(staticTexture.width, staticTexture.height, staticTexture.format, true);
    }

    void Start()
    {
        Graphics.CopyTexture(staticTexture, m_Copy);
        texture = m_Copy;
    }

}