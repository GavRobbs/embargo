using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleValueMessage<T> : GameMessage
{
    public T value;
    public SingleValueMessage(MessageConstants m, T val) : base(m)
    {
        value = val;
    }
}
