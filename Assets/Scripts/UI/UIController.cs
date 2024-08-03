using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    AudioClip moveSound;

    [SerializeField]
    AudioClip clickSound;

    [SerializeField]
    AudioClip playSound;

    private AudioSource audioSrc;

    private List<GameObject> panels;

    private bool selectingButton = false;

    private void Start()
    {
        audioSrc = GetComponent<AudioSource>();

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
        GameObject.Find("SceneManager").GetComponent<PlayerInput>().SwitchCurrentActionMap("UI");
        foreach (var child in panel.GetComponentsInChildren<Button>(false))
        {
            if (child.CompareTag("DefaultButton"))
            {
                selectingButton = true;
                try
                {
                    child.Select();
                }
                finally
                {
                    selectingButton = false;
                }
            }
        }
    }

    public void OnPlay()
    {
        DisableAllPanels();
        audioSrc.PlayOneShot(playSound);
        GameObject.Find("SceneManager").GetComponent<PlayerInput>().SwitchCurrentActionMap("InGame");
        Debug.Log("start game");
    }

    public void OnClick()
    {
        audioSrc.PlayOneShot(clickSound);
    }

    public void OnSelect()
    {
        if (!selectingButton)
        {
            audioSrc.PlayOneShot(moveSound);
        }
    }
}
