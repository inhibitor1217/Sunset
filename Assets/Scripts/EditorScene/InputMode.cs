using System.Collections.Generic;
using UnityEngine;

public class InputMode
{
    
    private static InputMode _instance = null;
    public static InputMode instance
    {
        get
        {
            if (_instance == null)
                _instance = new InputMode();
            return _instance;
        }
    }

    public const int MOVE = 0x01;
    public const int BRUSH = 0x02;
    public const int SLIC = 0x04;
    public const int BUSY = 0x20;
    public const int ERASE = 0x40;
    public const int FLOW = 0x80;

    public static bool isMove(int mode) { return (mode & MOVE) != 0; }
    public bool isMove() { return isMove(_mode); }
    public static bool isBrush(int mode) { return (mode & BRUSH) != 0; }
    public bool isBrush() { return isBrush(_mode); }
    public static bool isSLIC(int mode) { return (mode & SLIC) != 0; }
    public bool isSLIC() { return isSLIC(_mode); }
    public static bool isBusy(int mode) { return (mode & BUSY) != 0; }
    public bool isBusy() { return isBusy(_mode); }
    public static bool isErase(int mode) { return (mode & ERASE) != 0; }
    public bool isErase() { return isErase(_mode); }
    public static bool isFlow(int mode) { return (mode & FLOW) != 0; }
    public bool isFlow() { return isFlow(_mode); } 
    public static bool isMode(int mode1, int mode2) { return (mode1 & mode2) != 0; }
    public bool isMode(int mode) { return isMode(_mode, mode); }

    private int _mode = 0;
    public int mode { 
        get { return _mode; } 
        set {
            if (_mode != value)
            {
                // Apply Side Effects
                if (isBrush(_mode) && !isBrush(value))
                {
                    EditorSceneMaster.instance.RemoveBrush();
                    if (EditorSceneMaster.instance.GetMaskTexture().dirty)
                    {
                        EditorSceneMaster.instance.InvokePCA(value);
                        EditorSceneMaster.instance.GetMaskTexture().textureShouldUpdate = true;
                        EditorSceneMaster.instance.GetMaskTexture().dirty = false;
                        return;
                    }
            }
                if (!isBrush(_mode) && isBrush(value))
                {
                    EditorSceneMaster.instance.CreateBrush();
                }

                if (!isFlow(_mode) && isFlow(value))
                {
                    EditorSceneMaster.instance.CreateFlow();
                }
                if (isFlow(_mode) && !isFlow(value))
                {
                    EditorSceneMaster.instance.RemoveFlow();
                }

                SetModeWithoutSideEffect(value);
            }
        }
    }
    public void SetModeWithoutSideEffect(int value)
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

    public static string ModeToString(int mode)
    {
        string ret = "";
        if (isMove(mode)) ret += "MOVE ";
        if (isBrush(mode)) ret += "BRUSH ";
        if (isSLIC(mode)) ret += "SLIC ";
        if (isBusy(mode)) ret += "BUSY ";
        if (isErase(mode)) ret += "ERASE ";
        if (isFlow(mode)) ret += "FLOW ";
        return ret;
    }

}
