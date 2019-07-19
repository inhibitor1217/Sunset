using UnityEngine;

public class InputModeButton : MonoBehaviour
{
    public InputMode.Mode mode;

    void Start()
    {
        InputMode.Subscribe(this);
    }

    void OnDestroy()
    {
        InputMode.Unsubscribe(this);
    }

    public void SetMode()
    {
        InputMode.Instance.mode = mode;
    }

    public void OnInputModeChanged(InputMode.Mode mode)
    {

    }
}
