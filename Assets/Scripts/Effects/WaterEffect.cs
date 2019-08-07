using UnityEngine;

public class WaterEffect : MonoBehaviour
{

    private FractalNoiseRuntimeTexture _noiseProvider;
    private EffectTexture _effectProvider;

    [Header("Shared Textures")]
    public TextureProvider paletteProvider;
    public TextureProvider environmentProvider;

    [Header("Render Target")]
    public RawImageController target;

    // [Header("Properties")]
    // [SerializeField, Range(-30f, 45f)]
    // private float _sunAltitude = 30f;
    // public float shared_sunAltitude {
    //     set {
    //         _sunAltitude = value;
    //         if (_effectType != NONE)
    //             setSunAltitude(_sunAltitude);
    //     }
    // }
    // void setSunAltitude(float value)
    // {
    //     if (_effectProvider)
    //         _effectProvider.sunAltitude = value;
    // }
    // [SerializeField, Range(-45f, 45f)]
    // private float _sunDirection = 0f;
    // public float shared_sunDirection {
    //     set {
    //         _sunDirection = value;
    //         if (_effectType != NONE)
    //             setSunDirection(_sunDirection);
    //     }
    // }
    // void setSunDirection(float value)
    // {
    //     if (_effectProvider)
    //         _effectProvider.sunDirection = _sunDirection;
    // }
    // [SerializeField, Range(.1f, 1)]
    // private float _relativeSpeed = .1f;
    // private const float MAX_SPEED           = 0.8f;
    // private const float MAX_AMPLITUDE       = 2.5f;
    // private const float MAX_EVOLUTION_SPEED = 3f;
    // public float river_speed {
    //     set {
    //         _relativeSpeed = value;
    //         if (_effectType == RIVER)
    //             setRiverSpeed(_relativeSpeed);
    //     }
    // }
    // void setRiverSpeed(float value)
    // {
    //     if (_effectProvider)
    //     {
    //         _effectProvider.speed     = MAX_SPEED * _relativeSpeed;
    //         _effectProvider.amplitude = MAX_AMPLITUDE * _relativeSpeed;
    //         _noiseProvider.evolutionSpeed = MAX_EVOLUTION_SPEED * _relativeSpeed;
    //     }
    // }
    // [SerializeField, Range(-180, 180)]
    // private float _rotation = 0f;
    // public float river_direction {
    //     set {
    //         _rotation = value;
    //         if (_effectType == RIVER)
    //             setRiverDirection(_rotation);
    //     }
    // }
    // void setRiverDirection(float value)
    // {
    //     if (_effectProvider)
    //     {
    //         _effectProvider.rotation = _rotation;
    //     }
    // }

// #if UNITY_EDITOR
//     void OnValidate()
//     {
//         shared_sunAltitude  = _sunAltitude;
//         shared_sunDirection = _sunDirection;
//         river_speed         = _relativeSpeed;
//         river_direction     = _rotation;
//     }
// #endif

    private int _effectType;
    public const int NONE = 0;
    public const int CALM  = 1;
    public const int RIVER = 2;

    public void Setup(int effectType, int width, int height)
    {
        _effectType = effectType;

        if (!_noiseProvider)
            _noiseProvider = gameObject.AddComponent<FractalNoiseRuntimeTexture>();

        /* SETUP NOISE */
        switch (effectType)
        {
        case CALM:
            _noiseProvider.noiseType       = 4; // PERLIN_LINEAR
            _noiseProvider.fractalType     = 0; // BASIC
            _noiseProvider.scale           = new Vector2(16, 128);
            _noiseProvider.complexity      = 3;
            _noiseProvider.subInfluence    = .5f;
            _noiseProvider.subScale        = 2f * Vector2.one;
            _noiseProvider.brightness      = -.5f;
            _noiseProvider.contrast        = 2f;
            // _noiseProvider.enableEvolution = true;
            // _noiseProvider.evolutionSpeed  = 1f;
            break;
        case RIVER:
            _noiseProvider.noiseType       = 4; // PERLIN LINEAR
            _noiseProvider.fractalType     = 1; // TURBULENT
            _noiseProvider.scale           = new Vector2(8, 32);
            _noiseProvider.complexity      = 3;
            _noiseProvider.subInfluence    = .7f;
            _noiseProvider.subScale        = 2f * Vector2.one;
            _noiseProvider.brightness      = 0f;
            _noiseProvider.contrast        = 3f;
            // _noiseProvider.enableEvolution = true;
            // _noiseProvider.evolutionSpeed  = 1f;
            break;
        default:
            break;
        }

        if (!_effectProvider)
        {
            _effectProvider = gameObject.AddComponent<EffectTexture>();
            _effectProvider.noiseTexture       = _noiseProvider;
            _effectProvider.paletteTexture     = paletteProvider;
            _effectProvider.environmentTexture = environmentProvider;
        }

        _effectProvider.Setup(width, height);
        _effectProvider.effectType = _effectType;

        // // shared
        // shared_sunAltitude  = _sunAltitude;
        // shared_sunDirection = _sunDirection;
        // // type-specific
        // if (_effectType == CALM)
        // {

        // }
        // else
        // {

        // }

        // if (_effectType == RIVER)
        // {
        //     river_speed     = _relativeSpeed;
        //     river_direction = _rotation;
        // }
        // else
        // {
        //     setRiverSpeed(0);
        //     setRiverDirection(0);
        // }
        
        if (target)
            _effectProvider.SetTarget(target);
    }

}