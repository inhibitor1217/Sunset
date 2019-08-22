using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{

    private Slider _slider;

    private string _edit = "NONE";

    void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    public void Setup()
    {
        Store.instance.Subscribe(
            new string[]
            {
                SharedActions.FIELD__EDIT_PARAMETER,
                WaterEffectActions.FIELD__EVOLUTION_SPEED,
                WaterEffectActions.FIELD__ROTATION,
                WaterEffectActions.FIELD__VERTICAL_BLUR_STRENGTH,
                WaterEffectActions.FIELD__VERTICAL_BLUR_WIDTH,
                WaterEffectActions.FIELD__TONE_STRENGTH,
                WaterEffectActions.FIELD__DISTORTION_STRENGTH
            },
            (state) => {
                _edit = (string)state[SharedActions.FIELD__EDIT_PARAMETER];
                switch (_edit)
                {
                    case "NONE":
                        break;
                    case "SPEED":
                        _slider.value = (float)state[WaterEffectActions.FIELD__EVOLUTION_SPEED] / WaterEffectActions.MAX_EVOLUTION_SPEED;
                        break;
                    case "ROTATION":
                        _slider.value = ((float)state[WaterEffectActions.FIELD__ROTATION] - WaterEffectActions.MIN_ROTATION) / (WaterEffectActions.MAX_ROTATION - WaterEffectActions.MIN_ROTATION);
                        break;
                    case "VBS":
                        _slider.value = (float)state[WaterEffectActions.FIELD__VERTICAL_BLUR_STRENGTH] / WaterEffectActions.MAX_VERTICAL_BLUR_STRENGTH;
                        break;
                    case "VBW":
                        _slider.value = (float)state[WaterEffectActions.FIELD__VERTICAL_BLUR_WIDTH] / WaterEffectActions.MAX_VERTICAL_BLUR_WIDTH;
                        break;
                    case "TS":
                        _slider.value = (float)state[WaterEffectActions.FIELD__TONE_STRENGTH] / WaterEffectActions.MAX_TONE_STRENGTH;
                        break;
                    case "DS":
                        _slider.value = (float)state[WaterEffectActions.FIELD__DISTORTION_STRENGTH] / WaterEffectActions.MAX_DISTORTION_STRENGTH;
                        break;
                }
            }
        );
    }

    public void OnValueChanged()
    {
        switch(_edit)
        {
            case "NONE":
                break;
            case "SPEED":
                Store.instance.Dispatch(WaterEffectActions.instance.SetRiverSpeed(_slider.value));
                break;
            case "ROTATION":
                Store.instance.Dispatch(WaterEffectActions.instance.SetRotation(_slider.value));
                break;
            case "VBS":
                Store.instance.Dispatch(WaterEffectActions.instance.SetVBS(_slider.value));
                break;
            case "VBW":
                Store.instance.Dispatch(WaterEffectActions.instance.SetVBW(_slider.value));
                break;
            case "TS":
                Store.instance.Dispatch(WaterEffectActions.instance.SetTS(_slider.value));
                break;
            case "DS":
                Store.instance.Dispatch(WaterEffectActions.instance.SetDS(_slider.value));
                break;
        }
    }

}