using UnityEngine;
using System.Threading;
public static class OpenCVSLIC
{

    private static bool _asyncBusy = false;
    public static bool asyncBusy { get { return _asyncBusy; } }

    private static Thread m_Thread;

    public static void AsyncSLIC(Texture2D inTex, ref int[] outLabel, ref byte[] outContour)
    {
        if (!_asyncBusy)
        {
            _asyncBusy = true;

            int width = inTex.width;
            int height = inTex.height;
            Color32[] inColors = inTex.GetPixels32();

            int[] _outLabel = new int[width * height];
            byte[] _outContour = new byte[width * height];

            outLabel = _outLabel;
            outContour = _outContour;

            m_Thread = new Thread(() => SLIC(inColors, width, height, _outLabel, _outContour));
            m_Thread.Start();
        }
    }
    static void SLIC(Color32[] inColors, int width, int height, int[] outLabel, byte[] outContour)
    {
        Debug.Log("OpenCV SLIC - Input Dimension [" + width + ", " + height + "]");

        int numSuperpixels = OpenCVLibAdapter.OpenCV_processSLIC(
            Color32ToOpenCVMat(inColors), width, height, 
            outLabel, outContour
        );

        Debug.Log("OpenCV SLIC - # Superpixels: " + numSuperpixels);

        _asyncBusy = false;
    }

    public static byte[] Color32ToOpenCVMat(Color32[] colors)
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

    public static Color32[] OpenCVMatToColor32(byte[] bytes)
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