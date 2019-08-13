using UnityEngine;
using System.Threading;
public static class OpenCVSLIC
{

    public static bool asyncBusy { get; private set; } = false;

    public const int REGION_SIZE = 16;
    public const float RULER = 10f;

    public static bool AsyncSLIC(Texture2D inTex, ref int[] outLabel, ref byte[] outContour)
    {
        if (!asyncBusy)
        {
            asyncBusy = true;

            int width = inTex.width;
            int height = inTex.height;

#if UNITY_EDITOR
            Debug.Log("OpenCV SLIC - Input Dimension [" + width + ", " + height + "]");
#endif

            Color32[] inColors = inTex.GetPixels32();

            int[] _outLabel = new int[inColors.Length];
            byte[] _outContour = new byte[inColors.Length];

            outLabel = _outLabel;
            outContour = _outContour;

            new Thread(() => SLIC(inColors, width, height, _outLabel, _outContour, REGION_SIZE)).Start();

            return true;
        }

        return false;
    }
    static void SLIC(Color32[] inColors, int width, int height, int[] outLabel, byte[] outContour, int regionSize)
    {
        int numSuperpixels = OpenCVLibAdapter.OpenCV_processSLIC(
            OpenCVUtils.Color32ToOpenCVMat(inColors, OpenCVUtils.CV_8UC4), width, height, 
            outLabel, outContour,
            OpenCVLibAdapter.SLICAlgorithm__SLIC,
            regionSize,
            RULER
        );

#if UNITY_EDITOR
        Debug.Log("OpenCV SLIC - Processed " + width + " x " + height + " input image");
        Debug.Log("OpenCV SLIC - # Superpixels: " + numSuperpixels);
#endif

        asyncBusy = false;
    }

}