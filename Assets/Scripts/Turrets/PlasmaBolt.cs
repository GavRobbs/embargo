using UnityEngine;

public class PlasmaBolt : MonoBehaviour, IBullet
{
    private Vector3 forward_vector;

    [SerializeField]
    float _damage;

    public float Damage { get => _damage; set => _damage = value; }

    public bool ArmourBonus => true;

    [SerializeField]
    Rigidbody mRigidbody;

    [SerializeField]
    float speed;
    private void Start()
    {
        forward_vector = transform.forward;
        mRigidbody.AddForce(forward_vector * speed, ForceMode.Impulse);
        Destroy(gameObject, 3);
    }

}
