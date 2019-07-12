using UnityEngine;

public class InputModeButton : MonoBehaviour
{
    public int mode;

    public void SetMode()
    {
        InputMode.Instance.mode = mode;
    }
}
