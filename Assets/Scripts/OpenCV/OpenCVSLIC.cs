using UnityEngine;
using System.Threading;
using System.Collections.Generic;
public static class OpenCVSLIC
{

    public static bool asyncBusy { get; private set; } = false;
    public static int numAsyncTasks { get; private set; } = 0;
    public static int asyncProgress { get; private set; } = 0;

    public const int MIN_REGION_SIZE = 8;
    public const float MAX_REGION_RATIO = 32f;
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

            // Copy inTex to readable texture
            RenderTexture tempBuffer = RenderTexture.GetTemporary(
                width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear
            );
            Graphics.Blit(inTex, tempBuffer);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tempBuffer;

            Texture2D readable = new Texture2D(width, height);
            readable.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            readable.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tempBuffer);

            List<int> regionSizes = new List<int>();
            for (var regionSize = MIN_REGION_SIZE; 
                regionSize <= Mathf.Min(width, height) / MAX_REGION_RATIO; 
                regionSize *= 2)
            {
                regionSizes.Add(regionSize);
            }

            numAsyncTasks = regionSizes.Count;

            outLabel = new int[numAsyncTasks][];
            outContour = new byte[numAsyncTasks][];

            for (int i = 0; i < numAsyncTasks; i++)
            {
                // Use mipmap level
                Color32[] inColors = readable.GetPixels32(i);

                int[] _outLabel = new int[inColors.Length];
                byte[] _outContour = new byte[inColors.Length];
                outLabel[i] = _outLabel;
                outContour[i] = _outContour;

                int regionSize = regionSizes[i];
                int levelWidth = width >> i;
                int levelHeight = height >> i;

                new Thread(() => SLIC(inColors, levelWidth, levelHeight, _outLabel, _outContour, regionSize)).Start();
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