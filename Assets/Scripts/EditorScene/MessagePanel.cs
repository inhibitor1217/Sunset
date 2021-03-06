using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MonoBehaviour
{

    public static MessagePanel instance { get; private set; }

    private Image m_Image;
    public Text mainText;
    public Text subText;

    void Awake()
    {
        instance = this;

        m_Image = GetComponent<Image>();

        Disable();
    }

    public void ShowMessage(string mainMsg, string subMsg)
    {
        m_Image.enabled = true;
        mainText.enabled = true;
        subText.enabled = true;
        
        if (mainMsg != null)
            mainText.text = mainMsg;
        if (subMsg != null)
            subText.text = subMsg;
    }

    public void Disable()
    {
        m_Image.enabled = false;
        mainText.enabled = false;
        subText.enabled = false;
    }

}