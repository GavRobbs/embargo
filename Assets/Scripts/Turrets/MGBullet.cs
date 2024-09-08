using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGBullet : MonoBehaviour, IBullet
{
    private Vector3 forward_vector;

    [SerializeField]
    float _damage;

    public float Damage { get => _damage; set => _damage = value; }
    public bool ArmourBonus => false;

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

}
