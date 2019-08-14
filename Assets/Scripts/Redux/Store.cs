using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

public class Store : MonoBehaviour
{

    public static Store instance { get; private set; }
    public delegate void SubscriptionFunction(object payload);

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

    private Dictionary<string, Dictionary<int, SubscriptionFunction>> _subscriptions;
    private Queue<Action> _actions;

    void Awake()
    {
        instance = this;

        _actions = new Queue<Action>();
    }

    public void Init(Dictionary<string, object> initialState, Dictionary<string, Reducer> reducers)
    {
        _initialized = true;
        
        _state    = initialState;
        _reducers = reducers;

        _subscriptions = new Dictionary<string, Dictionary<int, SubscriptionFunction>>();
        foreach (string key in initialState.Keys)
        {
            _subscriptions[key] = new Dictionary<int, SubscriptionFunction>();
        }
    }

    void Update()
    {
        if (!_initialized)
            return;

        Dictionary<string, object> state = _state;

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

#if UNITY_EDITOR
        bool valueChanged = false;
#endif

        foreach (string key in _state.Keys)
        {
            if (!_state[key].Equals(state[key]))
            {
#if UNITY_EDITOR
                valueChanged = true;
#endif
                foreach (SubscriptionFunction func in _subscriptions[key].Values)
                {
                    func(state[key]);
                }
            }
        }

#if UNITY_EDITOR
        if (valueChanged)
            if (EditorApplication.isPlaying)
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

    public int Subscribe(string key, SubscriptionFunction func)
    {
        int subscriptionID = key.GetHashCode() ^ func.GetHashCode();
        _subscriptions[key][subscriptionID] = func;
        func(_state[key]);
        return subscriptionID;
    }

    public void Unsubscribe(string key, int subscriptionID)
    {
        if (_subscriptions[key].ContainsKey(subscriptionID))
            _subscriptions[key].Remove(subscriptionID);
#if UNITY_EDITOR
        else
            Debug.Log("Store: Subscription for " + key + " does not contain ID " + subscriptionID);
#endif
    }

#if UNITY_EDITOR
    /* DANGEROUS: ONLY FOR EDITOR */
    public Dictionary<string, object> GetState() { return _state; }
    public Dictionary<string, Reducer> GetReducers() { return _reducers; }
    public void SetValue(string key, object value)
    {
        _state[key] = value;
        foreach (SubscriptionFunction func in _subscriptions[key].Values)
        {
            func(value);
        }
    }
#endif

}