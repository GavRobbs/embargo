using System.Collections.Generic;
using UnityEngine;

public class TargetDetector : MonoBehaviour {
    private List<Building> buildings;

    private void Start() {
        buildings = new List<Building>();
    }

    private void OnTriggerEnter(Collider other) {
        var building = other.GetComponentInParent<Building>();
        if (building) {
            buildings.Add(building);
        }
    }

    private void OnTriggerExit(Collider other) {
        var building = other.GetComponentInParent<Building>();
        if (building) {
            buildings.Remove(building);
        }
    }

    public Building GetRandomTarget() {
        return buildings.Count != 0 ? buildings[Random.Range(0, buildings.Count)] : null;
    }
}