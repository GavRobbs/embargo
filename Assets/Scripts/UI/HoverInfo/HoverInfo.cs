using System.Collections;
using System.Collections.Generic;

public class HoverInfo
{
    public GameInputManager.HOVER_MODE mode;
    public int other_info;

    public HoverInfo(GameInputManager.HOVER_MODE h, int info)
    {
        mode = h;
        other_info = info;
    }
}
