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
        InputMode.Subscribe(this);
    }

    void OnDestroy()
    {
        InputMode.Unsubscribe(this);
    }

    public void OnToggleChanged(bool isOn)
    {
        InputMode.Instance.mode = isOn ? modeOn : modeOff;
    }

    public void onInputModeChanged(int mode)
    {
        if (mode != modeOn && mode != modeOff)
        {
            m_Toggle.interactable = false;
            return;
        }
        
        m_Toggle.interactable = true;
        var colors = m_Toggle.colors;
        if (mode == modeOff)
            colors.normalColor = Color.white;
        else if (mode == modeOn)
            colors.normalColor = Color.yellow;
        m_Toggle.colors = colors;
    }
}
