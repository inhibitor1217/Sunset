using UnityEngine;
using System.Threading;
using System.Collections.Generic;
public static class OpenCVSLIC
{

    private static bool _asyncBusy = false;
    public static bool asyncBusy { get { return _asyncBusy; } }

    private static int _numAsyncTasks = 0;
    public static int numAsyncTasks { get { return _numAsyncTasks; } }

    private static int _asyncProgress = 0;
    public static int asyncProgress { get { return _asyncProgress; } }

    public static void AsyncSLIC(Texture2D inTex, ref int[][] outLabel, ref byte[][] outContour)
    {
        if (!_asyncBusy)
        {
            _asyncBusy = true;
            _asyncProgress = 0;

            int width = inTex.width;
            int height = inTex.height;
            Color32[] inColors = inTex.GetPixels32();

            List<int> regionSizes = new List<int>();
            for (int regionSize = 5; regionSize <= Mathf.Min(width, height) / 8; regionSize *= 2)
            {
                regionSizes.Add(regionSize);
            }

            _numAsyncTasks = regionSizes.Count;

            outLabel = new int[_numAsyncTasks][];
            outContour = new byte[_numAsyncTasks][];

            for (int i = 0; i < _numAsyncTasks; i++)
            {
                int[] _outLabel = new int[width * height];
                byte[] _outContour = new byte[width * height];
                outLabel[i] = _outLabel;
                outContour[i] = _outContour;

                new Thread(() => SLIC(inColors, width, height, _outLabel, _outContour, regionSizes[i])).Start();
            }
        }
    }
    static void SLIC(Color32[] inColors, int width, int height, int[] outLabel, byte[] outContour, int regionSize)
    {
#if UNITY_EDITOR
        Debug.Log("OpenCV SLIC - Input Dimension [" + width + ", " + height + "]");
#endif

        int numSuperpixels = OpenCVLibAdapter.OpenCV_processSLIC(
            Color32ToOpenCVMat(inColors), width, height, 
            outLabel, outContour,
            OpenCVLibAdapter.SLICAlgorithm__SLIC,
            regionSize
        );

#if UNITY_EDITOR
        Debug.Log("OpenCV SLIC - # Superpixels: " + numSuperpixels);
#endif

        _asyncProgress++;
        if (_asyncProgress == _numAsyncTasks)
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
            if (bytes[i] == 0)
                colors[i] = new Color32(0, 0, 0, 0);
            else
                colors[i] = new Color32(255, 0, 0, 255);
        }
        return colors;
    }

}