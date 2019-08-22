using UnityEngine;
using System.Collections.Generic;

public class SharedActions : ActionModule
{

    private static SharedActions _instance;
    public static SharedActions instance
    {
        get {
            if (_instance == null)
                _instance = new SharedActions();
            return _instance;
        }
    }

    private const string ACTION__SET_HORIZON = "SHARED__SET_HORIZON";
    private const string ACTION__SET_EDIT_PARAMETER = "SHARED__SET_EDIT_PARAMETER";

    public const string FIELD__HORIZON         = "Shared__Horizon";
    public const string FIELD__PERSPECTIVE     = "Shared__Perspective";
    public const string FIELD__EDIT_PARAMETER = "Shared__EditParameter";

    public Action SetHorizon(float value) { return new Action<float>(ACTION__SET_HORIZON, value); }
    public Action SetEditParameter(string value) { return new Action<string>(ACTION__SET_EDIT_PARAMETER, value); }

    public override Dictionary<string, object> GetInitialState()
    {
        return new Dictionary<string, object>
        {
            { FIELD__HORIZON,          .65f },
            { FIELD__PERSPECTIVE,      1f   },
            { FIELD__EDIT_PARAMETER, "NONE" },
        };
    }

    public override Dictionary<string, Reducer> GetReducers()
    {
        return new Dictionary<string, Reducer>
        {
            {
                ACTION__SET_HORIZON,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    float payload = ((Action<float>) action).payload;

                    _state[FIELD__HORIZON] = payload;

                    return _state;
                })
            },
            {
                ACTION__SET_EDIT_PARAMETER,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    string payload = ((Action<string>) action).payload;

                    _state[FIELD__EDIT_PARAMETER] = payload;

                    return _state;
                })
            },
        };
    }

}