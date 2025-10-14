using UnityEngine;

public class DummyEnemy : MonoBehaviour, IDamageable
{
    [SerializeField] int hp = 3;
    public void TakeHit(int dmg)
    {
        hp -= dmg;
        if (hp <= 0) Destroy(gameObject);
    }
}
