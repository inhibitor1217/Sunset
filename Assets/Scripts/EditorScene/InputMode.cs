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

    public const int MOVE = 0x01;
    public const int BRUSH = 0x02;
    public const int SLIC = 0x04;
    public const int WATER = 0x08;
    public const int SKY = 0x10;
    public const int BUSY = 0x20;
    public const int ERASE = 0x40;

    public static bool isMove(int mode) { return (mode & MOVE) != 0; }
    public bool isMove() { return isMove(_mode); }
    public static bool isBrush(int mode) { return (mode & BRUSH) != 0; }
    public bool isBrush() { return isBrush(_mode); }
    public static bool isSLIC(int mode) { return (mode & SLIC) != 0; }
    public bool isSLIC() { return isSLIC(_mode); }
    public static bool isWater(int mode) { return (mode & WATER) != 0; }
    public bool isWater() { return isWater(_mode); }
    public static bool isSky(int mode) { return (mode & SKY) != 0; }
    public bool isSky() { return isSky(_mode); }
    public static bool isBusy(int mode) { return (mode & BUSY) != 0; }
    public bool isBusy() { return isBusy(_mode); }
    public static bool isErase(int mode) { return (mode & ERASE) != 0; }
    public bool isErase() { return isErase(_mode); }

    private int _mode = 0;
    public int mode { 
        get { return _mode; } 
        set {
            if (_mode != value)
            {
                // Apply Side Effects
                if (isBrush(value))
                {
                    if (isWater(value))
                        EditorSceneMaster.Instance.CreateBrush(EditorSceneMaster.EFFECT_WATER);
                    if (isSky(value))
                        EditorSceneMaster.Instance.CreateBrush(EditorSceneMaster.EFFECT_SKY);
                }
                else
                {
                    EditorSceneMaster.Instance.RemoveBrush(EditorSceneMaster.EFFECT_WATER);
                    EditorSceneMaster.Instance.RemoveBrush(EditorSceneMaster.EFFECT_SKY);
                }

                if (isSLIC(value))
                {
                    EditorSceneMaster.Instance.CreateSLIC(value);
                    return;
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
        if (isWater(mode)) ret += "WATER ";
        if (isSky(mode)) ret += "SKY ";
        if (isBusy(mode)) ret += "BUSY ";
        if (isErase(mode)) ret += "ERASE ";
        return ret;
    }

}
