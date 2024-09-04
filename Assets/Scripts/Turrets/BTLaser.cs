using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTLaser : MonoBehaviour
{
    private Vector3 forward_vector;

    [SerializeField]
    float _damage;
    public float Damage { get => _damage; set => _damage = value; }

    [SerializeField]
    Rigidbody mRigidbody;

    [SerializeField]
    float speed;
    void Start()
    {
        forward_vector = transform.forward;
        mRigidbody.AddForce(forward_vector * speed, ForceMode.Impulse);
        GameObject.Destroy(this.gameObject, 3);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Building"))
        {
            Building b = collision.gameObject.GetComponentInParent<Building>();
            b.Damage(Damage);
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Turret"))
        {
            ITurret t = collision.gameObject.GetComponentInParent<ITurret>();
            t.AttachedBuilding.Damage(Damage);

        }
        if (this.gameObject)
        {
            GameObject.Destroy(this.gameObject);
        }
    }

}
