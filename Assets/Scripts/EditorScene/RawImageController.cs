using UnityEngine;
using UnityEngine.UI;

public class RawImageController : MonoBehaviour
{

    public RectTransform container;
    public TextureProvider provider;

    public bool isRoot = false;
    public Camera maskCamera = null;

    private RawImage m_RawImage;
    private RectTransform m_RectTransform;
    private Vector2 m_ImageBaseScale;
    private float m_MultiplicativeScale = 1f;
    private float m_DesiredMultiplicativeScale = 1f;
    private Vector2 m_Position = Vector2.zero;
    private Vector2 m_DesiredPosition = Vector2.zero;

    private const float MIN_SCALE = 0.8f;
    private const float MAX_SCALE = 32.0f;

#if UNITY_ANDROID && !UNITY_EDITOR
    private const float GRID_OPACITY_CURVE_CENTER = 24f;
#else
    private const float GRID_OPACITY_CURVE_CENTER = 15f;
#endif

    void Awake()
    {
        m_RawImage = GetComponent<RawImage>();
        m_RectTransform = GetComponent<RectTransform>();

        if (m_RawImage)
        {
            m_RawImage.texture = Texture2D.whiteTexture;
        }

        if (provider)
            provider.SetTarget(this);

        TextureProviderManager.UpdateEager();

        m_ImageBaseScale = Vector2.one;
        m_Position = Vector2.zero;
        m_DesiredPosition = Vector2.zero;
        m_MultiplicativeScale = 1f;
        m_DesiredMultiplicativeScale = 1f;
    }

    void Update()
    {
        if (InputMode.Instance.mode != InputMode.MOVE)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (container
                && RectTransformUtility.RectangleContainsScreenPoint(
                        container, touch.position
                    ))
            {
                updatePosition(touch.deltaPosition);
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            if (container
                && RectTransformUtility.RectangleContainsScreenPoint(
                    container, touch0.position
                )
                && RectTransformUtility.RectangleContainsScreenPoint(
                    container, touch1.position
                ))
            {
                Vector2 touch0Prev = touch0.position - touch0.deltaPosition;
                Vector2 touch1Prev = touch1.position - touch1.deltaPosition;

                float prevMagnitude = (touch1Prev - touch0Prev).magnitude;
                float currMagnitude = (touch1.position - touch0.position).magnitude;

                updateScale(currMagnitude / prevMagnitude);
            }
        }
#else
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        updatePosition(-15f * new Vector2(inputX, inputY));

        if (Input.GetKey(KeyCode.Q))
            updateScale(1.1f);
        else if (Input.GetKey(KeyCode.W))
            updateScale(0.9f);
#endif

        m_MultiplicativeScale = Mathf.Lerp(
            m_MultiplicativeScale, m_DesiredMultiplicativeScale, 10f * Time.deltaTime
        );
        m_Position = Vector2.Lerp(
            m_Position, m_DesiredPosition, 10f * Time.deltaTime
        );

        m_RawImage.materialForRendering.SetFloat("_Grid_Opacity",
            Mathf.Clamp(.5f + .1f * (m_MultiplicativeScale - GRID_OPACITY_CURVE_CENTER), 0f, 1f)
        );

        if (isRoot)
        {
            m_RectTransform.anchoredPosition = m_Position * m_MultiplicativeScale;
            if (maskCamera)
            {
                maskCamera.orthographicSize = .5f * m_ImageBaseScale.y * m_MultiplicativeScale;
            }
        }
        m_RectTransform.sizeDelta = m_ImageBaseScale * m_MultiplicativeScale;
    }

    public void SetTexture(Texture texture)
    {
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        m_RawImage.texture = texture;
        
        m_ImageBaseScale = new Vector2(texture.width, texture.height);

        m_RectTransform.anchoredPosition = m_Position * m_MultiplicativeScale;
        m_RectTransform.sizeDelta = m_ImageBaseScale * m_MultiplicativeScale;
    }

    void updatePosition(Vector2 deltaPosition)
    {
        if (isRoot)
        {
            m_DesiredPosition += deltaPosition / m_MultiplicativeScale;
            m_DesiredPosition.x = Mathf.Clamp(m_DesiredPosition.x, -.5f * m_ImageBaseScale.x, .5f * m_ImageBaseScale.x);
            m_DesiredPosition.y = Mathf.Clamp(m_DesiredPosition.y, -.5f * m_ImageBaseScale.y, .5f * m_ImageBaseScale.y);
        }
    }

    void updateScale(float deltaScale)
    {
        m_DesiredMultiplicativeScale = Mathf.Clamp(
            m_DesiredMultiplicativeScale * deltaScale, 
            MIN_SCALE, MAX_SCALE
        );
    }

}
