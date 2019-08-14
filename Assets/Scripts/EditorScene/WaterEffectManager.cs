using UnityEngine;

public class WaterEffectManager : MonoBehaviour
{

    public static WaterEffectManager instance { get; private set; }

    private bool _initialized = false;

    private FractalNoiseRuntimeTexture _noiseProvider;
    private FlowTexture _flowProvider;
    private TextureProvider _effectProvider;
    [Header("UIs")]
    public GameObject riverOptionPanel;

    [Header("Shared Textures")]
    public TextureProvider paletteProvider;
    public TextureProvider environmentProvider;

    [Header("Render Target")]
    public RawImageController target;

    void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        _initialized = true;
 
        RemoveFlow();
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

        /* SETUP NOISE */
        if (!_noiseProvider && _effectType != NONE)
        {
            _noiseProvider = gameObject.AddComponent<FractalNoiseRuntimeTexture>();
            _noiseProvider.fieldNames = WaterEffectActions.fractalNoiseFieldNames;
            _noiseProvider.Setup();
        }

        /* SETUP EFFECT */
        if (_effectProvider)
            Destroy(_effectProvider);

        switch (_effectType)
        {
        case NONE:
            if (_noiseProvider)
            {
                Destroy(_noiseProvider);
                _noiseProvider = null;
            }
            if (_effectProvider)
            {
                Destroy(_effectProvider);
                _effectProvider = null;
            }
            
            riverOptionPanel.SetActive(false);
            InputManager.instance.optionMenu = null;
            break;
        case CL01:
        case CL02:
            _effectProvider = gameObject.AddComponent<CalmEffectTexture>();
            CalmEffectTexture calm  = _effectProvider as CalmEffectTexture;
            calm.noiseProvider = _noiseProvider;
            calm.paletteProvider = paletteProvider;
            calm.environmentProvider = environmentProvider;
            calm.Setup(width, height);

            /* SETUP UI */
            riverOptionPanel.SetActive(false);
            break;
        case RV01:
            _effectProvider = gameObject.AddComponent<RiverEffectTexture>();
            RiverEffectTexture river = _effectProvider as RiverEffectTexture;
            river.noiseProvider = _noiseProvider;
            river.paletteProvider = paletteProvider;
            river.environmentProvider = environmentProvider;
            river.Setup(width, height);

            /* SETUP UI */
            riverOptionPanel.SetActive(true);
            InputManager.instance.optionMenu = riverOptionPanel.GetComponent<RectTransform>();
            break;
        default:
            break;
        }
        
        if (target)
            _effectProvider.SetTarget(target);

        /* DISPATCH ACTIONS */
        switch (_effectType)
        {
        case NONE:
            break;
        case CL01:
            Store.instance.Dispatch(WaterEffectActions.instance.SetupCL01());
            break;
        case CL02:
            Store.instance.Dispatch(WaterEffectActions.instance.SetupCL02());
            break;
        case RV01:
            Store.instance.Dispatch(WaterEffectActions.instance.SetupRV01());
            break;
        default:
            break;
        }
    }

    public void CreateFlow(FlowController controller)
    {
        if (!_flowProvider)
        {
            _flowProvider = gameObject.AddComponent<FlowTexture>();
        }

        _flowProvider.GenerateTexture(controller);
    }

    public void RemoveFlow()
    {
        if (_flowProvider)
            Destroy(_flowProvider);
    }

}