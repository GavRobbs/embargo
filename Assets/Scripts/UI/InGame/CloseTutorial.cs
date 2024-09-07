using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseTutorial : MonoBehaviour
{
    public void Close()
    {
        gameObject.SetActive(false);
        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.StartGameMessage));
    }
}
