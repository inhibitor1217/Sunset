using UnityEngine;

public class OpenCVSLICClient : MonoBehaviour
{

    public Texture2D inTex;
    [SerializeField]
    private Texture2D outTex;

    private Texture2D m_ReadableTex;

    private int[] m_OutLabel;
    private byte[] m_OutContour;

    private bool m_Invoked = false;
    private float m_InvokedTime;

    void Awake()
    {
        if (inTex)
        {
            // Create Readable Texture
            RenderTexture renderTex = RenderTexture.GetTemporary(
                inTex.width,
                inTex.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(inTex, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            
            m_ReadableTex = new Texture2D(inTex.width, inTex.height);
            m_ReadableTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            m_ReadableTex.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            // (TEMP) Create Output Texture for Display
            outTex = new Texture2D(inTex.width, inTex.height);
        }
    }

    public bool Invoke(Texture2D _inTex, Texture2D _outContour)
    {
        if (OpenCVSLIC.asyncBusy)
            return false;

        int width = _inTex.width;
        int height = _inTex.height;

        OpenCVSLIC.AsyncSLIC(_inTex, ref m_OutLabel, ref m_OutContour);

        m_Invoked = true;
        m_InvokedTime = Time.time;

        return true;
    }

    void Update()
    {
        // Busy wait
        if (m_Invoked && !OpenCVSLIC.asyncBusy)
        {
            // Job finished
            m_Invoked = false;

            outTex.SetPixels32(OpenCVSLIC.OpenCVMatToColor32(m_OutContour));
            outTex.Apply();

            Debug.Log("OpenCVSLICClient - Finished AsyncSLIC in " + (Time.time - m_InvokedTime) + " seconds.");

        }
    }

    public void onClick()
    {
        if (m_ReadableTex)
        {
            Invoke(m_ReadableTex, outTex);
        }
    }

}