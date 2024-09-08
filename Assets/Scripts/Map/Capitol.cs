using UnityEngine;
using UnityEngine.UI;

public class Capitol : MonoBehaviour {
    private int hp = 6;

    private bool dead;
    // Start is called before the first frame update

    public int Health => hp;

    [SerializeField] private GameObject explosion;

    [SerializeField] private GameObject capitolMesh;

    private void DecreaseHp() {
        hp -= 1;
        if (hp < 0) return;

        Image[] images = GetComponentsInChildren<Image>();

        //Since I'm using the horizontal layout group
        //I use a kind of clever trick to make the bars invisible so they still maintain alignment

        var c = images[hp].color;
        c.a = 0.0f;
        images[hp].color = c;
    }

    private void OnTriggerEnter(Collider other) {
        if (dead) return;

        if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

        IEnemy enemy = other.GetComponentInParent<IEnemy>();

        for (var i = 0; i < enemy.CapitolDamage; i++) {
            DecreaseHp();
        }

        enemy.Destroy();

        if (hp > 0) return;

        explosion.SetActive(true);
        capitolMesh.SetActive(false);
        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.GameOverMessage));
        dead = true;
    }
}