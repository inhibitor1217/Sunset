using UnityEngine;

public class OpenCVSLICClient : MonoBehaviour
{

    public SLICLabelTexture labelTextureProvider;
    public SLICContourTexture contourTextureProvider;
    
    private OpenCVSLICData m_Data;
    private bool m_Invoked = false;
    private float m_InvokedTime;

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
            InputMode.instance.SetModeWithoutSideEffect(nextMode);
            return true;
        }

        _nextMode = nextMode;
        InputMode.instance.SetModeWithoutSideEffect(InputMode.BUSY);

        __cached_inTex = inTex;

        m_Data = new OpenCVSLICData();
        OpenCVSLIC.AsyncSLIC(inTex, ref m_Data.outLabel, ref m_Data.outContour);
        m_Data.width = inTex.width;
        m_Data.height = inTex.height;

        m_Invoked = true;
        m_InvokedTime = Time.time;

        return true;
    }

    void Update()
    {
        if (m_Invoked)
        {
            MessagePanel.instance.ShowMessage("이미지 전처리 중...", "OpenCV - SLIC Procedure");
        }

        // Busy wait
        if (m_Invoked && !OpenCVSLIC.asyncBusy)
        {
            // Job finished
            m_Invoked = false;

            InputMode.instance.SetModeWithoutSideEffect(_nextMode);

#if UNITY_EDITOR
            Debug.Log("OpenCVSLICClient - Finished AsyncSLIC in " + (Time.time - m_InvokedTime) + " seconds.");
#endif
            if (labelTextureProvider)
                labelTextureProvider.GenerateTextures(m_Data);
            if (contourTextureProvider)
                contourTextureProvider.GenerateTextures(m_Data);
            
            m_Data = null;

            MessagePanel.instance.Disable();

        }
    }

}