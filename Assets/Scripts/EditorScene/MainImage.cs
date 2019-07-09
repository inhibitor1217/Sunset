using UnityEngine;
using UnityEngine.UI;

public class MainImage : MonoBehaviour
{
    
    private RawImage m_RawImage;
    private RectTransform m_RectTransform;
    private Vector2 m_ImageBaseScale;
    private float m_MultiplicativeScale;
    private float m_DesiredMultiplicativeScale;
    private Vector2 m_Position;
    private Vector2 m_DesiredPosition;

    public const float CONTAINER_WIDTH = 1458;
    public const float CONTAINER_HEIGHT = 864;
    public const float MIN_SCALE = 0.8f;
    public const float MAX_SCALE = 32.0f;

#if UNITY_ANDROID && !UNITY_EDITOR
    private const float GRID_OPACITY_CURVE_CENTER = 24f;
#else
    private const float GRID_OPACITY_CURVE_CENTER = 15f;
#endif

    public Text scaleText;

    void Awake()
    {
        m_RawImage = GetComponent<RawImage>();
        m_RectTransform = GetComponent<RectTransform>();

        if (m_RawImage)
        {
            m_RawImage.texture = Texture2D.blackTexture;
        }

        m_ImageBaseScale = Vector2.one;
        m_Position = Vector2.zero;
        m_DesiredPosition = Vector2.zero;
        m_MultiplicativeScale = 1f;
        m_DesiredMultiplicativeScale = 1f;
    }

    void Update()
    {
        m_MultiplicativeScale = Mathf.Lerp(
            m_MultiplicativeScale, m_DesiredMultiplicativeScale, 10f * Time.deltaTime
        );
        m_Position = Vector2.Lerp(
            m_Position, m_DesiredPosition, 10f * Time.deltaTime
        );

        m_RawImage.materialForRendering.SetFloat("_Grid_Opacity",
            Mathf.Clamp(.5f + .1f * (m_MultiplicativeScale - GRID_OPACITY_CURVE_CENTER), 0f, 1f)
        );

        m_RawImage.uvRect = new Rect(
            -(.5f / m_MultiplicativeScale * m_ImageBaseScale + m_Position) + .5f * m_ImageBaseScale,
            (1f / m_MultiplicativeScale) * m_ImageBaseScale
        );

        if (scaleText)
            scaleText.text = m_MultiplicativeScale.ToString();
    }

    public void SetTexture(Texture2D texture)
    {
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        float aspectRatio = (float) CONTAINER_WIDTH / (float) CONTAINER_HEIGHT * (float) texture.height / (float) texture.width;
        if (aspectRatio > 1f)
            m_ImageBaseScale = new Vector2(aspectRatio, 1f);
        else
            m_ImageBaseScale = new Vector2(1f, 1f/aspectRatio);

        m_MultiplicativeScale = 1f;
        m_DesiredMultiplicativeScale = 1f;

        m_Position = .5f * m_ImageBaseScale - .5f * Vector2.one;
        m_DesiredPosition = .5f * m_ImageBaseScale - .5f * Vector2.one;

        m_RawImage.uvRect = new Rect(0, 0, 1, 1);
        m_RawImage.texture = texture;
    }

    public void UpdatePosition(Vector2 deltaPosition)
    {
        m_DesiredPosition += (.0005f / m_MultiplicativeScale) * deltaPosition;
        m_DesiredPosition.x = Mathf.Clamp(m_DesiredPosition.x, -.5f, -.5f + m_ImageBaseScale.x);
        m_DesiredPosition.y = Mathf.Clamp(m_DesiredPosition.y, -.5f, -.5f + m_ImageBaseScale.y);
    }

    public void UpdateScale(float deltaScale)
    {
        m_DesiredMultiplicativeScale = Mathf.Clamp(
            m_DesiredMultiplicativeScale * deltaScale, 
            MIN_SCALE, MAX_SCALE
        );
    }

}
