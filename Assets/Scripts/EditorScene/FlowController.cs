using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FlowController : MonoBehaviour
{

    public GameObject FlowPrefab;

    private List<Vector3> _currentPivots;
    private List<List<Vector3>> _pivots;
    private Vector3 _lastPivot;

    private bool _editing = false;
    private bool _created = false;

    private const float ADD_THRESHOLD = 4f / 2048f;
    private const float ERASE_THRESHOLD = 20f / 2048f;

    private List<GameObject> _uiFlowObjects;
    private GameObject _currentFlowObject;
    private LineRenderer _currentLineRenderer;

    void Awake()
    {
        _currentPivots = new List<Vector3>();
        _pivots        = new List<List<Vector3>>();        
        _uiFlowObjects = new List<GameObject>();
    }

    void Update()
    {
        if (InputMode.Instance.isFlow())
        {
            if (!InputMode.Instance.isErase())
            {
                if (!_editing && InputManager.Instance.held && InputManager.Instance.withinImage)
                {
                    _editing = true;
                    _lastPivot = EditorSceneMaster.Instance.RelativeCoordsToRootRect(InputManager.Instance.inputPosition);
                }
                if (!InputManager.Instance.held || !InputManager.Instance.withinImage)
                {
                    _editing = false;
                    _created = false;
                }

                if (_editing)
                {
                    if (Vector2.Distance(
                        _lastPivot, 
                        EditorSceneMaster.Instance.RelativeCoordsToRootRect(InputManager.Instance.inputPosition)
                    ) > ADD_THRESHOLD)
                    {
                        if (!_created)
                        {
                            _created = true;

                            _currentPivots = new List<Vector3>();
                            _pivots.Add(_currentPivots);
                            _currentPivots.Add(_lastPivot);

                            _currentFlowObject = GameObject.Instantiate(FlowPrefab);
                            _currentFlowObject.name = "Flow " + _pivots.Count;
                            _currentFlowObject.GetComponent<RectTransform>().SetParent(EditorSceneMaster.Instance.GetRootLayerTransform());
                            _currentLineRenderer = _currentFlowObject.GetComponent<LineRenderer>();
                            _uiFlowObjects.Add(_currentFlowObject);
                        }

                        _currentPivots.Add(EditorSceneMaster.Instance.RelativeCoordsToRootRect(InputManager.Instance.inputPosition));
                        
                        _currentLineRenderer.positionCount = _currentPivots.Count;
                        _currentLineRenderer.SetPositions(_currentPivots.ToArray());
                    }
                }
            }
            else
            {
                if (InputManager.Instance.pressed && InputManager.Instance.withinImage)
                {
                    int curveIdx = selectCurve(EditorSceneMaster.Instance.RelativeCoordsToRootRect(InputManager.Instance.inputPosition));
                    if (curveIdx != -1)
                    {
                        _pivots.RemoveAt(curveIdx);
                    }
                }
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
        int closestCurve = -1;
        float closestDist = 0, curDist;

        if (_pivots.Count == 0)
            return closestCurve;

        for (int i = 0; i < _pivots.Count; i++)
            for (int j = 0; j < _pivots[i].Count; j++)
                if ((curDist = Vector2.Distance(_pivots[i][j], pos)) < ERASE_THRESHOLD)
                    if (closestCurve == -1 || closestDist > curDist)
                    {
                        closestCurve = i;
                        closestDist = curDist;
                    }

        return closestCurve;
    }

    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh();

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

        mesh.SetVertices(vertices);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0, false);

        return mesh;
    }

}