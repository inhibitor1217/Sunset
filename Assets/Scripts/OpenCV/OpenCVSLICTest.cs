using UnityEngine;

public class OpenCVSLICTest : MonoBehaviour
{

    public Texture2D inTex;
    [SerializeField]
    private Texture2D outTex;

    void Awake()
    {
        if (inTex)
        {
            Debug.Log(inTex.width + ", " + inTex.height);
            RenderTexture renderTex = RenderTexture.GetTemporary(
                inTex.width,
                inTex.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(inTex, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableTex = new Texture2D(inTex.width, inTex.height);
            readableTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableTex.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            outTex = new Texture2D(inTex.width, inTex.height);
            OpenCVSLIC.SLIC(readableTex, outTex);
        }
    }

}