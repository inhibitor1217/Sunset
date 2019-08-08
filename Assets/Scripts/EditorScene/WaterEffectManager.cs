using UnityEngine;
using UnityEngine.UI;

public class WaterEffectManager : MonoBehaviour
{

    private FractalNoiseRuntimeTexture _noiseProvider;
    private FlowTexture _flowProvider;
    private TextureProvider _effectProvider;
    [Header("UIs")]
    public GameObject riverOptionPanel;
    public Slider riverSpeedSlider;

    [Header("Shared Textures")]
    public TextureProvider paletteProvider;
    public TextureProvider environmentProvider;

    [Header("Render Target")]
    public RawImageController target;

    [Header("Properties")]
    [SerializeField, Range(0, 1.5f)]
    private float _horizon;
    private const float DEFAULT_SHARED_HORIZON = .65f;
    public float shared_horizon {
        set {
            _horizon = value;
            switch(_effectType)
            {
            case NONE:
                break;
            case CALM:
                (_effectProvider as CalmEffectTexture).horizon = _horizon;
                break;
            case RIVER:
                (_effectProvider as RiverEffectTexture).horizon = _horizon;
                break;
            }
        }
    }
    [SerializeField, Range(.25f, 4)]
    private float _perspective;
    private const float DEFAULT_SHARED_PERSPECTIVE = 1f;
    public float shared_perspective {
        set {
            _perspective = value;
            switch(_effectType)
            {
            case NONE:
                break;
            case CALM:
                (_effectProvider as CalmEffectTexture).perspective = _perspective;
                break;
            case RIVER:
                (_effectProvider as RiverEffectTexture).perspective = _perspective;
                break;
            }
        }
    }
    [SerializeField, Range(-30f, 45f)]
    private float _sunAltitude;
    private const float DEFAULT_SHARED_SUN_ALTITUDE = 20f;
    public float shared_sunAltitude {
        set {
            _sunAltitude = value;
            switch(_effectType)
            {
            case NONE:
                break;
            case CALM:
                (_effectProvider as CalmEffectTexture).sunAltitude = _sunAltitude;
                break;
            case RIVER:
                (_effectProvider as RiverEffectTexture).sunAltitude = _sunAltitude;
                break;
            }
        }
    }
    [SerializeField, Range(-45f, 45f)]
    private float _sunDirection;
    private const float DEFAULT_SHARED_SUN_DIRECTION = 0f;
    public float shared_sunDirection {
        set {
            _sunDirection = value;
            switch(_effectType)
            {
            case NONE:
                break;
            case CALM:
                (_effectProvider as CalmEffectTexture).sunDirection = _sunDirection;
                break;
            case RIVER:
                (_effectProvider as RiverEffectTexture).sunDirection = _sunDirection;
                break;
            }
        }
    }
    [SerializeField, Range(.1f, 1)]
    private float _relativeSpeed;
    private const float DEFAULT_RIVER_SPEED = .3f;
    private const float MAX_SPEED           = 0.8f;
    private const float MAX_AMPLITUDE       = 2.5f;
    private const float MAX_EVOLUTION_SPEED = 3f;
    public float river_speed {
        set {
            _relativeSpeed = value;
            switch(_effectType)
            {
            case NONE:
                break;
            case CALM:
                break;
            case RIVER:
                (_effectProvider as RiverEffectTexture).speed     = MAX_SPEED * _relativeSpeed;
                (_effectProvider as RiverEffectTexture).amplitude = MAX_AMPLITUDE * _relativeSpeed;
                _noiseProvider.evolutionSpeed                     = MAX_EVOLUTION_SPEED * _relativeSpeed;
                break;
            }
        }
    }
    [SerializeField, Range(-180, 180)]
    private float _rotation;
    private const float DEFAULT_RIVER_DIRECTION = 0f;
    public float river_direction {
        set {
            _rotation = value;
            switch(_effectType)
            {
            case NONE:
                break;
            case CALM:
                break;
            case RIVER:
                (_effectProvider as RiverEffectTexture).rotation = _rotation;
                break;
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        shared_horizon      = _horizon;
        shared_perspective  = _perspective;
        shared_sunAltitude  = _sunAltitude;
        shared_sunDirection = _sunDirection;
        river_speed         = _relativeSpeed;
        river_direction     = _rotation;
    }
#endif

    public void Init()
    {
        shared_horizon      = DEFAULT_SHARED_HORIZON;
        shared_perspective  = DEFAULT_SHARED_PERSPECTIVE;
        shared_sunAltitude  = DEFAULT_SHARED_SUN_ALTITUDE;
        shared_sunDirection = DEFAULT_SHARED_SUN_DIRECTION;
        river_speed         = DEFAULT_RIVER_SPEED;
        river_direction     = DEFAULT_RIVER_DIRECTION;

        if (riverOptionPanel)
            riverOptionPanel.SetActive(false);
        if (riverSpeedSlider)
        {
            riverSpeedSlider.value = DEFAULT_RIVER_SPEED;
            riverSpeedSlider.onValueChanged.AddListener((value) => {
                river_speed = value;
            });
        }

        RemoveFlow();
    }

    void Start()
    {
        Init();
    }

    private int _effectType;
    public const int NONE = 0;
    public const int CALM  = 1;
    public const int RIVER = 2;

    public void Setup(int effectType, int width, int height)
    {
        if (effectType == _effectType)
            return;
        _effectType = effectType;

        switch (effectType)
        {
        case NONE:
            if (_noiseProvider)
                Destroy(_noiseProvider);
            if (_effectProvider)
                Destroy(_effectProvider);
            if (riverOptionPanel)
                riverOptionPanel.SetActive(false);
            break;
        case CALM:
            /* SETUP NOISE */
            if (!_noiseProvider)
                _noiseProvider = gameObject.AddComponent<FractalNoiseRuntimeTexture>();
            _noiseProvider.noiseType       = 4; // PERLIN_LINEAR
            _noiseProvider.fractalType     = 0; // BASIC
            _noiseProvider.scale           = new Vector2(16, 128);
            _noiseProvider.complexity      = 3;
            _noiseProvider.subInfluence    = .5f;
            _noiseProvider.subScale        = 2f * Vector2.one;
            _noiseProvider.brightness      = -.5f;
            _noiseProvider.contrast        = 2f;
            _noiseProvider.enableEvolution = true;
            _noiseProvider.evolutionSpeed  = 1f;

            /* SETUP EFFECT */
            if (_effectProvider)
                Destroy(_effectProvider);
            _effectProvider = gameObject.AddComponent<CalmEffectTexture>();
            CalmEffectTexture calm  = _effectProvider as CalmEffectTexture;
            calm.noiseTexture       = _noiseProvider;
            calm.paletteTexture     = paletteProvider;
            calm.environmentTexture = environmentProvider;
            shared_horizon          = _horizon;
            shared_perspective      = _perspective;
            shared_sunAltitude      = _sunAltitude;
            shared_sunDirection     = _sunDirection;
            calm.Setup(width, height);

            /* SETUP UI */
            if (riverOptionPanel)
                riverOptionPanel.SetActive(false);
            break;
        case RIVER:
            /* SETUP NOISE */
            if (!_noiseProvider)
                _noiseProvider = gameObject.AddComponent<FractalNoiseRuntimeTexture>();
            _noiseProvider.noiseType       = 4; // PERLIN LINEAR
            _noiseProvider.fractalType     = 1; // TURBULENT
            _noiseProvider.scale           = new Vector2(8, 32);
            _noiseProvider.complexity      = 3;
            _noiseProvider.subInfluence    = .7f;
            _noiseProvider.subScale        = 2f * Vector2.one;
            _noiseProvider.brightness      = 0f;
            _noiseProvider.contrast        = 3f;
            _noiseProvider.enableEvolution = true;
            _noiseProvider.evolutionSpeed  = 1f;

            /* SETUP EFFECT */
            if (_effectProvider)
                Destroy(_effectProvider);
            _effectProvider = gameObject.AddComponent<RiverEffectTexture>();
            RiverEffectTexture river = _effectProvider as RiverEffectTexture;
            river.noiseTexture       = _noiseProvider;
            river.paletteTexture     = paletteProvider;
            river.environmentTexture = environmentProvider;
            shared_horizon           = _horizon;
            shared_perspective       = _perspective;
            shared_sunAltitude       = _sunAltitude;
            shared_sunDirection      = _sunDirection;
            river_direction          = _rotation;
            river_speed              = _relativeSpeed;
            river.Setup(width, height);

            /* SETUP UI */
            if (riverOptionPanel)
                riverOptionPanel.SetActive(true);
            break;
        default:
            break;
        }
        
        if (target)
            _effectProvider.SetTarget(target);
    }

    public void CreateFlow(Mesh flowVectorMesh)
    {
        if (!_flowProvider)
        {
            _flowProvider = gameObject.AddComponent<FlowTexture>();
            _flowProvider.flowVectorMesh = flowVectorMesh;
        }
    }

    public void RemoveFlow()
    {
        if (_flowProvider)
            Destroy(_flowProvider);
    }

}