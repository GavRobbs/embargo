using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scout : MonoBehaviour, ITargetable
{
    [SerializeField]
    GameObject forwardPoint;

    [SerializeField]
    GameObject scoutBody;
    public Vector3 Position => forwardPoint.transform.position;

    public bool IsFriendly => false;

    public bool IsKilled => HitPoints <= 0.0f;

    [SerializeField]
    float hp;

    public float HitPoints => hp;

    bool dying = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(hp <= 0.0f && !dying)
        {
            KillMe();
        }
        
    }

    public void Damage(float value)
    {
        hp -= value;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            Destroy(collision.gameObject);
            //TODO: Patch so that the damage is done appropriately
            Damage(5.0f);
        }

    }

    void KillMe()
    {
        dying = true;
        scoutBody.SetActive(false);
        //sparks.Play();
        //flash.Play();
        GameObject.Destroy(this.gameObject, 2.5f);
        //explosion.Play();

    }
}
