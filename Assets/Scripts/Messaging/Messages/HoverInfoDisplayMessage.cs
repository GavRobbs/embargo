using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverInfoDisplayMessage : GameMessage
{
    public Dictionary<string, string> info;
    public Vector2 display_position;
    public HoverInfoDisplayMessage(Dictionary<string, string> info_dict, Vector2 pos) : base(MessageConstants.HoverInfoDisplayMessage)
    {
        display_position = pos;
        info = info_dict;
    }
}
