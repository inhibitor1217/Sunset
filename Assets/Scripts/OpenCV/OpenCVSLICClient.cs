using UnityEngine;

public class OpenCVSLICClient : MonoBehaviour
{

    public SLICLabelTexture labelTextureProvider;
    public SLICContourTexture contourTextureProvider;
    
    private OpenCVSLICData m_Data;
    public int NumLevels { get; private set; }
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

        _nextMode = nextMode;
        InputMode.Instance.SetModeWithoutSideEffect(InputMode.BUSY);

        __cached_inTex = inTex;

        m_Data = new OpenCVSLICData();
        NumLevels = OpenCVSLIC.AsyncSLIC(inTex, ref m_Data.outLabel, ref m_Data.outContour);

        m_Invoked = true;
        m_InvokedTime = Time.time;
        m_prevProgress = -1;

        return true;
    }

    public int[] getLabel(int level)
    {
        return m_Data.outLabel[level];
    }

    public byte[] getContour(int level)
    {
        return m_Data.outContour[level];
    }

    public int getWidth(int level)
    {
        return __cached_inTex.width >> level;
    }

    public int getHeight(int level)
    {
        return __cached_inTex.height >> level;
    }

    void Update()
    {
        if (m_Invoked && m_prevProgress != OpenCVSLIC.asyncProgress)
        {
            MessagePanel.Instance.ShowMessage("OpenCV - SLIC 이미지 처리 중... (" + OpenCVSLIC.asyncProgress + "/" + NumLevels + ")");
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
            
            m_Data = null;

            MessagePanel.Instance.Disable();

        }
    }

}