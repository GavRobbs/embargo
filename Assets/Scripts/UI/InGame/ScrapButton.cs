using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapButton : MonoBehaviour
{
    public void OnClick()
    {
        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.EngageScrapMode));
    }
}
