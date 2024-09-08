using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    public void OnClick()
    {
        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.EngageUpgradeMode));
    }
}
