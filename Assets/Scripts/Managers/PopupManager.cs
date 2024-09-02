using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/* This controls the different types of popup that can appear. with utility functions to populate the data*/

public class PopupManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> popupOptions;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HideAllPopups()
    {
        foreach (var p in popupOptions)
        {
            p.gameObject.SetActive(false);
        }
    }

    public void ShowBuildingPopup(int hp, Vector2 pos)
    {
        HideAllPopups();
        popupOptions[0].transform.Find("BuildingHP").GetComponent<TextMeshProUGUI>().text = $"HP: {hp}/30";
        popupOptions[0].transform.position = pos;
        popupOptions[0].gameObject.SetActive(true);
    }

    public void ShowOffensiveTurretBuildingPopup(string turret_name, int turret_lv, int bhp, float atk_bonus, float range_bonus, float cd_bonus, Vector2 pos)
    {
        HideAllPopups();
        popupOptions[1].transform.Find("Turretname").GetComponent<TextMeshProUGUI>().text = turret_name;
        popupOptions[1].transform.Find("TurretLv").GetComponent<TextMeshProUGUI>().text = $"Lv. {turret_lv}";
        popupOptions[1].transform.Find("BuildingHP").GetComponent<TextMeshProUGUI>().text = $"HP: {bhp}/30";
        popupOptions[1].transform.Find("BonusList").GetComponent<TextMeshProUGUI>().text = $"+{System.Math.Round(atk_bonus * 100.0, 1)}% attack damage\n+{System.Math.Round(range_bonus * 100.0, 1)}% attack range\n-{System.Math.Round(cd_bonus * 100.0, 1)}% cooldown\n";
        popupOptions[1].transform.position = pos;
        popupOptions[1].gameObject.SetActive(true);
    }

    public void ShowEnemyInfoPopup(string enemy_name, int current_hp, int max_hp, Vector2 pos)
    {
        HideAllPopups();
        popupOptions[2].transform.Find("EnemyName").GetComponent<TextMeshProUGUI>().text = enemy_name;
        popupOptions[2].transform.Find("EnemyHP").GetComponent<TextMeshProUGUI>().text = $"HP: {current_hp}/{max_hp}";
        popupOptions[2].transform.position = pos;
        popupOptions[2].gameObject.SetActive(true);
    }

    public void ShowDefensiveTurretBuildingPopup(string turret_name, int turret_lv, int bhp, Vector2 pos)
    {
        HideAllPopups();
        popupOptions[3].transform.Find("Turretname").GetComponent<TextMeshProUGUI>().text = turret_name;
        popupOptions[3].transform.Find("TurretLv").GetComponent<TextMeshProUGUI>().text = $"Lv. {turret_lv}";
        popupOptions[3].transform.Find("BuildingHP").GetComponent<TextMeshProUGUI>().text = $"HP: {bhp}/30";
        popupOptions[3].transform.position = pos;
        popupOptions[3].gameObject.SetActive(true);
    }

    public void ShowDronePopup(Vector2 pos)
    {
        HideAllPopups();
        popupOptions[4].transform.position = pos;
        popupOptions[4].gameObject.SetActive(true);
    }
}
