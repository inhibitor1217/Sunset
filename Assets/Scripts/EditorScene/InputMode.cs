using System.Collections.Generic;
using UnityEngine;

public class InputMode
{
    
    private static InputMode instance = null;
    public static InputMode Instance
    {
        get
        {
            if (instance == null)
                instance = new InputMode();
            return instance;
        }
    }

    public enum Mode
    {
        EMPTY = 0,
        MOVE = 10,
        BRUSH_PLAIN = 20,
        BRUSH_SLIC = 21,
        BUSY = 30
    };

    private Mode _mode = Mode.EMPTY;
    public Mode mode { 
        get { return _mode; } 
        set {
            if (_mode != value)
            {
                // Apply Side Effects
                if (value == Mode.BRUSH_PLAIN)
                    EditorSceneMaster.Instance.CreateBrush(0);
                else
                    EditorSceneMaster.Instance.RemoveBrush(0);

                if (IsSLICRequired(value))
                {
                    EditorSceneMaster.Instance.CreateSLIC(value);
                    return;
                }

                SetModeWithoutSideEffect(value);
            }
        }
    }
    public void SetModeWithoutSideEffect(Mode value)
    {
        if (_mode != value)
        {
            // Update UI Components
            foreach (var button in _UIButtons)
                button.OnInputModeChanged(value);
            foreach (var toggle in _UIToggles)
                toggle.onInputModeChanged(value);

            _mode = value;

#if UNITY_EDITOR
            Debug.Log("InputMode: Set mode to " + ModeToString(_mode));
#endif
        }
    }

    private static List<InputModeButton> _UIButtons = new List<InputModeButton>();
    private static List<InputModeToggle> _UIToggles = new List<InputModeToggle>();

    public bool IsModeBrush()
    {
        return IsModeBrush(_mode);
    }

    public static bool IsModeBrush(Mode mode)
    {
        return (mode == Mode.BRUSH_PLAIN) || (mode == Mode.BRUSH_SLIC);
    }

    public bool IsSLICRequired()
    {
        return IsSLICRequired(_mode);
    }

    public static bool IsSLICRequired(Mode mode)
    {
        return (mode == Mode.BRUSH_SLIC);
    }

    public static void Subscribe(InputModeButton button)
    {
        _UIButtons.Add(button);
    }

    public static void Unsubscribe(InputModeButton button)
    {
        _UIButtons.Remove(button);
    }

    public static void Subscribe(InputModeToggle toggle)
    {
        _UIToggles.Add(toggle);
    }

    public static void Unsubscribe(InputModeToggle toggle)
    {
        _UIToggles.Remove(toggle);
    }

    public static string ModeToString(Mode mode)
    {
        switch(mode)
        {
        case Mode.MOVE:
            return "MOVE";
        case Mode.BRUSH_PLAIN:
            return "BRUSH_PLAIN";
        case Mode.BRUSH_SLIC:
            return "BRUSH_SLIC";
        case Mode.BUSY:
            return "BUSY";
        default:
            return "UNKNOWN";
        }
    }

}
