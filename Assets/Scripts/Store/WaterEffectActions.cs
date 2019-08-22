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

    private const string ACTION__SETUP_RV01      = "WATEREFFECT__SETUP_RV01";
    private const string ACTION__SETUP_RV02      = "WATEREFFECT__SETUP_RV02";
    private const string ACTION__SET_RIVER_SPEED = "WATEREFFECT__SET_RIVER_SPEED";
    private const string ACTION__SET_ROTATION = "WATEREFFECT__SET_ROTATION";
    private const string ACTION__SET_VBW = "WATEREFFECT__SET_VBW";
    private const string ACTION__SET_VBS = "WATEREFFECT__SET_VBS";
    private const string ACTION__SET_DS = "WATEREFFECT__SET_DS";
    private const string ACTION__SET_TS = "WATEREFFECT__SET_TS";

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
    public const string FIELD__ROTATION        = "WaterEffect__Rotation";
    public const string FIELD__VERTICAL_BLUR_WIDTH = "WaterEffect__VerticalBlurWidth";
    public const string FIELD__VERTICAL_BLUR_STRENGTH = "WaterEffect__VerticalBlurStrength";
    public const string FIELD__DISTORTION_STRENGTH = "WaterEffect__DistortionStrength";
    public const string FIELD__TONE_STRENGTH = "WaterEffect__ToneStrength";

    public static string[] fractalNoiseFieldNames;

    public const float MAX_AMPLITUDE = .75f;
    public const float MAX_EVOLUTION_SPEED = 1.0f;
    public const float MIN_ROTATION = -45f * Mathf.Deg2Rad;
    public const float MAX_ROTATION = 45f * Mathf.Deg2Rad;
    public const float MAX_VERTICAL_BLUR_WIDTH = .3f;
    public const float MAX_VERTICAL_BLUR_STRENGTH = 2.5f;
    public const float MAX_DISTORTION_STRENGTH = 2.5f;
    public const float MAX_TONE_STRENGTH = 1f;

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

    public Action SetupRV01() { return new Action(ACTION__SETUP_RV01); }
    public Action SetupRV02() { return new Action(ACTION__SETUP_RV02); }
    public Action SetRiverSpeed(float value) { return new Action<float>(ACTION__SET_RIVER_SPEED, value); }
    public Action SetRotation(float value) { return new Action<float>(ACTION__SET_ROTATION, value); }
    public Action SetVBW(float value) { return new Action<float>(ACTION__SET_VBW, value); }
    public Action SetVBS(float value) { return new Action<float>(ACTION__SET_VBS, value); }
    public Action SetDS(float value) { return new Action<float>(ACTION__SET_DS, value); }
    public Action SetTS(float value) { return new Action<float>(ACTION__SET_TS, value); }

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
            { FIELD__EVOLUTION_SPEED, .5f },
            { FIELD__AMPLITUDE,       .375f },
            { FIELD__ROTATION,        0f },
            { FIELD__VERTICAL_BLUR_WIDTH, .12f },
            { FIELD__VERTICAL_BLUR_STRENGTH, 1f },
            { FIELD__DISTORTION_STRENGTH, 1f },
            { FIELD__TONE_STRENGTH, .5f },
        };
    }

    public override Dictionary<string, Reducer> GetReducers()
    {
        return new Dictionary<string, Reducer>
        {
            {
                ACTION__SETUP_RV01,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    
                    _state[FIELD__NOISE_TYPE]    = 4;
                    _state[FIELD__FRACTAL_TYPE]  = 0;
                    _state[FIELD__SEED]          = 0;
                    _state[FIELD__GLOBAL_SCALE]  = new Vector2(4f, 16f);
                    _state[FIELD__SUB_INFLUENCE] = .7f;
                    _state[FIELD__SUB_SCALE]     = new Vector2(2f, 2f);
                    _state[FIELD__BRIGHTNESS]    = 0f;
                    _state[FIELD__CONTRAST]      = 3f;

                    _state[FIELD__EVOLUTION_SPEED] = .5f;
                    _state[FIELD__AMPLITUDE] = .375f;
                    _state[FIELD__VERTICAL_BLUR_WIDTH] = .12f;
                    _state[FIELD__VERTICAL_BLUR_STRENGTH] = 1f;
                    _state[FIELD__DISTORTION_STRENGTH] = 1f;
                    _state[FIELD__TONE_STRENGTH] = .5f;
                    
                    return _state;
                })
            },
            {
                ACTION__SETUP_RV02,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);

                    _state[FIELD__NOISE_TYPE]    = 4;
                    _state[FIELD__FRACTAL_TYPE]  = 0;
                    _state[FIELD__SEED]          = 0;
                    _state[FIELD__GLOBAL_SCALE]  = new Vector2(4f, 16f);
                    _state[FIELD__SUB_INFLUENCE] = .7f;
                    _state[FIELD__SUB_SCALE]     = new Vector2(2f, 2f);
                    _state[FIELD__BRIGHTNESS]    = 0f;
                    _state[FIELD__CONTRAST]      = 3f;

                    _state[FIELD__EVOLUTION_SPEED] = .16f;
                    _state[FIELD__AMPLITUDE] = .12f;
                    _state[FIELD__VERTICAL_BLUR_WIDTH] = 0f;
                    _state[FIELD__VERTICAL_BLUR_STRENGTH] = 0f;
                    _state[FIELD__DISTORTION_STRENGTH] = 2.45f;
                    _state[FIELD__TONE_STRENGTH] = .1f;

                    return _state;
                })
            },
            {
                ACTION__SET_RIVER_SPEED,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    float payload = ((Action<float>) action).payload;

                    _state[FIELD__EVOLUTION_SPEED] = MAX_EVOLUTION_SPEED * payload;
                    _state[FIELD__AMPLITUDE]       = MAX_AMPLITUDE * payload;

                    return _state;
                })
            },
            {
                ACTION__SET_ROTATION,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    float payload = ((Action<float>) action).payload;

                    _state[FIELD__ROTATION] = Mathf.Lerp(MIN_ROTATION, MAX_ROTATION, payload);

                    return _state;
                })
            },
            {
                ACTION__SET_VBW,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    float payload = ((Action<float>) action).payload;

                    _state[FIELD__VERTICAL_BLUR_WIDTH] = MAX_VERTICAL_BLUR_WIDTH * payload;

                    return _state;
                })
            },
            {
                ACTION__SET_VBS,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    float payload = ((Action<float>) action).payload;

                    _state[FIELD__VERTICAL_BLUR_STRENGTH] = MAX_VERTICAL_BLUR_STRENGTH * payload;

                    return _state;
                })
            },
            {
                ACTION__SET_TS,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    float payload = ((Action<float>) action).payload;

                    _state[FIELD__TONE_STRENGTH] = MAX_TONE_STRENGTH * payload;

                    return _state;
                })
            },
            {
                ACTION__SET_DS,
                new Reducer((state, action) => {
                    var _state = new Dictionary<string, object>(state);
                    float payload = ((Action<float>) action).payload;

                    _state[FIELD__DISTORTION_STRENGTH] = MAX_DISTORTION_STRENGTH * payload;

                    return _state;
                })
            },
        };
    }

}