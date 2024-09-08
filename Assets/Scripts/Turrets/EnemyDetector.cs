using System.Collections.Generic;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    List<ITargetable> enemies;

    private void Start()
    {
        enemies = new List<ITargetable>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            enemies.Add(other.gameObject.GetComponentInParent<ITargetable>());
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            enemies.Remove(other.gameObject.GetComponentInParent<ITargetable>());
        }
    }

    public ITargetable PickEnemy()
    {
        if (enemies.Count == 0)
        {
            return null;
        }

        return enemies[Random.Range(0, enemies.Count)];
    }

    public void SetDetectionRadius(float radius)
    {
        var sc = GetComponent<SphereCollider>();
        if(sc != null)
        {
            sc.radius = radius;
        }
    }
}
