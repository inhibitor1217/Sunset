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

    private Constants.ModeWaterType _effectType;

    void Awake()
    {
        instance = this;
    }

    public void Init()
    {
        _initialized = true;
 
        RemoveFlow();
    }

    public void Setup(Constants.ModeWaterType effectType, int width, int height)
    {
        if (_effectType == effectType)
            return;
        _effectType = effectType;

        /* SETUP NOISE */
        if (!_noiseProvider && _effectType != Constants.ModeWaterType.NONE)
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
        case Constants.ModeWaterType.NONE:
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
            break;
        case Constants.ModeWaterType.RV01:
        case Constants.ModeWaterType.RV02:
            _effectProvider = gameObject.AddComponent<EffectTexture>();
            EffectTexture river = _effectProvider as EffectTexture;
            river.noiseProvider = _noiseProvider;
            river.paletteProvider = paletteProvider;
            river.environmentProvider = environmentProvider;
            river.Setup(width, height);
            break;
        default:
            break;
        }

        /* DISPATCH ACTIONS */
        switch (_effectType)
        {
        case Constants.ModeWaterType.NONE:
            break;
        case Constants.ModeWaterType.RV01:
            Store.instance.Dispatch(WaterEffectActions.instance.SetupRV01());
            break;
        case Constants.ModeWaterType.RV02:
            Store.instance.Dispatch(WaterEffectActions.instance.SetupRV02());
            break;
        default:
            break;
        }

        /* SET TARGET */
        if (target && _effectType != Constants.ModeWaterType.NONE)
            _effectProvider.SetTarget(target);
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