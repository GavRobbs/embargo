﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField]
    TextAsset creditsData;

    [SerializeField]
    GameObject creditPrefab;

    private List<CreditData> credits;
    private List<GameObject> creditObjects;

    private int nextIndex;

    void Awake()
    {
        List<string> data = new List<string>(creditsData.text.Replace('\r', ' ').Split('\n'));
        credits = data.Select(
            credit => (CreditData)Activator.CreateInstance(typeof(CreditData), credit.Split(':').Select(cred => cred.Trim()).ToArray())
        ).ToList();
        creditObjects = new List<GameObject>();
        nextIndex = 0;
    }

    private void OnEnable()
    {        
        StartCoroutine("HandleCredits");
    }

    private void OnDisable()
    {
        StopCoroutine("HandleCredits");

        // Destroy all credits
        foreach (var credit in creditObjects)
        {
            Destroy(credit);
        }
        creditObjects.Clear();
        nextIndex = 0;
    }

    IEnumerator HandleCredits()
    {
        // Clear refs to any destroyed credits
        for (var i = creditObjects.Count - 1; i >= 0; --i)
        {
            if (!creditObjects[i])
            {
                creditObjects.RemoveAt(i);
            }
        }

        if (isActiveAndEnabled)
        {
            GameObject newCredit = Instantiate(creditPrefab, transform);
            newCredit.GetComponent<TextMeshProUGUI>().text = credits[nextIndex++].formattedString;
            creditObjects.Add(newCredit);
            if (nextIndex == credits.Count)
            {
                nextIndex = 0;
                yield return new WaitForSecondsRealtime(2);
            }
            yield return new WaitForSecondsRealtime(1);
            StartCoroutine("HandleCredits");
        }
    }
}
