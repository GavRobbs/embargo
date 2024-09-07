using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This is the static class that handles message dispatch. Note that it isn't a GameObject, but it is a singleton. 
 * Messages are dispatched to all registered handlers, leaving it up to the handler if they can/want to handle it or not. 
 * Every manager that can handle messages should register itself with the MessageDispatcher in its start method.*/
public class MessageDispatcher
{
    private static MessageDispatcher instance;

    List<IMessageHandler> handlers = new List<IMessageHandler>();

    private MessageDispatcher()
    {

    }

    public static MessageDispatcher GetInstance()
    {
        if (MessageDispatcher.instance == null)
        {
            MessageDispatcher.instance = new MessageDispatcher();
        }
        return MessageDispatcher.instance;
    }

    public void Dispatch(GameMessage message)
    {
        foreach (IMessageHandler handler in handlers)
        {
            handler.HandleMessage(message);
        }
    }

    public void AddHandler(IMessageHandler handler)
    {
        handlers.Add(handler);
    }
    public void RemoveHandler(IMessageHandler handler)
    {
        handlers.Remove(handler);
    }
}
