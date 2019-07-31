using UnityEngine;

public class OpenCVPCAClient : MonoBehaviour
{

    public StaticTexture paletteTextureProvider;

    private bool m_Invoked = false;
    private float m_InvokedTime;

    private int _nextMode;
    
    private OpenCVPCAData m_Data;

    private const int OCTAVE_LEVELS = 5;
    private const int PALETTE_SIZE = 2046;

    public bool Invoke(StaticTexture img, MaskTexture mask, SLICLabelTexture label, int nextMode)
    {
        if (OpenCVPCA.asyncBusy)
            return false;

        _nextMode = nextMode;
        InputMode.Instance.SetModeWithoutSideEffect(InputMode.BUSY);

        m_Data = new OpenCVPCAData();
        m_Data.levels = OCTAVE_LEVELS;
        m_Data.paletteArray = new float[PALETTE_SIZE];
        OpenCVPCA.AsyncPCA(img.GetReadableTexture(), mask.GetReadableTexture(), label.GetLabelTexture(), OCTAVE_LEVELS, m_Data.paletteArray);

        m_Invoked = true;
        m_InvokedTime = Time.time;

        return true;
    }

    void Update()
    {
        if (m_Invoked)
        {
            MessagePanel.Instance.ShowMessage("영역 설정 적용 중...", "OpenCV - PCA Procedure");
        }

        if (m_Invoked && !OpenCVPCA.asyncBusy)
        {
            m_Invoked = false;

            InputMode.Instance.SetModeWithoutSideEffect(_nextMode);

#if UNITY_EDITOR
            Debug.Log("OpenCVPCAClient - Finished AsyncPCA in " + (Time.time - m_InvokedTime) + " seconds.");
#endif
            float[][] palette = new float[m_Data.levels][];
            for (int level = 0, idx = 0; level < m_Data.levels; level++)
            {
                int size = 3 * (1 << (2 * level + 1));
                palette[level] = new float[size];
                for (int i = 0; i < size; i++)
                    palette[level][i] = m_Data.paletteArray[idx++];
            }
            
            Texture2D paletteTexture = new Texture2D(1 << (m_Data.levels - 1), 1 << (m_Data.levels), TextureFormat.RGB24, false);
            paletteTexture.SetPixels32(OpenCVUtils.OpenCVFloatArrayToColor32(palette[m_Data.levels - 1]));
            paletteTexture.Apply();

            if (paletteTextureProvider)
            {
                paletteTextureProvider.SetStaticTexture(paletteTexture);
                paletteTextureProvider.SetTarget();
                paletteTextureProvider.staticTexture.filterMode = FilterMode.Bilinear;
                paletteTextureProvider.staticTexture.wrapMode = TextureWrapMode.Clamp;
            }

            m_Data = null;

            MessagePanel.Instance.Disable();
        }
    }

}