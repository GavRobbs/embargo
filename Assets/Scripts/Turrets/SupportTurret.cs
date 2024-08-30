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

    public Dictionary<string, string> GetHoverData()
    {
        return new Dictionary<string, string>()
        {
            {"type" , "support_turret"},
            {"name", TurretClass },
            {"level", Level.ToString() }
        };
    }

    public virtual void OnHoverOff()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnHoverOver()
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
