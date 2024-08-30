using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SupportTurret : MonoBehaviour, ITurret
{
    virtual public string TurretClass => throw new System.NotImplementedException();

    virtual public int Level => throw new System.NotImplementedException();

    virtual public float Influence => throw new System.NotImplementedException();

    [SerializeField]
    protected Transform influence_center;

    public PopupContent GetHoverData()
    {
        throw new System.NotImplementedException();
    }

    public void OnHoverOff()
    {
        throw new System.NotImplementedException();
    }

    public void OnHoverOver()
    {
        throw new System.NotImplementedException();
    }

    virtual public void OnTurretDestroy()
    {
    }

    virtual public void OnTurretSpawn()
    {
    }

}
