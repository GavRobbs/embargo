using System.Collections.Generic;

public interface IHoverable {
    Dictionary<string, string> GetHoverData();

    void OnHoverOver(HoverInfo info);

    void OnHoverOff();
}