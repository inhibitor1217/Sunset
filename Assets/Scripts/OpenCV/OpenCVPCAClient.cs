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
        InputMode.instance.SetModeWithoutSideEffect(InputMode.BUSY);

        m_Data = new OpenCVPCAData();
        m_Data.levels = OCTAVE_LEVELS;
        m_Data.paletteArray = new float[PALETTE_SIZE];
        OpenCVPCA.AsyncPCA(
            img.GetReadableTexture(), 
            mask.GetReadableTexture(),
            label.GetLabelTexture(), 
            OCTAVE_LEVELS, 
            m_Data.paletteArray
        );

        m_Invoked = true;
        m_InvokedTime = Time.time;

        return true;
    }

    void Update()
    {
        if (m_Invoked)
        {
            MessagePanel.instance.ShowMessage("영역 설정 적용 중...", "OpenCV - PCA Procedure");
        }

        if (m_Invoked && !OpenCVPCA.asyncBusy)
        {
            m_Invoked = false;

            InputMode.instance.SetModeWithoutSideEffect(_nextMode);

#if UNITY_EDITOR
            Debug.Log("OpenCVPCAClient - Finished AsyncPCA in " + (Time.time - m_InvokedTime) + " seconds.");
#endif
            float[][] low = new float[m_Data.levels][], high = new float[m_Data.levels][];
            for (int level = 0, idx = 0; level < m_Data.levels; level++)
            {
                int size = 3 * (1 << (2 * level));
                low[level] = new float[size];
                high[level] = new float[size];
                for (int i = 0; i < size; i++)
                    low[level][i] = m_Data.paletteArray[idx++];
                for (int i = 0; i < size; i++)
                    high[level][i] = m_Data.paletteArray[idx++];
            }

            float[] palette = new float[3 * (1 << (2 * m_Data.levels - 1))];
            for (int x = 0; x < (1 << (m_Data.levels - 1)); x++)
                for (int y = 0; y < (1 << (m_Data.levels - 1)); y++)
                    for (int c = 0; c < 3; c++)
                    {
                        for (int level = 0; level < m_Data.levels; level++)
                        {
                            float lowColor, highColor;
                            if (level < m_Data.levels - 1)
                            {
                                float _x = x + .5f - (1 << (m_Data.levels - level - 2));
                                float _y = y + .5f - (1 << (m_Data.levels - level - 2));
                                int _ix = Mathf.FloorToInt(_x / (1 << (m_Data.levels - level - 1)));
                                int _iy = Mathf.FloorToInt(_y / (1 << (m_Data.levels - level - 1)));
                                float _tx = _x % (1 << (m_Data.levels - level - 1));
                                float _ty = _y % (1 << (m_Data.levels - level - 1));
                                int _jx = _ix + 1;
                                int _jy = _iy + 1;
                                _ix = Mathf.Clamp(_ix, 0, (1 << level) - 1);
                                _iy = Mathf.Clamp(_iy, 0, (1 << level) - 1);
                                _jx = Mathf.Clamp(_jx, 0, (1 << level) - 1);
                                _jy = Mathf.Clamp(_jy, 0, (1 << level) - 1);
                                lowColor = Mathf.Lerp(
                                    Mathf.Lerp(
                                        low[level][3 * (_ix + _iy * (1 << level)) + c],
                                        low[level][3 * (_ix + _jy * (1 << level)) + c],
                                        _ty
                                    ),
                                    Mathf.Lerp(
                                        low[level][3 * (_jx + _iy * (1 << level)) + c],
                                        low[level][3 * (_jx + _jy * (1 << level)) + c],
                                        _ty
                                    ),
                                    _tx
                                );
                                highColor = Mathf.Lerp(
                                    Mathf.Lerp(
                                        high[level][3 * (_ix + _iy * (1 << level)) + c],
                                        high[level][3 * (_ix + _jy * (1 << level)) + c],
                                        _ty
                                    ),
                                    Mathf.Lerp(
                                        high[level][3 * (_jx + _iy * (1 << level)) + c],
                                        high[level][3 * (_jx + _jy * (1 << level)) + c],
                                        _ty
                                    ),
                                    _tx
                                );
                            }
                            else
                            {
                                lowColor  = low [level][3 * (x + y * (1 << level)) + c];
                                highColor = high[level][3 * (x + y * (1 << level)) + c];
                            }
                            // LOW
                            palette[3 * ( x + y * (1 << (m_Data.levels - 1)) ) + c] += (m_Data.levels - level) * lowColor;
                            // HIGH
                            palette[3 * ( x + y * (1 << (m_Data.levels - 1)) ) + c + 3 * (1 << (2 * m_Data.levels - 2))] += (level + 1) * highColor;
                        }
                        palette[3 * ( x + y * (1 << (m_Data.levels - 1)) ) + c] /= (float)(m_Data.levels * (m_Data.levels + 1) / 2);
                        palette[3 * ( x + y * (1 << (m_Data.levels - 1)) ) + c + 3 * (1 << (2 * m_Data.levels - 2))] /= (float)(m_Data.levels * (m_Data.levels + 1) / 2);
                    }
            
            Texture2D paletteTexture = new Texture2D(1 << (m_Data.levels - 1), 1 << (m_Data.levels), TextureFormat.RGB24, false);
            paletteTexture.SetPixels32(OpenCVUtils.OpenCVFloatArrayToColor32(palette));
            paletteTexture.Apply();

            if (paletteTextureProvider)
            {
                paletteTextureProvider.SetStaticTexture(paletteTexture);
                paletteTextureProvider.SetTarget();
                paletteTextureProvider.staticTexture.filterMode = FilterMode.Bilinear;
                paletteTextureProvider.staticTexture.wrapMode = TextureWrapMode.Clamp;
            }

            m_Data = null;

            MessagePanel.instance.Disable();
        }
    }

}