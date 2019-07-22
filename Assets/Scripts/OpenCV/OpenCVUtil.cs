using UnityEngine;

public static class OpenCVUtil
{

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

    public static Color32[] OpenCVLabelToColor32(int[] labels)
    {
        Color32[] colors = new Color32[labels.Length];
        for (int i = 0; i < labels.Length; i++)
        {
            colors[i] = new Color32(
                (byte)((labels[i] >> 24) & 0xFF),
                (byte)((labels[i] >> 16) & 0xFF),
                (byte)((labels[i] >>  8) & 0xFF),
                (byte)((labels[i]      ) & 0xFF)
            );
        }
        return colors;
    }

}