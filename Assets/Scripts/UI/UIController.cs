using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    GameObject creditsPanel;

    [SerializeField]
    GameObject mainMenuPanel;

    [SerializeField]
    GameObject victoryPanel;

    [SerializeField]
    GameObject gameOverPanel;

    private List<GameObject> panels = new List<GameObject>();

    private void Start()
    {
        panels.Add(creditsPanel);
        panels.Add(mainMenuPanel);
        panels.Add(victoryPanel);
        panels.Add(gameOverPanel);

        SwitchToPanel(mainMenuPanel);
    }

    private void DisableAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
    }

    private void SwitchToPanel(GameObject panel)
    {
        DisableAllPanels();
        panel.SetActive(true);
        foreach (var child in panel.GetComponentsInChildren<Button>(false))
        {
            if (child.CompareTag("DefaultButton"))
            {
                child.Select();
            }
        }
    }

    public void OnPlay()
    {
        Debug.Log("start game");
        DisableAllPanels();
    }

    public void OnCredits()
    {
        SwitchToPanel(creditsPanel);
    }
    public void OnMainMenu()
    {
        SwitchToPanel(mainMenuPanel);
    }
}
