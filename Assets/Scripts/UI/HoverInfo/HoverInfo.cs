public class HoverInfo
{
    public GameInputManager.HoverMode mode;
    public int other_info;

    public HoverInfo(GameInputManager.HoverMode h, int info)
    {
        mode = h;
        other_info = info;
    }
}
