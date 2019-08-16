using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

public class Store : MonoBehaviour
{

    public static Store instance { get; private set; }
    public delegate void SubscriptionFunction(Dictionary<string, object> state);

    public static Dictionary<string, object> MergeInitialStates(ActionModule[] modules)
    {
        var initialState = new Dictionary<string, object>();
        foreach (var module in modules)
        {
            var _initialState = module.GetInitialState();
            foreach (string key in _initialState.Keys)
                initialState[key] = _initialState[key];
        }
        return initialState;
    }

    public static Dictionary<string, Reducer> MergeReducers(ActionModule[] modules)
    {
        var reducers = new Dictionary<string, Reducer>();
        foreach (var module in modules)
        {
            var _reducers = module.GetReducers();
            foreach (string key in _reducers.Keys)
                reducers[key] = _reducers[key];
        }
        return reducers;
    }

    private bool _initialized = false;
    private Dictionary<string, object> _state;
    private Dictionary<string, Reducer> _reducers;
    private Dictionary<HashSet<string>, List<int>> _subscriptions;
    private Dictionary<int, SubscriptionFunction>  _functionMap;
    private Queue<Action> _actions;
    private HashSet<string> _updatedKeys;

    void Awake()
    {
        instance = this;

        _subscriptions = new Dictionary<HashSet<string>, List<int>>(HashSet<string>.CreateSetComparer());
        _functionMap   = new Dictionary<int, SubscriptionFunction>();
        _actions       = new Queue<Action>();
        _updatedKeys   = new HashSet<string>();
    }

    public void Init(Dictionary<string, object> initialState, Dictionary<string, Reducer> reducers)
    {
        _initialized = true;
        
        foreach (var set in _subscriptions.Keys)
        {
            set.Clear();
            _subscriptions[set].Clear();
        }
        _subscriptions.Clear();
        _functionMap.Clear();
        _actions.Clear();
        _updatedKeys.Clear();

        _state    = new Dictionary<string, object>(initialState);
        _reducers = new Dictionary<string, Reducer>(reducers);
    }

    void Update()
    {
        if (!_initialized)
            return;

        Dictionary<string, object> state = _state;

        /* PROCESS ACTIONS */
        while (_actions.Count > 0)
        {
            Action action = _actions.Dequeue();
            if (_reducers.ContainsKey(action.type))
            {
                state = _reducers[action.type].func(state, action);
            }
#if UNITY_EDITOR
            else
                Debug.Log("Store: No reducer available for action type " + action.type);
#endif
        }

        /* DETECT CHANGES */
#if UNITY_EDITOR
        bool valueChanged = false;
#endif
        _updatedKeys.Clear();

        foreach (string key in _state.Keys)
        {
            if (!_state[key].Equals(state[key]))
            {
#if UNITY_EDITOR
                valueChanged = true;
#endif
                _updatedKeys.Add(key);
            }
        }

        /* NOTIFY SUBSCRIPTORS */
        foreach (var keySet in _subscriptions.Keys)
        {
            if (keySet.Overlaps(_updatedKeys))
                foreach (int id in _subscriptions[keySet])
                {
                    _functionMap[id](state);
                }
        }

#if UNITY_EDITOR
        if (valueChanged)
            if (EditorApplication.isPlaying && StoreEditor.instance)
                StoreEditor.instance.Repaint();
#endif

        _state = state;
    }

    public T GetValue<T> (string key)
    {
        return (T)_state[key];
    }

    public void Dispatch(Action action)
    {
        _actions.Enqueue(action);
    }

    public int Subscribe(string[] keys, SubscriptionFunction func)
    {
        int subscriptionID = func.GetHashCode();
        HashSet<string> keySet = new HashSet<string>(keys);

        if (!_subscriptions.ContainsKey(keySet))
            _subscriptions[keySet] = new List<int>();
        _subscriptions[keySet].Add(subscriptionID);
        _functionMap[subscriptionID] = func;

        func(_state);

        return subscriptionID;
    }

    public void Unsubscribe(int subscriptionID)
    {
        foreach (var keySet in _subscriptions.Keys)
        {
            _subscriptions[keySet].Remove(subscriptionID);
        }
    }

#if UNITY_EDITOR
    /* DANGEROUS: ONLY FOR EDITOR */
    public Dictionary<string, object> GetState() { return _state; }
    public Dictionary<string, Reducer> GetReducers() { return _reducers; }
    public void SetValue(string key, object value)
    {
        _state[key] = value;
        foreach (var keySet in _subscriptions.Keys)
        {
            if (keySet.Contains(key))
                foreach (int id in _subscriptions[keySet])
                {
                    _functionMap[id](_state);
                }
        }
    }
#endif

}