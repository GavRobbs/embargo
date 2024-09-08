using System.Collections.Generic;
using UnityEngine;

public abstract class SupportTurret : MonoBehaviour, ITurret {
    [SerializeField] Building _building;

    [SerializeField] int _cost;

    virtual public string TurretClass => throw new System.NotImplementedException();

    public int Level { get; set; }

    public int Cost => _cost;

    virtual public float Influence => throw new System.NotImplementedException();

    public Building AttachedBuilding {
        get => _building;
        set => _building = value;
    }

    virtual public float BuildTime { get; }

    [SerializeField] bool _online;

    public bool Online {
        get => _online;

        set { _online = value; }
    }


    [SerializeField] protected Transform influence_center;

    public Dictionary<string, string> GetHoverData() {
        if (!Online) {
            return null;
        }

        return new Dictionary<string, string>() {
            { "type", "support_turret" },
            { "name", TurretClass },
            { "level", Level.ToString() },
            { "bhp", _building.hp.ToString() },
        };
    }

    public void OnHoverOver(HoverInfo info) {
        if (info == null) {
            return;
        }

        if (info.mode == GameInputManager.HoverMode.UPGRADE && Online) {
            AttachedBuilding?.ActivateArrow();
            return;
        }

        if (info.mode == GameInputManager.HoverMode.SCRAP && Online) {
            AttachedBuilding?.ActivateScrapIcon();
        }
    }

    public void OnHoverOff() {
        if (AttachedBuilding != null) {
            AttachedBuilding.DeactivateArrow();
            AttachedBuilding.DeactivateScrapIcon();
        }
    }

    virtual public void OnTurretDestroy() {
    }

    virtual public void OnTurretSpawn() {
        Online = true;
    }

    virtual public void OnTurretUpgrade() {
        throw new System.NotImplementedException();
    }

    virtual public void BestowBonus(ITurret turret) {
    }

    public virtual void Start() {
        Level = 1;
    }

    public void Stop() {
    }
}