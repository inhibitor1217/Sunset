using UnityEngine;

public class OpenCVPCAClient : MonoBehaviour
{

    private bool m_Invoked = false;
    private float m_InvokedTime;

    private int _nextMode;
    
    private float[] m_PaletteArray;

    [SerializeField]
    private Color32[] m_Palette;

    public bool Invoke(StaticTexture img, MaskTexture mask, SLICLabelTexture label, int nextMode)
    {
        if (OpenCVPCA.asyncBusy)
            return false;

        _nextMode = nextMode;
        InputMode.Instance.SetModeWithoutSideEffect(InputMode.BUSY);

        OpenCVPCA.AsyncPCA(img.GetReadableTexture(), mask.GetReadableTexture(), label.GetLabelTexture(), m_PaletteArray);

        m_Invoked = true;
        m_InvokedTime = Time.time;

        return true;
    }

    void Awake()
    {
        m_PaletteArray = new float[33];
        m_Palette = new Color32[11];
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
            
            for (int i = 0; i < 11; i++)
            {
                m_Palette[i] = new Color32(
                    (byte)Mathf.RoundToInt(m_PaletteArray[3 * i + 2] * 255f), 
                    (byte)Mathf.RoundToInt(m_PaletteArray[3 * i + 1] * 255f), 
                    (byte)Mathf.RoundToInt(m_PaletteArray[3 * i + 0] * 255f),
                    255
                );
            }

            MessagePanel.Instance.Disable();
        }
    }

}