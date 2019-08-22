using UnityEngine;
using UnityEngine.UI;

public class ParameterEditorToggle : MonoBehaviour
{

    private Toggle _toggle;

    public string edit;
    public Color activeColor;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();
    }

    public void Setup()
    {
        Store.instance.Subscribe(
            new string[] { SharedActions.FIELD__EDIT_PARAMETER },
            (state) => {
                bool on = (string)state[SharedActions.FIELD__EDIT_PARAMETER] == edit;
                var colors = _toggle.colors;
                colors.normalColor = on ? activeColor : Color.white;
                _toggle.colors = colors;
            }
        );
    }

    public void OnToggleChanged(bool isOn)
    {
        Store.instance.Dispatch(SharedActions.instance.SetEditParameter(Store.instance.GetValue<string>(SharedActions.FIELD__EDIT_PARAMETER) == edit ? "NONE" : edit));
    }

}