using UnityEngine;

public class SliderPanelController : MonoBehaviour
{
    
    public void Setup()
    {
        Store.instance.Subscribe(
            new string[] { SharedActions.FIELD__EDIT_PARAMETER },
            (state) => {
                string edit = (string)state[SharedActions.FIELD__EDIT_PARAMETER];
                gameObject.SetActive(edit != "NONE");
                InputManager.instance.optionMenu = edit != "NONE" ? gameObject.GetComponent<RectTransform>() : null;
            }
        );
    }

}
