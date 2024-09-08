using UnityEngine;

/* By making these MonoBehaviours, we can take advantage of the component pattern here. 
 * If I make this a child of the support turret, it is destroyed when the turret is destroyed, and all turrets that were 
 * benefitting get the update. Similarly, I keep a reference to this for each turret so it can dynamically 
 * add up the bonuses it is entitled to. We keep a reference to the producer ie. the support turret that produces the effect
 * so that we can increase it by level. */

public class AttackBoostBonus : MonoBehaviour
{
    //This is the turret that produces the attack boost
    ITurret _producer;

    public ITurret Producer
    {
        get
        {
            return _producer;
        }

        set
        {
            _producer = value;
        }
    }

    OffensiveTurret _receiver;

    public OffensiveTurret Beneficiary {

        get
        {
            return _receiver;
        }

        set
        {
            _receiver = value;

            if (_receiver != null)
            {
                _receiver.AddAttackBonus(this);
            }

        }
    }

    public float RangeBonus
    {
        get
        {
            return _producer.Level * 0.015f;
        }
    }

    public float AttackBonus
    {
        get
        {
            return _producer.Level * 0.02f;
        }
    }

    public float CooldownBonus
    {
        get
        {
            return _producer.Level * 0.01f;
        }
    }

    void OnDestroy()
    {
        Beneficiary.RemoveAttackBonus(this);        
    }

    void Update()
    {
        if(Producer == null)
        {
            Beneficiary.RemoveAttackBonus(this);
        }
    }
}
