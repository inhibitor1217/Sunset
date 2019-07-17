using UnityEngine;

public class OpenCVSLICClient : MonoBehaviour
{

    public Texture2D inTex;

    private Texture2D m_ReadableTex;

    private int[][] m_OutLabel = null;
    private byte[][] m_OutContour = null;

    private int m_InTexWidth;
    private int m_InTexHeight;
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

        MessagePanel.Instance.ShowMessage("이미지 전처리 중...");

        if (!inTex.isReadable)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                m_InTexWidth,
                m_InTexHeight,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(inTex, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            
            m_ReadableTex = new Texture2D(m_InTexWidth, m_InTexHeight);
            m_ReadableTex.ReadPixels(new Rect(0, 0, m_InTexWidth, m_InTexHeight), 0, 0);
            m_ReadableTex.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
        }

        OpenCVSLIC.AsyncSLIC(inTex.isReadable ? inTex : m_ReadableTex, ref m_OutLabel, ref m_OutContour);

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
            MessagePanel.Instance.ShowMessage("OpenCV - SLIC 이미지 처리 중... (" + OpenCVSLIC.asyncProgress + "/" + OpenCVSLIC.numAsyncTasks + ")");
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

            MessagePanel.Instance.Disable();

        }
    }

}