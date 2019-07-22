using UnityEngine;

public class OpenCVSLICClient : MonoBehaviour
{

    public SLICLabelTexture labelTextureProvider;
    public SLICContourTexture contourTextureProvider;

    private Texture2D m_ResizedTexture;

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

    private int _nextMode;

    // Cache
    private Texture2D __cached_inTex;

    public bool Invoke(Texture2D inTex, int nextMode)
    {
        if (OpenCVSLIC.asyncBusy)
            return false;

        if (inTex == __cached_inTex)
        {
            InputMode.Instance.SetModeWithoutSideEffect(nextMode);
            return true;
        }

        m_InTexWidth = inTex.width;
        m_InTexHeight = inTex.height;

        _nextMode = nextMode;
        InputMode.Instance.SetModeWithoutSideEffect(InputMode.BUSY);

        __cached_inTex = inTex;

        RenderTexture renderTex = RenderTexture.GetTemporary(
            m_InTexWidth,
            m_InTexHeight,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        Graphics.Blit(inTex, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        
        m_ResizedTexture = new Texture2D(m_InTexWidth, m_InTexHeight);
        m_ResizedTexture.ReadPixels(new Rect(0, 0, m_InTexWidth, m_InTexHeight), 0, 0);
        m_ResizedTexture.Apply();
        
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        m_NumLevels = OpenCVSLIC.AsyncSLIC(m_ResizedTexture, ref m_OutLabel, ref m_OutContour);

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

            InputMode.Instance.SetModeWithoutSideEffect(_nextMode);

#if UNITY_EDITOR
            Debug.Log("OpenCVSLICClient - Finished AsyncSLIC in " + (Time.time - m_InvokedTime) + " seconds.");
#endif
            if (labelTextureProvider)
                labelTextureProvider.GenerateTextures(this);
            if (contourTextureProvider)
                contourTextureProvider.GenerateTextures(this);
            MessagePanel.Instance.Disable();

        }
    }

}