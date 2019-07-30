using UnityEngine;

public class OpenCVPCAClient : MonoBehaviour
{

    private bool m_Invoked = false;
    private float m_InvokedTime;

    private int _nextMode;
    
    private float[] m_PaletteArray;

    private const int OCTAVE_LEVELS = 5;

    public bool Invoke(StaticTexture img, MaskTexture mask, SLICLabelTexture label, int nextMode)
    {
        if (OpenCVPCA.asyncBusy)
            return false;

        _nextMode = nextMode;
        InputMode.Instance.SetModeWithoutSideEffect(InputMode.BUSY);

        OpenCVPCA.AsyncPCA(img.GetReadableTexture(), mask.GetReadableTexture(), label.GetLabelTexture(), OCTAVE_LEVELS, m_PaletteArray);

        m_Invoked = true;
        m_InvokedTime = Time.time;

        return true;
    }

    void Awake()
    {
        int size = 0;
        for (int i = 0; i < OCTAVE_LEVELS; i++)
        {
            size += 2 * 3 * (1 << (2 * (OCTAVE_LEVELS - i - 1)));
        }

        m_PaletteArray = new float[size];
    }

    void Update()
    {
        if (m_Invoked)
        {
            MessagePanel.Instance.ShowMessage("OpenCV - PCA 이미지 처리 중...");
        }

        if (m_Invoked && !OpenCVPCA.asyncBusy)
        {
            m_Invoked = false;

            InputMode.Instance.SetModeWithoutSideEffect(_nextMode);

#if UNITY_EDITOR
            Debug.Log("OpenCVPCAClient - Finished AsyncPCA in " + (Time.time - m_InvokedTime) + " seconds.");
#endif
            

            MessagePanel.Instance.Disable();
        }
    }

}