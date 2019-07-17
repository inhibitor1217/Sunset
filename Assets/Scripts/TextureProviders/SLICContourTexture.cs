using UnityEngine;

public class SLICContourTexture : TextureProvider
{

    public OpenCVSLICClient SLICClient;

    public override bool Draw()
    {
        return true;
    }



}