using UnityEngine;

public static class OpenCVUtils
{

    public const int CV_8UC1 = 0;
    public const int CV_8UC2 = 1;
    public const int CV_8UC3 = 2;
    public const int CV_8UC4 = 3;

    public static byte[] Color32ToOpenCVMat(Color32[] colors, int matType)
    {
        byte[] bytes = null;

        switch (matType)
        {
        case CV_8UC1:
            bytes = new byte[colors.Length];
            break;
        case CV_8UC2:
            bytes = new byte[colors.Length * 2];
            break;
        case CV_8UC3:
            bytes = new byte[colors.Length * 3];
            break;
        case CV_8UC4:
            bytes = new byte[colors.Length * 4];
            break;
        default:
            break;
        }

        for (int i = 0; i < colors.Length; i++)
        {
            switch (matType)
            {
            case CV_8UC1:
                bytes[i] = colors[i].r;
                break;
            case CV_8UC2:
                bytes[2 * i + 0] = colors[i].r;
                bytes[2 * i + 1] = colors[i].g;
                break;
            case CV_8UC3:
                bytes[3 * i + 0] = colors[i].b;
                bytes[3 * i + 1] = colors[i].g;
                bytes[3 * i + 2] = colors[i].r;
                break;
            case CV_8UC4:
                bytes[4 * i + 0] = colors[i].b;
                bytes[4 * i + 1] = colors[i].g;
                bytes[4 * i + 2] = colors[i].r;
                bytes[4 * i + 3] = colors[i].a;
                break;
            default:
                break;
            }
        }

        return bytes;
    }

    public static Color32[] OpenCVContourToColor32(byte[] bytes)
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

    public static Color32[] OpenCVLabelToColor32(int[] labels)
    {
        Color32[] colors = new Color32[labels.Length];
        for (int i = 0; i < labels.Length; i++)
        {
            colors[i] = new Color32(
                (byte)((labels[i] >> 16) & 0xFF),
                (byte)((labels[i] >>  8) & 0xFF),
                (byte)((labels[i]      ) & 0xFF),
                (byte)((labels[i] >> 24) & 0xFF)
            );
        }
        return colors;
    }

}