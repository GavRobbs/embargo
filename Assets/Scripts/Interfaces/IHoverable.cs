using System.Collections.Generic;
using UnityEngine;

public interface IHoverable
{
    Dictionary<string, string> GetHoverData();

    void OnHoverOver(HoverInfo info);

    void OnHoverOff();
}