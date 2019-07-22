using UnityEngine;
using UnityEngine.UI;

public class InputModeToggle : MonoBehaviour
{
    private Toggle m_Toggle;

    public int modeToggle;
    public int modeInteractable;
    public int modeColorOn;

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
        if ((mode & modeInteractable) == 0)
        {
            m_Toggle.interactable = false;
        }
        else
        {
            m_Toggle.interactable = true;
            var colors = m_Toggle.colors;
            if ((mode & modeColorOn) == 0)
                colors.normalColor = Color.white;
            else
                colors.normalColor = Color.yellow;
            m_Toggle.colors = colors;
        }
    }
}
