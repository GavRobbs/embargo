using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private List<GameObject> panels;

    private void Start()
    {
        panels = new List<GameObject>(GameObject.FindGameObjectsWithTag("Panel"));
        var defaultPanel = GameObject.FindGameObjectWithTag("DefaultPanel");
        panels.Add(defaultPanel);
        SwitchToPanel(defaultPanel);
    }

    private void DisableAllPanels()
    {
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
    }

    public void SwitchToPanel(GameObject panel)
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
}
