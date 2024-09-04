using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDetector : MonoBehaviour
{
    List<Building> buildings;

    void Start()
    {
        buildings = new List<Building>();
    }

    void OnTriggerEnter(Collider other)
    {
        var building = other.GetComponentInParent<Building>();
        if(building != null)
        {
            buildings.Add(building);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var building = other.GetComponentInParent<Building>();
        if (building != null)
        {
            buildings.Remove(building); 
        }

    }

    public Building GetRandomTarget()
    {
        if(buildings.Count == 0)
        {
            return null;
        }

        return buildings[Random.Range(0, buildings.Count)];
    }
}
