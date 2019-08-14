using UnityEngine;
using System.Collections.Generic;

public class FlowController : MonoBehaviour
{

    public GameObject FlowPrefab;

    private List<Vector3> _currentPivots;
    public List<List<Vector3>> pivots { get; private set; }
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
        pivots         = new List<List<Vector3>>();        
        _uiFlowObjects = new List<GameObject>();
    }

    void Update()
    {
        if (InputMode.instance.isFlow())
        {
            Vector2 pos = EditorSceneMaster.instance.rootLayer.RelativeCoords(InputManager.instance.inputPosition);
            
            if (!InputMode.instance.isErase())
            {
                if (!_editing && InputManager.instance.held && InputManager.instance.withinImage)
                {
                    _editing = true;
                    _lastPivot = pos;
                }
                if (!InputManager.instance.held || !InputManager.instance.withinImage)
                {
                    _editing = false;
                    _created = false;
                }

                if (_editing)
                {
                    if (Vector2.Distance(_lastPivot, pos) > ADD_THRESHOLD)
                    {
                        if (!_created)
                        {
                            _created = true;

                            _currentPivots = new List<Vector3>();
                            pivots.Add(_currentPivots);
                            _currentPivots.Add(_lastPivot);

                            _currentFlowObject = GameObject.Instantiate(FlowPrefab);
                            _currentFlowObject.name = "Flow " + pivots.Count;
                            _currentFlowObject.GetComponent<RectTransform>().SetParent(EditorSceneMaster.instance.rootLayerObject.transform);
                            _currentLineRenderer = _currentFlowObject.GetComponent<LineRenderer>();
                            _uiFlowObjects.Add(_currentFlowObject);
                        }

                        _lastPivot = pos;
                        _currentPivots.Add(_lastPivot);
                        
                        _currentLineRenderer.positionCount = _currentPivots.Count;
                        _currentLineRenderer.SetPositions(_currentPivots.ToArray());
                    }
                }
            }
            else
            {
                if (InputManager.instance.pressed && InputManager.instance.withinImage)
                {
                    int curveIdx = selectCurve(pos);
                    if (curveIdx != -1)
                    {
                        pivots.RemoveAt(curveIdx);
                        Destroy(_uiFlowObjects[curveIdx]);
                        _uiFlowObjects.RemoveAt(curveIdx);
                    }
                }
            }

            foreach (GameObject obj in _uiFlowObjects)
            {
                if (!obj.activeInHierarchy)
                    obj.SetActive(true);

                obj.GetComponent<RectTransform>().localPosition = new Vector3(
                    -.5f * InputManager.instance.multiplicativeScale * EditorSceneMaster.instance.width,
                    -.5f * InputManager.instance.multiplicativeScale * EditorSceneMaster.instance.height,
                    0
                );
                obj.GetComponent<RectTransform>().localScale = new Vector3(
                    InputManager.instance.multiplicativeScale * EditorSceneMaster.instance.width,
                    InputManager.instance.multiplicativeScale * EditorSceneMaster.instance.height,
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
        float closestDist = 0;

        if (pivots.Count == 0)
            return closestCurve;

        for (int i = 0; i < pivots.Count; i++)
            for (int j = 0; j < pivots[i].Count - 1; j++)
            {
                Vector2 curveDir = (pivots[i][j + 1] - pivots[i][j]).normalized;
                Vector2 posDir   = pos - new Vector2(pivots[i][j].x, pivots[i][j].y);

                float p = Vector2.Dot(curveDir, posDir);
                float curDist = (posDir - p * curveDir).magnitude;

                if (0 < p && p < (pivots[i][j + 1] - pivots[i][j]).magnitude && curDist < ERASE_THRESHOLD)
                    if (closestCurve == -1 || closestDist > curDist)
                    {
                        closestCurve = i;
                        closestDist = curDist;
                    }
            }

        return closestCurve;
    }

}