using System.Runtime.InteropServices;
using UnityEngine;

public static class OpenCVLibAdapter
{
#if UNITY_ANDROID && !UNITY_EDITOR

#else
    [DllImport("OpenCVNativePlugin")]
    private extern static int processSLIC(byte[] inputArray, int width, int height, 
        int[] outputLabelArray, byte[] outputContourArray, int algorithm, int region_size, float ruler);

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
#endif
}