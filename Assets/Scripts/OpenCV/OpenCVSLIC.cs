using UnityEngine;
using System.Threading;
using System.Collections.Generic;
public static class OpenCVSLIC
{

    public static bool asyncBusy { get; private set; } = false;
    public static int numAsyncTasks { get; private set; } = 0;
    public static int asyncProgress { get; private set; } = 0;

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

            Color32[] inColors = inTex.GetPixels32();

            List<int> regionSizes = new List<int>();
            for (int regionSize = 4; regionSize <= Mathf.Min(width, height) / 8; regionSize *= 2)
            {
                regionSizes.Add(regionSize);
            }

            numAsyncTasks = regionSizes.Count;

            outLabel = new int[numAsyncTasks][];
            outContour = new byte[numAsyncTasks][];

            for (int i = 0; i < numAsyncTasks; i++)
            {
                int[] _outLabel = new int[width * height];
                byte[] _outContour = new byte[width * height];
                outLabel[i] = _outLabel;
                outContour[i] = _outContour;

                int regionSize = regionSizes[i];

                new Thread(() => SLIC(inColors, width, height, _outLabel, _outContour, regionSize)).Start();
            }

            return numAsyncTasks;
        }

        return -1;
    }
    static void SLIC(Color32[] inColors, int width, int height, int[] outLabel, byte[] outContour, int regionSize)
    {
        int numSuperpixels = OpenCVLibAdapter.OpenCV_processSLIC(
            OpenCVUtils.Color32ToOpenCVMat(inColors), width, height, 
            outLabel, outContour,
            OpenCVLibAdapter.SLICAlgorithm__SLIC,
            regionSize
        );

#if UNITY_EDITOR
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