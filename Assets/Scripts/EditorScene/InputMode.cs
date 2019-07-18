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

    public const int MOVE = 0;
    public const int BRUSH = 1;
    public const int BRUSH_SLIC = 2;

    public int mode = MOVE;

}
