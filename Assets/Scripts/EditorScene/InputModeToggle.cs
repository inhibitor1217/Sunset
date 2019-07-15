using UnityEngine;
using UnityEngine.UI;

public class InputModeToggle : MonoBehaviour
{
    private Toggle m_Toggle;

    public int mode;

    void Awake()
    {
        m_Toggle = GetComponent<Toggle>();
    }

    public void OnToggleChanged(bool isOn)
    {
        InputMode.Instance.mode = isOn ? mode : InputMode.MOVE;

        var colors = m_Toggle.colors;
        colors.normalColor = isOn ? Color.yellow : Color.white;
        m_Toggle.colors = colors;
    }
}
