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
        if (InputMode.instance.isBrush() && !InputMode.instance.isSLIC() 
            && InputManager.instance.held
            && InputManager.instance.withinContainer)
        {
            if (!m_MeshRenderer.enabled)
                m_MeshRenderer.enabled = true;
            transform.position = InputManager.instance.inputPosition;
        }
        else
        {
            if (m_MeshRenderer.enabled)
                m_MeshRenderer.enabled = false;
        }
    }

}