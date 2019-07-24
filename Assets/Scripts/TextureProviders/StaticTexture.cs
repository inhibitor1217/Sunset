using UnityEngine;

public class StaticTexture: TextureProvider
{

    public Texture2D staticTexture;

    public override bool Draw()
    {
        return true;
    }

    public override Texture GetTexture()
    {
        return staticTexture;
    }

    public void SetStaticTexture(Texture2D texture)
    {
        staticTexture = texture;

        staticTexture.wrapMode = TextureWrapMode.Clamp;
        staticTexture.filterMode = FilterMode.Point;
        
        textureShouldUpdate = true;
    }

}