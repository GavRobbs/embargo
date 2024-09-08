public interface ISelectable {
    void Select();

    void Deselect();
    //Objects that create a context menu will make it pop up from the bottom of the screen, while objects that don't just exist to be acted upon
}