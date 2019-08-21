using UnityEngine;

public class SaveTextureButton : MonoBehaviour
{

    public TextureProvider textureProvider;
    public bool isRenderTexture;
    public string savePath;

    public void OnClick()
    {
        if (textureProvider && savePath != "")
        {
            if (isRenderTexture)
                ImageIO.SaveRenderTextureToImage(savePath, textureProvider.GetTexture() as RenderTexture);
            else
                ImageIO.SaveTextureToImage(savePath, textureProvider.GetTexture() as Texture2D);
        }
    }

}