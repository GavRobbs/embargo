/* This enum has all the possible messages that can be sent by the game event system, 
 * helps with decoupling, and the SceneMessage class carries the extra data needed by the message */

public enum MessageConstants : int
{
    NullMessage,
    HoverInfoDisplayMessage,
    HideHoverPopupMessage
}