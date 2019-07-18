using UnityEngine;
using UnityEngine.UI;

public class InputModeToggle : MonoBehaviour
{
    private Toggle m_Toggle;

    public int modeOff;
    public int modeOn;

    void Awake()
    {
        m_Toggle = GetComponent<Toggle>();
    }

    public void OnToggleChanged(bool isOn)
    {
        InputMode.Instance.mode = isOn ? modeOn : modeOff;

        var colors = m_Toggle.colors;
        colors.normalColor = isOn ? Color.yellow : Color.white;
        m_Toggle.colors = colors;
    }
}
