using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private List<GameObject> panels;

    private void Start()
    {
        GameObject defaultPanel = null;
        panels = new List<GameObject>();
        var canvasObj = GameObject.FindObjectOfType<Canvas>();
        foreach (Transform child in canvasObj.transform)
        {
            if (child.gameObject.CompareTag("Panel"))
            {
                panels.Add(child.gameObject);
            }
            else if (child.gameObject.CompareTag("DefaultPanel"))
            {
                panels.Add(child.gameObject);
                defaultPanel = child.gameObject;
            }
        }
        if (defaultPanel)
        {
            SwitchToPanel(defaultPanel);
        }
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
