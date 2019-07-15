using UnityEngine;
using UnityEngine.UI;

public class InputModeButton : MonoBehaviour
{
    public int mode;
    
    private Button m_Button;

    void Awake()
    {
        m_Button = GetComponent<Button>();
    }

    public void SetMode()
    {
        InputMode.Instance.mode = mode;
    }
}
