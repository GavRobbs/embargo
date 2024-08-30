using System.Collections.Generic;
public interface IHoverable
{
    Dictionary<string, string> GetHoverData();

    void OnHoverOver();

    void OnHoverOff();
}