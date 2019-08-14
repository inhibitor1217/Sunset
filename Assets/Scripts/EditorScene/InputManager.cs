using UnityEngine;

public class InputManager : MonoBehaviour
{

    public RectTransform container;
    public RectTransform image;
    public RectTransform optionMenu;
    
    public static InputManager instance { get; private set; }

    private bool _initialized = false;

    public float multiplicativeScale { get; private set; }
    private float m_DesiredMultiplicativeScale;
    public Vector2 position { get; private set; }
    private Vector2 m_DesiredPosition;

    public Vector2 inputPosition { get; private set; }
    public bool pressed { get; private set; }
    public bool released { get; private set; }
    public bool held { get; private set; }
    public bool withinContainer { get; private set; }
    public bool withinImage { get; private set; }

    public const float MIN_SCALE = 0.5f;
    public const float MAX_SCALE = 32.0f;
    public const float SCALE_MULTIPLIER = 1.1f;
    public const float MAX_SCALE_LOG = 1.50515f;

    void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        _initialized = true;

        image = null;
        optionMenu = null;

        multiplicativeScale = 1f;
        m_DesiredMultiplicativeScale = 1f;
        position = Vector2.zero;
        m_DesiredPosition = Vector2.zero;

        inputPosition = Vector2.zero;
        pressed = false;
        released = false;
        held = false;
        withinContainer = false;
        withinImage = false;
    }

    void Update()
    {
        if (!_initialized)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            inputPosition = touch.position;
            withinContainer = RectTransformUtility.RectangleContainsScreenPoint(container, inputPosition);
            if (optionMenu)
                withinContainer &= !RectTransformUtility.RectangleContainsScreenPoint(optionMenu, inputPosition);
            withinImage = RectTransformUtility.RectangleContainsScreenPoint(image, inputPosition);
            if (optionMenu)
                withinImage &= !RectTransformUtility.RectangleContainsScreenPoint(optionMenu, inputPosition);
            held = true;
            pressed = (touch.phase == TouchPhase.Began);
            released = (touch.phase == TouchPhase.Ended);

            if (withinContainer 
                && InputMode.Instance.isMove()
                && !InputMode.Instance.isBusy())
                updatePosition(touch.deltaPosition);
        }
        else if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            inputPosition   = Vector2.zero;
            withinContainer = false;
            withinImage     = false;
            held            = false;
            pressed         = false;
            released        = false;

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

                if (!InputMode.Instance.isBusy())
                {
                    updateScale(currMagnitude / prevMagnitude);
                    updatePosition(.5f * (touch0.deltaPosition + touch1.deltaPosition));
                }
            }
        }
        else
        {
            inputPosition   = Vector2.zero;
            withinContainer = false;
            withinImage     = false;
            held            = false;
            pressed         = false;
            released        = false;
        }
#else
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        held = Input.GetMouseButton(0);
        inputPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        withinContainer = RectTransformUtility.RectangleContainsScreenPoint(container, inputPosition);
        if (optionMenu)
            withinContainer &= !RectTransformUtility.RectangleContainsScreenPoint(optionMenu, inputPosition);
        withinImage = RectTransformUtility.RectangleContainsScreenPoint(image, inputPosition);
        if (optionMenu)
            withinImage &= !RectTransformUtility.RectangleContainsScreenPoint(optionMenu, inputPosition);
        pressed = Input.GetMouseButtonDown(0);
        released = Input.GetMouseButtonUp(0);

        updatePosition(-15f * new Vector2(inputX, inputY));
        if (Input.GetKey(KeyCode.Q))
            updateScale(SCALE_MULTIPLIER);
        else if (Input.GetKey(KeyCode.W))
            updateScale(1f/SCALE_MULTIPLIER);
#endif

        multiplicativeScale = Mathf.Lerp(
            multiplicativeScale, m_DesiredMultiplicativeScale, 10f * Time.deltaTime
        );
        position = Vector2.Lerp(
            position, m_DesiredPosition, 10f * Time.deltaTime
        );
    }

    void updatePosition(Vector2 deltaPosition)
    {
        m_DesiredPosition += deltaPosition / multiplicativeScale;

        float xBound = EditorSceneMaster.instance.rootLayer.xBound;
        float yBound = EditorSceneMaster.instance.rootLayer.yBound;
        
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