using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField]
    GameObject stageOne;

    [SerializeField]
    GameObject stageTwo;

    private void Awake()
    {
        stageOne.SetActive(true);
        stageTwo.SetActive(false);
    }

    public void Next()
    {
        stageOne.SetActive(false);
        stageTwo.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.StartGameMessage));
    }
}
