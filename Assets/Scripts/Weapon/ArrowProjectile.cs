using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class ArrowProjectile : MonoBehaviour {
    int dmg; Vector2 dir; float speed; LayerMask mask;

    void Awake(){
        var rb = GetComponent<Rigidbody2D>(); rb.isKinematic = true; rb.gravityScale = 0;
        var col = GetComponent<CapsuleCollider2D>();
        col.isTrigger = true;
        col.direction = CapsuleDirection2D.Horizontal; // cho mũi tên nằm ngang
    }

    public void Init(int damage, Vector2 direction, float spd, LayerMask enemyMask, float life=2f){
        dmg = damage; dir = direction.normalized; speed = spd; mask = enemyMask;
        Destroy(gameObject, life);
    }

    void Update(){ transform.position += (Vector3)(dir * speed * Time.deltaTime); }

    void OnTriggerEnter2D(Collider2D other){
        if (((1<<other.gameObject.layer) & mask) == 0) return;
        other.GetComponentInParent<IDamageable>()?.TakeHit(dmg);
        Destroy(gameObject);
    }
}
