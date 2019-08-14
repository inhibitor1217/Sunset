using UnityEngine;
using System.Collections.Generic;

public class WaterEffectActions : ActionModule
{

    private static WaterEffectActions _instance;
    public static WaterEffectActions instance
    {
        get {
            if (_instance == null)
                _instance = new WaterEffectActions();
            return _instance;
        }
    }

    private const string ACTION__SETUP_CL01      = "WATEREFFECT__SETUP_CL01";
    private const string ACTION__SETUP_CL02      = "WATEREFFECT__SETUP_CL02";
    private const string ACTION__SETUP_RV01      = "WATEREFFECT__SETUP_RV01";
    private const string ACTION__SET_RIVER_SPEED = "WATEREFFECT__SET_RIVER_SPEED";

    public const string FIELD__NOISE_TYPE      = "WaterEffect__NoiseType";
    public const string FIELD__FRACTAL_TYPE    = "WaterEffect__FractalType";
    public const string FIELD__SEED            = "WaterEffect__Seed";
    public const string FIELD__GLOBAL_SCALE    = "WaterEffect__GlobalScale";
    public const string FIELD__SUB_INFLUENCE   = "WaterEffect__SubInfluence";
    public const string FIELD__SUB_SCALE       = "WaterEffect__SubScale";
    public const string FIELD__BRIGHTNESS      = "WaterEffect__Brightness";
    public const string FIELD__CONTRAST        = "WaterEffect__Contrast";
    public const string FIELD__EVOLUTION_SPEED = "WaterEffect__EvolutionSpeed";
    public const string FIELD__AMPLITUDE       = "WaterEffect__Amplitude";
    public const string FIELD__SPEED           = "WaterEffect__Speed";
    public const string FIELD__ROTATION        = "WaterEffect__Rotation";

    private const float MAX_SPEED = .4f;
    private const float MAX_AMPLITUDE = 1.6f;
    private const float MAX_EVOLUTION_SPEED = 2.4f;

    public static string[] fractalNoiseFieldNames;

    public WaterEffectActions()
    {
        fractalNoiseFieldNames = new string[FractalNoiseRuntimeTexture.NUM_FIELDS];
        fractalNoiseFieldNames[FractalNoiseRuntimeTexture.INDEX__AMPLITUDE]       = FIELD__AMPLITUDE;
        fractalNoiseFieldNames[FractalNoiseRuntimeTexture.INDEX__BRIGHTNESS]      = FIELD__BRIGHTNESS;
        fractalNoiseFieldNames[FractalNoiseRuntimeTexture.INDEX__CONTRAST]        = FIELD__CONTRAST;
        fractalNoiseFieldNames[FractalNoiseRuntimeTexture.INDEX__EVOLUTION_SPEED] = FIELD__EVOLUTION_SPEED;
        fractalNoiseFieldNames[FractalNoiseRuntimeTexture.INDEX__FRACTAL_TYPE]    = FIELD__FRACTAL_TYPE;
        fractalNoiseFieldNames[FractalNoiseRuntimeTexture.INDEX__GLOBAL_SCALE]    = FIELD__GLOBAL_SCALE;
        fractalNoiseFieldNames[FractalNoiseRuntimeTexture.INDEX__NOISE_TYPE]      = FIELD__NOISE_TYPE;
        fractalNoiseFieldNames[FractalNoiseRuntimeTexture.INDEX__SEED]            = FIELD__SEED;
        fractalNoiseFieldNames[FractalNoiseRuntimeTexture.INDEX__SUB_INFLUENCE]   = FIELD__SUB_INFLUENCE;
        fractalNoiseFieldNames[FractalNoiseRuntimeTexture.INDEX__SUB_SCALE]       = FIELD__SUB_SCALE;
    }

    public Action SetupCL01() { return new Action(ACTION__SETUP_CL01); }
    public Action SetupCL02() { return new Action(ACTION__SETUP_CL02); }
    public Action SetupRV01() { return new Action(ACTION__SETUP_RV01); }
    public Action SetRiverSpeed(float value) { return new Action<float>(ACTION__SET_RIVER_SPEED, value); }

    public override Dictionary<string, object> GetInitialState()
    {
        return new Dictionary<string, object> 
        {
            { FIELD__NOISE_TYPE,      4 },
            { FIELD__FRACTAL_TYPE,    0 },
            { FIELD__SEED,            0 },
            { FIELD__GLOBAL_SCALE,    8f * Vector2.one },
            { FIELD__SUB_INFLUENCE,   .5f },
            { FIELD__SUB_SCALE,       2f * Vector2.one },
            { FIELD__BRIGHTNESS,      0f },
            { FIELD__CONTRAST,        1f },
            { FIELD__EVOLUTION_SPEED, 0f },
            { FIELD__AMPLITUDE,       1f },
            { FIELD__SPEED,           0f },
            { FIELD__ROTATION,        0f },
        };
    }

    public override Dictionary<string, Reducer> GetReducers()
    {
        return new Dictionary<string, Reducer>
        {
            {
                ACTION__SETUP_CL01,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    
                    _state[FIELD__NOISE_TYPE]      = 4;
                    _state[FIELD__FRACTAL_TYPE]    = 0;
                    _state[FIELD__SEED]            = 0;
                    _state[FIELD__GLOBAL_SCALE]    = new Vector2(16f, 64f);
                    _state[FIELD__SUB_INFLUENCE]   = .5f;
                    _state[FIELD__SUB_SCALE]       = new Vector2(2f, 2f);
                    _state[FIELD__BRIGHTNESS]      = 0f;
                    _state[FIELD__CONTRAST]        = 1.2f;
                    _state[FIELD__EVOLUTION_SPEED] = 1f;
                    _state[FIELD__SPEED]           = 0f;
                    
                    return _state;
                })
            },
            {
                ACTION__SETUP_CL02,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    
                    _state[FIELD__NOISE_TYPE]      = 4;
                    _state[FIELD__FRACTAL_TYPE]    = 0;
                    _state[FIELD__SEED]            = 0;
                    _state[FIELD__GLOBAL_SCALE]    = new Vector2(64f, 64f);
                    _state[FIELD__SUB_INFLUENCE]   = .5f;
                    _state[FIELD__SUB_SCALE]       = new Vector2(2f, 2f);
                    _state[FIELD__BRIGHTNESS]      = 0f;
                    _state[FIELD__CONTRAST]        = .5f;
                    _state[FIELD__EVOLUTION_SPEED] = 1f;
                    _state[FIELD__SPEED]           = 0f;
                    
                    return _state;
                })
            },
            {
                ACTION__SETUP_RV01,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    
                    _state[FIELD__NOISE_TYPE]    = 4;
                    _state[FIELD__FRACTAL_TYPE]  = 1;
                    _state[FIELD__SEED]          = 0;
                    _state[FIELD__GLOBAL_SCALE]  = new Vector2(4f, 16f);
                    _state[FIELD__SUB_INFLUENCE] = .7f;
                    _state[FIELD__SUB_SCALE]     = new Vector2(2f, 2f);
                    _state[FIELD__BRIGHTNESS]    = 0f;
                    _state[FIELD__CONTRAST]      = 3f;
                    
                    return _state;
                })
            },
            {
                ACTION__SET_RIVER_SPEED,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    float payload = ((Action<float>) action).payload;

                    Debug.Log("Set River Speed");

                    _state[FIELD__EVOLUTION_SPEED] = MAX_EVOLUTION_SPEED * payload;
                    _state[FIELD__AMPLITUDE]       = MAX_AMPLITUDE * payload;
                    _state[FIELD__SPEED]           = MAX_SPEED * payload;

                    return _state;
                })
            },
        };
    }

}