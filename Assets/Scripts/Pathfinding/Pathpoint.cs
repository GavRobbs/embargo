using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathpoint : MonoBehaviour
{
    [SerializeField]
    bool passable;

    BoxCollider zone;

    [SerializeField]
    Vector2Int indexed_position;

    public bool IsPassable
    {
        get
        {
            return passable;
        }

        set
        {
            passable = value;
        }
    }

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public Vector2Int IndexedPosition
    {
        get
        {
            return indexed_position;
        }

        set
        {
            indexed_position = value;
        }
    }

    public int GetPositionAsLinearArrayIndex(int array_n_elements_per_row)
    {
        return indexed_position.y * array_n_elements_per_row + indexed_position.x;
    }

    void Start()
    {
        zone = GetComponentInChildren<BoxCollider>();
    }

    public bool IsPointOnTile(Vector3 point)
    {
        return zone.bounds.Contains(point);
    }

}
