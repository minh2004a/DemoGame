// ArrowProjectile.cs
using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    Rigidbody2D rb;
    int damage;
    Transform owner;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    public void Fire(Vector2 dir, float speed, int dmg, Transform shooter)
    {
        owner = shooter;
        damage = dmg;
        rb.velocity = dir.normalized * speed;
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other || other.transform.root == owner) return; // không tự bắn trúng mình
        var d = other.GetComponentInParent<IDamageable>();
        if (d != null) { d.TakeHit(damage); Destroy(gameObject); }
        else if (other.gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
            Destroy(gameObject); // chạm tường/vật khác thì huỷ
        }
    }
}
