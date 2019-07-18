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
    }

    public void UpdateColor(bool isOn)
    {
        var colors = m_Toggle.colors;
        colors.normalColor = isOn ? Color.yellow : Color.white;
        m_Toggle.colors = colors;
    }

    public void BrushToggle(bool isOn)
    {
        if (isOn)
            EditorSceneMaster.Instance.CreateBrush(0);
        else
            EditorSceneMaster.Instance.RemoveBrush();
    }
}
