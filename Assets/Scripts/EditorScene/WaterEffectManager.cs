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
            if (_effectProvider)
                _effectProvider.SetPropertyFloat("Horizon", _horizon);
        }
    }
    [SerializeField, Range(.25f, 4)]
    private float _perspective;
    private const float DEFAULT_SHARED_PERSPECTIVE = 1f;
    public float shared_perspective {
        set {
            _perspective = value;
            if (_effectProvider)
                _effectProvider.SetPropertyFloat("Perspective", _perspective);
        }
    }
    [SerializeField, Range(-30f, 45f)]
    private float _sunAltitude;
    private const float DEFAULT_SHARED_SUN_ALTITUDE = 20f;
    public float shared_sunAltitude {
        set {
            _sunAltitude = value;
            if (_effectProvider)
                _effectProvider.SetPropertyFloat("SunAltitude", _sunAltitude);
        }
    }
    [SerializeField, Range(-45f, 45f)]
    private float _sunDirection;
    private const float DEFAULT_SHARED_SUN_DIRECTION = 0f;
    public float shared_sunDirection {
        set {
            _sunDirection = value;
            if (_effectProvider)
                _effectProvider.SetPropertyFloat("SunDirection", _sunDirection);
        }
    }
    [SerializeField, Range(.1f, 1)]
    private float _relativeSpeed;
    private const float DEFAULT_RIVER_SPEED = .3f;
    private const float MAX_SPEED           = 0.4f;
    private const float MAX_AMPLITUDE       = 1.6f;
    private const float MAX_EVOLUTION_SPEED = 2.4f;
    public float river_speed {
        set {
            _relativeSpeed = value;
            switch(_effectType)
            {
            case NONE:
            case CL01:
            case CL02:
                break;
            case RV01:
                if (_effectProvider)
                    _effectProvider.SetPropertyFloat("Speed"         , MAX_SPEED * _relativeSpeed);
                if (_noiseProvider)
                    _noiseProvider .SetPropertyFloat("Amplitude"     , MAX_AMPLITUDE * _relativeSpeed);
                if (_noiseProvider)
                    _noiseProvider .SetPropertyFloat("EvolutionSpeed", MAX_EVOLUTION_SPEED * _relativeSpeed);
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
            case CL01:
            case CL02:
                break;
            case RV01:
                if (_effectProvider)
                    _effectProvider.SetPropertyFloat("Rotation", _rotation);
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
    public const int CL01  = 1;
    public const int CL02  = 2;
    public const int RV01 = 10;

    public void Setup(int effectType, int width, int height)
    {
        if (effectType == _effectType)
            return;
        _effectType = effectType;

        switch (_effectType)
        {
        case NONE:
            if (_noiseProvider)
                Destroy(_noiseProvider);
            if (_effectProvider)
                Destroy(_effectProvider);
            if (riverOptionPanel)
            {
                riverOptionPanel.SetActive(false);
                InputManager.Instance.optionMenu = null;
            }
            break;
        case CL01:
        case CL02:
            /* SETUP NOISE */
            if (!_noiseProvider)
                _noiseProvider = gameObject.AddComponent<FractalNoiseRuntimeTexture>();

            if (_effectType == CL01)
            {
                _noiseProvider.SetPropertyInt   ("NoiseType"     , 4); // PERLIN_LINEAR
                _noiseProvider.SetPropertyInt   ("FractalType"   , 0); // BASIC
                _noiseProvider.SetPropertyInt   ("Seed"          , 0);
                _noiseProvider.SetPropertyVector("GlobalScale"   , new Vector4(1f/16f, 1f/64f, 16f, 64f));
                _noiseProvider.SetPropertyFloat ("SubInfluence"  , .5f);
                _noiseProvider.SetPropertyVector("SubScale"      , new Vector4(.5f, .5f, 2f, 2f));
                _noiseProvider.SetPropertyFloat ("Brightness"    , 0f);
                _noiseProvider.SetPropertyFloat ("Contrast"      , 1.2f);
                _noiseProvider.SetPropertyFloat ("EvolutionSpeed", 1f);
            }
            else if (_effectType == CL02)
            {
                _noiseProvider.SetPropertyInt   ("NoiseType"     , 4); // PERLIN_LINEAR
                _noiseProvider.SetPropertyInt   ("FractalType"   , 0); // BASIC
                _noiseProvider.SetPropertyInt   ("Seed"          , 0);
                _noiseProvider.SetPropertyVector("GlobalScale"   , new Vector4(1f/64f, 1f/64f, 64f, 64f));
                _noiseProvider.SetPropertyFloat ("SubInfluence"  , .5f);
                _noiseProvider.SetPropertyVector("SubScale"      , new Vector4(.5f, .5f, 2f, 2f));
                _noiseProvider.SetPropertyFloat ("Brightness"    , 0f);
                _noiseProvider.SetPropertyFloat ("Contrast"      , .5f);
                _noiseProvider.SetPropertyFloat ("EvolutionSpeed", 1f);
            }

            /* SETUP EFFECT */
            if (_effectProvider)
                Destroy(_effectProvider);
            _effectProvider = gameObject.AddComponent<CalmEffectTexture>();
            CalmEffectTexture calm  = _effectProvider as CalmEffectTexture;
            calm.SetPropertyProvider("NoiseTexture"      , _noiseProvider);
            calm.SetPropertyProvider("PaletteTexture"    , paletteProvider);
            calm.SetPropertyProvider("EnvironmentTexture", environmentProvider);
            shared_horizon          = _horizon;
            shared_perspective      = _perspective;
            shared_sunAltitude      = _sunAltitude;
            shared_sunDirection     = _sunDirection;
            calm.Setup(width, height);

            /* SETUP UI */
            if (riverOptionPanel)
                riverOptionPanel.SetActive(false);
            break;
        case RV01:
            /* SETUP NOISE */
            if (!_noiseProvider)
                _noiseProvider = gameObject.AddComponent<FractalNoiseRuntimeTexture>();
            _noiseProvider.SetPropertyInt   ("NoiseType"     , 4); // PERLIN_LINEAR
            _noiseProvider.SetPropertyInt   ("FractalType"   , 1); // TURBULENT
            _noiseProvider.SetPropertyInt   ("Seed"          , 0);
            _noiseProvider.SetPropertyVector("GlobalScale"   , new Vector4(1f/4f, 1f/16f, 4f, 16f));
            _noiseProvider.SetPropertyFloat ("SubInfluence"  , .7f);
            _noiseProvider.SetPropertyVector("SubScale"      , new Vector4(.5f, .5f, 2f, 2f));
            _noiseProvider.SetPropertyFloat ("Brightness"    , 0f);
            _noiseProvider.SetPropertyFloat ("Contrast"      , 3f);
            _noiseProvider.SetPropertyFloat ("EvolutionSpeed", 1f);

            /* SETUP EFFECT */
            if (_effectProvider)
                Destroy(_effectProvider);
            _effectProvider = gameObject.AddComponent<RiverEffectTexture>();
            RiverEffectTexture river = _effectProvider as RiverEffectTexture;
            river.SetPropertyProvider("NoiseTexture"      , _noiseProvider);
            river.SetPropertyProvider("PaletteTexture"    , paletteProvider);
            river.SetPropertyProvider("EnvironmentTexture", environmentProvider);
            shared_horizon           = _horizon;
            shared_perspective       = _perspective;
            shared_sunAltitude       = _sunAltitude;
            shared_sunDirection      = _sunDirection;
            river_direction          = _rotation;
            river_speed              = _relativeSpeed;
            river.Setup(width, height);

            /* SETUP UI */
            if (riverOptionPanel)
            {
                riverOptionPanel.SetActive(true);
                InputManager.Instance.optionMenu = riverOptionPanel.GetComponent<RectTransform>();
            }
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