using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FlowController : MonoBehaviour
{

    public GameObject FlowPrefab;

    private List<Vector3> _currentPivots;
    private List<List<Vector3>> _pivots;

    private bool _editing = false;

    private const float THRESHOLD = 4f / 2048f;

    private List<GameObject> _uiFlowObjects;
    private GameObject _currentFlowObject;
    private LineRenderer _currentLineRenderer;

    private Mesh _mesh;

    void Awake()
    {
        _currentPivots = new List<Vector3>();
        _pivots        = new List<List<Vector3>>();        
        _uiFlowObjects = new List<GameObject>();
        _mesh          = new Mesh();
    }

    void Update()
    {
        if (InputMode.Instance.isFlow())
        {
            if (!InputMode.Instance.isErase())
            {
                if (!_editing && InputManager.Instance.held && InputManager.Instance.withinContainer)
                {
                    _editing = true;

                    _currentPivots = new List<Vector3>();
                    _pivots.Add(_currentPivots);
                    _currentPivots.Add(EditorSceneMaster.Instance.RelativeCoordsToRootRect(InputManager.Instance.inputPosition));

                    _currentFlowObject = GameObject.Instantiate(FlowPrefab);
                    _currentFlowObject.name = "Flow " + _pivots.Count;
                    _currentFlowObject.GetComponent<RectTransform>().SetParent(EditorSceneMaster.Instance.GetRootLayerTransform());
                    _currentLineRenderer = _currentFlowObject.GetComponent<LineRenderer>();
                    _currentLineRenderer.positionCount = _currentPivots.Count;
                    _currentLineRenderer.SetPositions(_currentPivots.ToArray());
                    _uiFlowObjects.Add(_currentFlowObject);
                }
                if (!InputManager.Instance.held || !InputManager.Instance.withinContainer)
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
                        
                        _currentLineRenderer.positionCount = _currentPivots.Count;
                        _currentLineRenderer.SetPositions(_currentPivots.ToArray());
                    }
                }
            }
            else
            {

            }

            foreach (GameObject obj in _uiFlowObjects)
            {
                if (!obj.activeInHierarchy)
                    obj.SetActive(true);

                obj.GetComponent<RectTransform>().localPosition = new Vector3(
                    -.5f * InputManager.Instance.MultiplicativeScale * EditorSceneMaster.Instance.width,
                    -.5f * InputManager.Instance.MultiplicativeScale * EditorSceneMaster.Instance.height,
                    0
                );
                obj.GetComponent<RectTransform>().localScale = new Vector3(
                    InputManager.Instance.MultiplicativeScale * EditorSceneMaster.Instance.width,
                    InputManager.Instance.MultiplicativeScale * EditorSceneMaster.Instance.height,
                    1
                );
            }
        }
        else
        {
            foreach (GameObject obj in _uiFlowObjects)
            {
                if (obj.activeInHierarchy)
                    obj.SetActive(false);
            }
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
        foreach (List<Vector3> curve in _pivots)
        {
            for (int i = 0; i < curve.Count - 1; i++)
            {
                Vector3 st = curve[i];
                Vector3 ed = curve[i + 1];

                vertices.Add(st);
                vertices.Add(ed);

                indices.Add(cnt++);
                indices.Add(cnt++);
            }
        }

        _mesh.SetVertices(vertices);
        _mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0, false);
    }

    public Mesh GetMesh()
    {
        return _mesh;
    }

}