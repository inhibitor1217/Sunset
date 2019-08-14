using UnityEngine;
using UnityEngine.UI;

public class RiverSpeedSlider : MonoBehaviour
{

    private Slider _slider;

    private const float DEFAULT_RIVER_SPEED = .3f;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.value = DEFAULT_RIVER_SPEED;
    }

    void OnEnable()
    {
        Store.instance.Dispatch(WaterEffectActions.instance.SetRiverSpeed(_slider.value));
    }

    public void OnValueChanged()
    {
        Store.instance.Dispatch(WaterEffectActions.instance.SetRiverSpeed(_slider.value));
    }

}