using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    private GameObject activePanel;
    private GameObject defaultPanel;

    private bool selectingButton = false;

    private void Start()
    {
        audioSrc = GetComponent<AudioSource>();

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
        activePanel = null;
    }

    public void SwitchToPanel(GameObject panel)
    {
        DisableAllPanels();

        panel.SetActive(true);
        activePanel = panel;
        
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
        foreach (var child in activePanel.GetComponentsInChildren<Button>(false))
        {
            child.interactable = false;
        }
        StartCoroutine("StartGame");
    }

    IEnumerator StartGame()
    {
        // Play suspenseful sound and wait for it to complete
        audioSrc.PlayOneShot(playSound);
        yield return new WaitForSecondsRealtime(playSound.length);

        // Load and switch to the main game scene
        yield return SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
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
