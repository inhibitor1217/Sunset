using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MonoBehaviour
{

    private static MessagePanel m_Instance;
    public static MessagePanel Instance { get { return m_Instance; } }

    private Image m_Image;
    private Text m_Text;

    void Awake()
    {
        m_Instance = this;

        m_Image = GetComponent<Image>();
        m_Text = GetComponentInChildren<Text>();

        Disable();
    }

    public void ShowMessage(string msg)
    {
        m_Image.enabled = true;
        m_Text.enabled = true;
        m_Text.text = msg;
    }

    public void Disable()
    {
        m_Image.enabled = false;
        m_Text.enabled = false;
    }

}