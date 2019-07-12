using UnityEngine;

public class BrushController : MonoBehaviour
{
    
    public RectTransform container;

    private MeshRenderer m_MeshRenderer;

    void Awake()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        m_MeshRenderer.enabled = false;
    }

    void Update()
    {
        Vector2 input;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (InputMode.Instance.mode == InputMode.BRUSH && Input.touchCount == 1)
#else
        if (InputMode.Instance.mode == InputMode.BRUSH && Input.GetMouseButton(0))
#endif
        {
            if (!m_MeshRenderer.enabled)
                m_MeshRenderer.enabled = true;
#if UNITY_ANDROID && !UNITY_EDITOR
            input = Input.GetTouch(0).position;
#else
            input = Input.mousePosition;
#endif
        }
        else
        {
            if (m_MeshRenderer.enabled)
                m_MeshRenderer.enabled = false;
            return;
        }

        if (container
            && RectTransformUtility.RectangleContainsScreenPoint(container, input))
        {
            transform.position = input;
        }
    }

}
