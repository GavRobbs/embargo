using UnityEngine;

public class ScrapButton : MonoBehaviour
{
    public void OnClick()
    {
        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.EngageScrapMode));
    }
}
