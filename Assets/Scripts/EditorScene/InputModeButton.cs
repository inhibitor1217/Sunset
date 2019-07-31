using UnityEngine;
using UnityEngine.UI;

public class InputModeButton : MonoBehaviour
{

    public int modeInteractable;
    public int modeNotInteractable;

    private Button m_Button;

    void Awake()
    {
        m_Button = GetComponent<Button>();
        InputMode.Subscribe(this);
    }

    void Start()
    {
        InputMode.Subscribe(this);
    }

    void OnDestroy()
    {
        InputMode.Unsubscribe(this);
    }

    public void OnInputModeChanged(int mode)
    {
        if ((mode & modeInteractable) != modeInteractable)
        {
            m_Button.interactable = false;
        }
        else if ((mode & modeNotInteractable) != 0)
        {
            m_Button.interactable = false;
        }
        else
        {
            m_Button.interactable = true;
        }
    }
}
