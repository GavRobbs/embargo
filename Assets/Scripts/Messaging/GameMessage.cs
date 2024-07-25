using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMessage
{
    MessageConstants message_type;

    public MessageConstants MessageType
    {
        get
        {
            return message_type;
        }
    }

    public GameMessage(MessageConstants m_t)
    {
        message_type = m_t;
    }
}
