using UnityEngine;
using UnityEngine.UI;

public class MainImage : MonoBehaviour
{
    
    private Image m_Image;
    private RectTransform m_RectTransform;
    private Vector2 m_ImageBaseScale;
    private float m_MultiplicativeScale;
    private float m_DesiredMultiplicativeScale;
    private Vector2 m_Position;
    private Vector2 m_DesiredPosition;

    public const float CONTAINER_WIDTH = 1620;
    public const float CONTAINER_HEIGHT = 1080;
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
        m_Image = GetComponent<Image>();
        m_RectTransform = GetComponent<RectTransform>();

        if (m_Image)
        {
            m_Image.material.mainTexture = Texture2D.blackTexture;
        }
    }

    void Update()
    {
        m_MultiplicativeScale = Mathf.Lerp(
            m_MultiplicativeScale, m_DesiredMultiplicativeScale, 10f * Time.deltaTime
        );
        m_Position = Vector2.Lerp(
            m_Position, m_DesiredPosition, 10f * Time.deltaTime
        );

        m_RectTransform.sizeDelta = m_MultiplicativeScale * m_ImageBaseScale;
        m_RectTransform.anchoredPosition = m_MultiplicativeScale * m_Position;

        m_Image.materialForRendering.SetFloat("_Grid_Opacity",
            Mathf.Clamp(.5f + .1f * (m_MultiplicativeScale - GRID_OPACITY_CURVE_CENTER), 0f, 1f)
        );

        if (scaleText)
            scaleText.text = m_MultiplicativeScale.ToString();
    }

    public void SetTexture(Texture2D texture)
    {
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        float initialScale = 0.9f * Mathf.Min(
            CONTAINER_WIDTH / texture.width, CONTAINER_HEIGHT / texture.height
        );
        m_RectTransform.sizeDelta = m_ImageBaseScale = initialScale * new Vector2(texture.width, texture.height);
        
        m_MultiplicativeScale = 1f;
        m_DesiredMultiplicativeScale = 1f;
        m_Position = Vector2.zero;
        m_DesiredPosition = Vector2.zero;

        m_Image.sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            .5f * Vector2.one
        );
    }

    public void UpdatePosition(Vector2 deltaPosition)
    {
        m_DesiredPosition += (.5f / m_MultiplicativeScale) * deltaPosition;
        m_DesiredPosition.x = Mathf.Clamp(m_DesiredPosition.x, -.5f * m_ImageBaseScale.x, .5f * m_ImageBaseScale.x);
        m_DesiredPosition.y = Mathf.Clamp(m_DesiredPosition.y, -.5f * m_ImageBaseScale.y, .5f * m_ImageBaseScale.y);
    }

    public void UpdateScale(float deltaScale)
    {
        m_DesiredMultiplicativeScale = Mathf.Clamp(
            m_DesiredMultiplicativeScale * deltaScale, 
            MIN_SCALE, MAX_SCALE
        );
    }

}
