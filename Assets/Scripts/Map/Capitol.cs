using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Capitol : MonoBehaviour
{
    int hp = 6;
    bool dead = false;
    // Start is called before the first frame update

    [SerializeField]
    GameObject explosion;

    [SerializeField]
    GameObject capitolMesh;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DecreaseHP()
    {
        hp -= 1;
        Image[] images = GetComponentsInChildren<Image>();

        if (images != null)
        {
            Destroy(images[images.Length - 1]);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (dead)
        {
            return;
        }

        if(other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            IEnemy enemy = other.GetComponentInParent<IEnemy>();

            for(int i = 0; i < enemy.CapitolDamage; i++)
            {
                DecreaseHP();
            }

            enemy.KillMe();

            if(hp <= 0)
            {
                explosion.SetActive(true);
                capitolMesh.SetActive(false);
                MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.GameOverMessage));
                dead = true;
            }

        }
    }

}
