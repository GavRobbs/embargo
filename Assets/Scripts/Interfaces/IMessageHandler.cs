/* This allows us to implement the observer pattern and decouple our GameObjects somewhat.
 * All/most of the manager objects should implement this interface so they can handle messages from the MessageDispatcher.
 * Manager objects should add themselves as a listener to the MessageDispatcher in their start method. */

public interface IMessageHandler {
    void HandleMessage(GameMessage message);
}