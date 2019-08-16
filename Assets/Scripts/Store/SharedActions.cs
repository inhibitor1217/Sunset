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

    public const string FIELD__HORIZON         = "Shared__Horizon";
    public const string FIELD__PERSPECTIVE     = "Shared__Perspective";
    public const string FIELD__LIGHT_DIRECTION = "Shared__LightDirection";

    public Action SetHorizon(float value) { return new Action<float>(ACTION__SET_HORIZON, value); }

    public override Dictionary<string, object> GetInitialState()
    {
        return new Dictionary<string, object>
        {
            { FIELD__HORIZON,          .65f },
            { FIELD__PERSPECTIVE,      1f   },
            { FIELD__LIGHT_DIRECTION,  new Vector4(0f, 0.939693f, 0.342020f, 0f) },
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
            }
        };
    }

}