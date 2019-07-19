using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BrushController : MonoBehaviour
{
    
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
        if (InputMode.Instance.isBrush() && !InputMode.Instance.isSLIC() 
            && InputManager.Instance.held
            && InputManager.Instance.withinContainer)
        {
            if (!m_MeshRenderer.enabled)
                m_MeshRenderer.enabled = true;
            transform.position = InputManager.Instance.inputPosition;
        }
        else
        {
            if (m_MeshRenderer.enabled)
                m_MeshRenderer.enabled = false;
        }
    }

}
