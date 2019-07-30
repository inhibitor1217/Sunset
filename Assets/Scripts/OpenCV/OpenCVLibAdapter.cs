using System.Runtime.InteropServices;

public static class OpenCVLibAdapter
{
#if UNITY_ANDROID && !UNITY_EDITOR
    [DllImport("OpenCVAndroidPlugin")]
    private extern static int processSLIC(byte[] inputArray, int width, int height, 
        int[] outputLabelArray, byte[] outputContourArray, int algorithm, int region_size, float ruler);
#else
    [DllImport("OpenCVNativePlugin")]
    private extern static int processSLIC(byte[] inputArray, int width, int height, 
        int[] outputLabelArray, byte[] outputContourArray, int algorithm, int region_size, float ruler);
    [DllImport("OpenCVNativePlugin")]
    private extern static int processPCA(byte[] inImageArray, byte[] inMaskArray, byte[] inLabelArray, 
        int width, int height, float[] outPaletteArray);
    [DllImport("OpenCVNativePlugin")]
    private extern static void processOctavePCA(byte[] inImageArray, byte[] inMaskArray, byte[] inLabelArray, 
        int width, int height, int levels, float[] outPaletteArray);
#endif
    public const int SLICAlgorithm__SLIC = 100;
    public const int SLICAlgorithm__SLICO = 101;
    public const int SLICAlgorithm__MSLIC = 102;

    public static int OpenCV_processSLIC(
        byte[] inputArray, int width, int height, int[] outputLabelArray, byte[] outputContourArray,
        int algorithm=SLICAlgorithm__SLICO, int region_size=10, float ruler=10f
    )
    {
        return processSLIC(inputArray, width, height, outputLabelArray, outputContourArray, algorithm, region_size, ruler);
    }

    public static int OpenCV_processPCA(
        byte[] inImageArray, byte[] inMaskArray, byte[] inLabelArray, int width, int height, float[] outPaletteArray
    )
    {
        return processPCA(inImageArray, inMaskArray, inLabelArray, width, height, outPaletteArray);
    }

    public static void OpenCV_processOctavePCA(
        byte[] inImageArray, byte[] inMaskArray, byte[] inLabelArray, int width, int height, int levels, float[] outPaletteArray
    )
    {
        processOctavePCA(inImageArray, inMaskArray, inLabelArray, width, height, levels, outPaletteArray);
    }
}