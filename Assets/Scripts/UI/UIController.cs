using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField]
    GameObject creditsPanel;

    [SerializeField]
    GameObject mainMenuPanel;

    private List<GameObject> panels = new List<GameObject>();

    private void Start()
    {
        panels.Add(creditsPanel);
        panels.Add(mainMenuPanel);

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
