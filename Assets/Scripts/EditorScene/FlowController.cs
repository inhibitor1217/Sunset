using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FlowController : MonoBehaviour
{

    private List<Vector2> _currentPivots;
    private List<List<Vector2>> _pivots;

    private bool _editing = false;

    private const float THRESHOLD = 4f / 2048f;

    private MeshFilter m_MeshFilter;
    private MeshRenderer m_MeshRenderer;

    void Awake()
    {
        _currentPivots = new List<Vector2>();
        _pivots        = new List<List<Vector2>>();
        
        m_MeshFilter   = GetComponent<MeshFilter>();
        m_MeshRenderer = GetComponent<MeshRenderer>();

        m_MeshFilter.mesh = new Mesh();
    }

    void Update()
    {
        if (InputMode.Instance.isFlow())
        {
            if (!m_MeshRenderer.enabled)
                m_MeshRenderer.enabled = true;

            transform.localPosition = new Vector3(
                -.5f * InputManager.Instance.MultiplicativeScale * EditorSceneMaster.Instance.width,
                -.5f * InputManager.Instance.MultiplicativeScale * EditorSceneMaster.Instance.height,
                1
            );
            transform.localScale = new Vector3(
                InputManager.Instance.MultiplicativeScale * EditorSceneMaster.Instance.width,
                InputManager.Instance.MultiplicativeScale * EditorSceneMaster.Instance.height,
                1
            );

            if (!InputMode.Instance.isErase())
            {
                if (!_editing && InputManager.Instance.held)
                {
                    _currentPivots = new List<Vector2>();
                    _pivots.Add(_currentPivots);

                    _editing = true;

                    _currentPivots.Add(EditorSceneMaster.Instance.RelativeCoordsToRootRect(InputManager.Instance.inputPosition));
                }
                if (_editing && !InputManager.Instance.held)
                {
                    _editing = false;
                }

                if (_editing)
                {
                    if (Vector2.Distance(
                        _currentPivots[_currentPivots.Count - 1], 
                        EditorSceneMaster.Instance.RelativeCoordsToRootRect(InputManager.Instance.inputPosition)
                    ) > THRESHOLD)
                    {
                        _currentPivots.Add(EditorSceneMaster.Instance.RelativeCoordsToRootRect(InputManager.Instance.inputPosition));
                        GenerateMesh();
                    }
                }
            }
            else
            {

            }
        }
        else
        {
            if (m_MeshRenderer.enabled)
                m_MeshRenderer.enabled = false;
        }
    }

    int selectCurve(Vector2 pos)
    {
        return 0;
    }

    void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int>     indices  = new List<int>();

        int cnt = 0;
        foreach (List<Vector2> curve in _pivots)
        {
            for (int i = 0; i < curve.Count - 1; i++)
            {
                Vector2 st = curve[i];
                Vector2 ed = curve[i + 1];

                vertices.Add(new Vector3(st.x, st.y, 0));
                vertices.Add(new Vector3(ed.x, ed.y, 0));

                indices.Add(cnt++);
                indices.Add(cnt++);
            }
        }

        m_MeshFilter.mesh.SetVertices(vertices);
        m_MeshFilter.mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0, false);
    }

    public Mesh GetMesh()
    {
        return m_MeshFilter.mesh;
    }

}