using UnityEngine;

public class OpenCVSLICClient : MonoBehaviour
{

    public Texture2D inTex;
    public SLICLabelTexture labelTextureProvider;
    public SLICContourTexture contourTextureProvider;

    private Texture2D m_ReadableTex;

    private int[][] m_OutLabel = null;
    private byte[][] m_OutContour = null;

    private int m_InTexWidth;
    public int TexWidth { get { return m_InTexWidth; } }
    private int m_InTexHeight;
    public int TexHeight { get { return m_InTexHeight; } }
    private int m_NumLevels;
    public int NumLevels { get { return m_NumLevels; } }
    private bool m_Invoked = false;
    private float m_InvokedTime;
    private int m_prevProgress;

    public void onClick()
    {
        if (inTex)
            Invoke();
    }

    public bool Invoke()
    {
        if (OpenCVSLIC.asyncBusy)
            return false;

        m_InTexWidth = inTex.width;
        m_InTexHeight = inTex.height;

        if (!inTex.isReadable)
        {
            // RenderTexture renderTex = RenderTexture.GetTemporary(
            //     m_InTexWidth,
            //     m_InTexHeight,
            //     0,
            //     RenderTextureFormat.Default,
            //     RenderTextureReadWrite.Linear
            // );

            // Graphics.Blit(inTex, renderTex);
            // RenderTexture previous = RenderTexture.active;
            // RenderTexture.active = renderTex;
            
            // m_ReadableTex = new Texture2D(m_InTexWidth, m_InTexHeight);
            // m_ReadableTex.ReadPixels(new Rect(0, 0, m_InTexWidth, m_InTexHeight), 0, 0);
            // m_ReadableTex.Apply();
            
            // RenderTexture.active = previous;
            // RenderTexture.ReleaseTemporary(renderTex);
            return false;
        }

        m_NumLevels = OpenCVSLIC.AsyncSLIC(inTex.isReadable ? inTex : m_ReadableTex, ref m_OutLabel, ref m_OutContour);

        m_Invoked = true;
        m_InvokedTime = Time.time;
        m_prevProgress = -1;

        return true;
    }

    public int[] getLabel(int level)
    {
        return m_OutLabel[level];
    }

    public int getLabelAt(int level, int x, int y)
    {
        return m_OutLabel[level][y * m_InTexWidth + x];
    }

    public byte[] getContour(int level)
    {
        return m_OutContour[level];
    }

    void Update()
    {
        if (m_Invoked && m_prevProgress != OpenCVSLIC.asyncProgress)
        {
            MessagePanel.Instance.ShowMessage("OpenCV - SLIC 이미지 처리 중... (" + OpenCVSLIC.asyncProgress + "/" + m_NumLevels + ")");
            m_prevProgress = OpenCVSLIC.asyncProgress;
        }

        // Busy wait
        if (m_Invoked && !OpenCVSLIC.asyncBusy)
        {
            // Job finished
            m_Invoked = false;
            m_prevProgress = -1;

#if UNITY_EDITOR
            Debug.Log("OpenCVSLICClient - Finished AsyncSLIC in " + (Time.time - m_InvokedTime) + " seconds.");
#endif
            if (labelTextureProvider && labelTextureProvider.isActiveAndEnabled)
                labelTextureProvider.GenerateTextures(this);
            if (contourTextureProvider && contourTextureProvider.isActiveAndEnabled)
                contourTextureProvider.GenerateTextures(this);
            MessagePanel.Instance.Disable();

        }
    }

}