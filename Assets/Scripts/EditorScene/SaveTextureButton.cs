using UnityEngine;

public class SaveTextureButton : MonoBehaviour
{

    public TextureProvider textureProvider;
    public string savePath;

    public void OnClick()
    {
        if (textureProvider && savePath != "")
        {
            ImageIO.SaveRenderTextureToImage(savePath, textureProvider.GetTexture() as RenderTexture);
        }
    }

}