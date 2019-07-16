using UnityEngine;
public static class OpenCVSLIC
{

    public static void SLIC(Texture2D inTex, Texture2D outTex)
    {
        Color32[] inColors = inTex.GetPixels32();
        int[] outArray = new int[inTex.width * inTex.height];
        byte[] outContourBytes = new byte[inTex.width * inTex.height];

        int numSuperpixels = OpenCVLibAdapter.OpenCV_processSLIC(Color32ToOpenCVMat(inColors), inTex.width, inTex.height, outArray, outContourBytes);

        Debug.Log(numSuperpixels);

        outTex.SetPixels32(OpenCVMatToColor32(outContourBytes));
        outTex.Apply();
    }

    static byte[] Color32ToOpenCVMat(Color32[] colors)
    {
        byte[] bytes = new byte[colors.Length * 4];
        for (int i = 0; i < colors.Length; i++)
        {
            bytes[4 * i + 0] = colors[i].b;
            bytes[4 * i + 1] = colors[i].g;
            bytes[4 * i + 2] = colors[i].r;
            bytes[4 * i + 3] = colors[i].a;
        }
        return bytes;
    }

    static Color32[] OpenCVMatToColor32(byte[] bytes)
    {
        Color32[] colors = new Color32[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
        {
            byte value = bytes[i] == 0 ? (byte)0 : (byte)255;
            colors[i] = new Color32(value, value, value, 255);
        }
        return colors;
    }

}