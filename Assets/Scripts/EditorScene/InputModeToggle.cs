using UnityEngine;
using UnityEngine.UI;

public class InputModeToggle : MonoBehaviour
{
    private Toggle m_Toggle;

    public int modeToggle;
    public int modeInteractable = -1;
    public int modeInteractable2 = -1;
    public int modeNotInteractable;
    public int modeColorOn;
    public Color activeColor;

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
        InputMode.Instance.mode ^= modeToggle;
    }

    public void onInputModeChanged(int mode)
    {
        if ((mode & modeInteractable) != modeInteractable 
            && (mode & modeInteractable2) != modeInteractable2)
        {
            m_Toggle.interactable = false;
        }
        else if ((mode & modeNotInteractable) != 0)
        {
            m_Toggle.interactable = false;
        }
        else
        {
            m_Toggle.interactable = true;
            var colors = m_Toggle.colors;
            if ((mode & modeColorOn) == modeColorOn)
                colors.normalColor = activeColor;
            else
                colors.normalColor = Color.white;
            m_Toggle.colors = colors;
        }
    }
}
