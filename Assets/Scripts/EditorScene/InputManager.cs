using UnityEngine;

public class InputManager : MonoBehaviour
{

    public RectTransform container;
    
    private static InputManager m_Instance;
    public static InputManager Instance { get { return m_Instance; } }

    [HideInInspector]
    public float xBound = .5f, yBound = .5f;

    private float m_MultiplicativeScale = 1f;
    public float MultiplicativeScale { get { return m_MultiplicativeScale; } }
    private float m_DesiredMultiplicativeScale = 1f;
    private Vector2 m_Position = Vector2.zero;
    public Vector2 Position { get { return m_Position; } }
    private Vector2 m_DesiredPosition = Vector2.zero;

    public const float MIN_SCALE = 0.8f;
    public const float MAX_SCALE = 32.0f;
    public const float SCALE_MULTIPLIER = 1.1f;
    public const float MAX_SCALE_LOG = 1.50515f;

    void Awake()
    {
        m_Instance = this;
    }

    void FixedUpdate()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Input.touchCount == 1)
        {
            if (InputMode.Instance.mode != InputMode.MOVE)
                return;

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
            updateScale(SCALE_MULTIPLIER);
        else if (Input.GetKey(KeyCode.W))
            updateScale(1f/SCALE_MULTIPLIER);
#endif

        m_MultiplicativeScale = Mathf.Lerp(
            m_MultiplicativeScale, m_DesiredMultiplicativeScale, 10f * Time.deltaTime
        );
        m_Position = Vector2.Lerp(
            m_Position, m_DesiredPosition, 10f * Time.deltaTime
        );
    }

    void updatePosition(Vector2 deltaPosition)
    {
        m_DesiredPosition += deltaPosition / m_MultiplicativeScale;
        m_DesiredPosition.x = Mathf.Clamp(m_DesiredPosition.x, -.5f * xBound, .5f * xBound);
        m_DesiredPosition.y = Mathf.Clamp(m_DesiredPosition.y, -.5f * yBound, .5f * yBound);
    }

    void updateScale(float deltaScale)
    {
        m_DesiredMultiplicativeScale = Mathf.Clamp(
            m_DesiredMultiplicativeScale * deltaScale, 
            MIN_SCALE, MAX_SCALE
        );
    }

}