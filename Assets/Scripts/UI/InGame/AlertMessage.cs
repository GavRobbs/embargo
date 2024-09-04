using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlertMessage : MonoBehaviour
{
    [SerializeField]
    Animator animator;

    [SerializeField]
    TextMeshProUGUI text;

    [SerializeField]
    AudioSource error_tone;

    public void DeactivateSelf()
    {
        gameObject.SetActive(false);
    }

    public void Display(string message)
    {
        text.text = message;
        animator.Play("alertfade");
        error_tone.Play();
    }
}
