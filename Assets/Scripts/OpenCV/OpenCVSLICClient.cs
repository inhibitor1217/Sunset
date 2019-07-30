using UnityEngine;

public class OpenCVSLICClient : MonoBehaviour
{

    public SLICLabelTexture labelTextureProvider;
    public SLICContourTexture contourTextureProvider;
    
    private OpenCVSLICData m_Data;
    private bool m_Invoked = false;
    private float m_InvokedTime;
    private int m_prevProgress;

    private int _nextMode;

    // Cache
    private Texture2D __cached_inTex;

    public bool Invoke(StaticTexture img, int nextMode)
    {
        if (OpenCVSLIC.asyncBusy)
            return false;

        Texture2D inTex = img.GetReadableTexture();

        if (inTex == __cached_inTex)
        {
            InputMode.Instance.SetModeWithoutSideEffect(nextMode);
            return true;
        }

        _nextMode = nextMode;
        InputMode.Instance.SetModeWithoutSideEffect(InputMode.BUSY);

        __cached_inTex = inTex;

        m_Data = new OpenCVSLICData();
        m_Data.levels = OpenCVSLIC.AsyncSLIC(inTex, ref m_Data.outLabel, ref m_Data.outContour);
        m_Data.width = inTex.width;
        m_Data.height = inTex.height;

        m_Invoked = true;
        m_InvokedTime = Time.time;
        m_prevProgress = -1;

        return true;
    }

    void Update()
    {
        if (m_Invoked && m_prevProgress != OpenCVSLIC.asyncProgress)
        {
            MessagePanel.Instance.ShowMessage("이미지 전처리 중...", "OpenCV - SLIC Procedure (" + OpenCVSLIC.asyncProgress + "/" + m_Data.levels + ")");
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
                labelTextureProvider.GenerateTextures(m_Data);
            if (contourTextureProvider)
                contourTextureProvider.GenerateTextures(m_Data);
            
            m_Data = null;

            MessagePanel.Instance.Disable();

        }
    }

}