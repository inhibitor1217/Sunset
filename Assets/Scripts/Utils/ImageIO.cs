using UnityEngine;

public static class ImageIO
{

    public static void SaveRenderTextureToImage(string path, RenderTexture renderTexture)
    {
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = renderTexture;

        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        RenderTexture.active = prev;

        SaveTextureToImage(path, texture);
    }

    public static void SaveTextureToImage(string path, Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }

}