using UnityEngine;
using System.Threading;
public static class OpenCVSLIC
{

    public static bool asyncBusy { get; private set; } = false;
    public static int numAsyncTasks { get; private set; } = 0;
    public static int asyncProgress { get; private set; } = 0;

    public const int MIN_REGION_SIZE = 16;
    public const float MAX_REGION_RATIO = 16f;
    public const float RULER = 10f;

    public static int AsyncSLIC(Texture2D inTex, ref int[][] outLabel, ref byte[][] outContour)
    {
        if (!asyncBusy)
        {
            asyncBusy = true;

            int width = inTex.width;
            int height = inTex.height;

#if UNITY_EDITOR
            Debug.Log("OpenCV SLIC - Input Dimension [" + width + ", " + height + "]");
#endif

            numAsyncTasks = 0;
            for (var regionSize = MIN_REGION_SIZE; 
                regionSize <= Mathf.Min(width, height) / MAX_REGION_RATIO; 
                regionSize *= 2)
            {
                numAsyncTasks++;
            }

            outLabel = new int[numAsyncTasks][];
            outContour = new byte[numAsyncTasks][];

            for (int i = 0; i < numAsyncTasks; i++)
            {
                // Use mipmap level
                Color32[] inColors = inTex.GetPixels32(i);

                int[] _outLabel = new int[inColors.Length];
                byte[] _outContour = new byte[inColors.Length];
                outLabel[i] = _outLabel;
                outContour[i] = _outContour;

                int levelWidth = width >> i;
                int levelHeight = height >> i;

                new Thread(() => SLIC(inColors, levelWidth, levelHeight, _outLabel, _outContour, MIN_REGION_SIZE)).Start();
            }

            return numAsyncTasks;
        }

        return -1;
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

        asyncProgress++;
        if (asyncProgress == numAsyncTasks)
        {
            asyncBusy = false;
            asyncProgress = 0;
            numAsyncTasks = 0;
        }
    }

}