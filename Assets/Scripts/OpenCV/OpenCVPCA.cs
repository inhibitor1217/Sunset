using UnityEngine;
using System.Threading;

public static class OpenCVPCA
{

    public static bool asyncBusy { get; private set; } = false;

    public static bool AsyncPCA(Texture2D imgTex, Texture2D maskTex, Texture2D labelTex, 
        int levels, float[] paletteArray)
    {
        if (!asyncBusy)
        {
            asyncBusy = true;

            Color32[] imgColors = imgTex.GetPixels32();
            Color32[] maskColors = maskTex.GetPixels32();
            Color32[] labelColors = labelTex.GetPixels32();

            int width  = imgTex.width;
            int height = imgTex.height;

            new Thread(() => OctavePCA(imgColors, maskColors, labelColors, width, height, levels, paletteArray)).Start();

            return true;
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("OpenCV PCA - Currently busy. Ignoring the invoke.");
#endif        
            return false;
        }
    }

    static void PCA(Color32[] inImage, Color32[] inMask, Color32[] inLabel, int width, int height, float[] outPaletteArray)
    {

        int numValidSegments = OpenCVLibAdapter.OpenCV_processPCA(
            OpenCVUtils.Color32ToOpenCVMat(inImage, OpenCVUtils.CV_8UC4),
            OpenCVUtils.Color32ToOpenCVMat(inMask , OpenCVUtils.CV_8UC1),
            OpenCVUtils.Color32ToOpenCVMat(inLabel, OpenCVUtils.CV_8UC3),
            width, height,
            outPaletteArray
        );

#if UNITY_EDITOR
        Debug.Log("OpenCV PCA - # valid segments = " + numValidSegments);
#endif

        asyncBusy = false;

    }

    static void OctavePCA(Color32[] inImage, Color32[] inMask, Color32[] inLabel, int width, int height, 
        int levels, float[] outPaletteArray)
    {
        OpenCVLibAdapter.OpenCV_processOctavePCA(
            OpenCVUtils.Color32ToOpenCVMat(inImage, OpenCVUtils.CV_8UC4),
            OpenCVUtils.Color32ToOpenCVMat(inMask , OpenCVUtils.CV_8UC1),
            OpenCVUtils.Color32ToOpenCVMat(inLabel, OpenCVUtils.CV_8UC3),
            width, height,
            levels,
            outPaletteArray
        );

        asyncBusy = false;
    }

}