public class OpenCVSLICData
{

    public int levels;
    public int width;
    public int height;
    public int[][] outLabel = null;
    public byte[][] outContour = null;

    public int getWidth(int level)
    {
        return width >> level;
    }

    public int getHeight(int level)
    {
        return height >> level;
    }

}