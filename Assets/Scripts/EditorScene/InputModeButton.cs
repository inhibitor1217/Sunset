using UnityEngine;

public class InputModeButton : MonoBehaviour
{
    public int mode;

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

    public void OnInputModeChanged(int mode)
    {

    }
}
