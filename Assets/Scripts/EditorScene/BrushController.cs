using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BrushController : MonoBehaviour
{
    
    public RectTransform container;

    private MeshRenderer m_MeshRenderer;

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = Geometry.CreateCircleMesh(32);
        m_MeshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        m_MeshRenderer.enabled = false;
    }

    void Update()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        Vector2 input = Input.GetTouch(0).position;
        #else
        Vector2 input = Input.mousePosition;
        #endif

        if (container
            && RectTransformUtility.RectangleContainsScreenPoint(container, input))
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (InputMode.Instance.mode == InputMode.BRUSH && Input.touchCount == 1)
            #else
            if (InputMode.Instance.mode == InputMode.BRUSH && Input.GetMouseButton(0))
            #endif
            {
                if (!m_MeshRenderer.enabled)
                    m_MeshRenderer.enabled = true;
            }
            else
            {
                if (m_MeshRenderer.enabled)
                    m_MeshRenderer.enabled = false;
                return;
            }

            transform.position = input;
        }
        else
        {
            if (m_MeshRenderer.enabled)
                    m_MeshRenderer.enabled = false;
        }
    }

}
