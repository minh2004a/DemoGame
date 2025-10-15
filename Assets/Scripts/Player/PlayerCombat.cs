// PlayerCombat.cs (tối giản cho kiếm)
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] PlayerInventory inv;
    [SerializeField] PlayerController controller;
    [SerializeField] Animator anim;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] LayerMask enemyMask;
    [SerializeField] bool bowAimWithMouse = true;
    [SerializeField] float defaultHitRadius = 0.35f; // giữ bán kính mặc định
    [SerializeField] float minCooldown = 0.15f;

    Rigidbody2D rb; Vector2 move, lastFacing = Vector2.down; float cd;

    void Awake() { rb = GetComponent<Rigidbody2D>(); if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>(); }
    void Update() { if (cd > 0) cd -= Time.deltaTime; }

    public void OnMove(InputValue v)
    {
        move = v.Get<Vector2>();
        if (move.sqrMagnitude > 0.0001f)
            lastFacing = (Mathf.Abs(move.x) >= Mathf.Abs(move.y)) ? new Vector2(Mathf.Sign(move.x), 0) : new Vector2(0, Mathf.Sign(move.y));
        anim?.SetFloat("Horizontal", lastFacing.x);
        anim?.SetFloat("Vertical", lastFacing.y);
        anim?.SetFloat("Speed", move.sqrMagnitude);
        if (sprite) sprite.flipX = lastFacing.x < 0f;
    }

    public void OnUse(InputValue v)
{
    if (!v.isPressed || cd>0) return;
    var it = inv?.CurrentItem; if (it==null || it.category!=ItemCategory.Weapon) return;

    if (it.weaponType == WeaponType.Bow){
        anim?.ResetTrigger("Shoot");
        anim?.SetTrigger("Shoot");        // Any State -> BowShoot
        cd = Mathf.Max(minCooldown, it.cooldown);
        return;
    }
    if (it.weaponType == WeaponType.Sword) {
        LockMove(true);
        anim?.ResetTrigger("Attack");
        anim?.SetTrigger("Attack");
        if (sprite) sprite.flipX = lastFacing.x < 0f;
        cd = Mathf.Max(minCooldown, it.cooldown);
        return;
    }
}

    
    public void ShootArrow(){
    var it = inv?.CurrentItem; if (it == null || it.weaponType != WeaponType.Bow) return;

    Vector2 origin = rb.position;
    // hướng trước mặt: lấy từ lastFacing
    Vector2 dir = (Mathf.Abs(lastFacing.x) >= Mathf.Abs(lastFacing.y))
        ? new Vector2(Mathf.Sign(lastFacing.x), 0f)
        : new Vector2(0f, Mathf.Sign(lastFacing.y));

    var go = Instantiate(it.projectilePrefab, origin, Quaternion.identity);
    var proj = go.GetComponent<ArrowProjectile>() ?? go.AddComponent<ArrowProjectile>();
    proj.Init(it.power, dir, it.projectileSpeed, enemyMask);
    go.transform.right = dir; // xoay sprite mũi tên
}

    // Animation Events trên clip Attack:
    public void AttackStart() { LockMove(true); } // đặt ở đầu clip

    public void AttackHit() // gọi bằng Animation Event
    {
        var it = inv?.CurrentItem; if (it == null) return;

        float dist = it.range > 0f ? it.range : 0.6f;      // lấy từ ItemSO
        float rad = defaultHitRadius;

        Vector2 origin = rb.position;
        Vector2 dir = (Mathf.Abs(lastFacing.x) >= Mathf.Abs(lastFacing.y))
                    ? new Vector2(Mathf.Sign(lastFacing.x), 0)
                    : new Vector2(0, Mathf.Sign(lastFacing.y));

        Vector2 center = origin + dir * dist;

        var hits = Physics2D.OverlapCircleAll(center, rad, enemyMask);
        foreach (var c in hits)
            c.GetComponentInParent<IDamageable>()?.TakeHit(it.power);

        Debug.Log($"Hit with {it.name} | range={it.range} | usedDist={dist} | hits={hits.Length}");
    }
void OnDrawGizmosSelected(){
    if (!Application.isPlaying) return;
    var it = inv?.CurrentItem; if (it == null) return;
    float dist = it.range > 0 ? it.range : 0.6f;
    float rad  = 0.35f;
    Vector2 origin = (Vector2)transform.position;
    Vector2 dir = (Mathf.Abs(lastFacing.x)>=Mathf.Abs(lastFacing.y)) ? new Vector2(Mathf.Sign(lastFacing.x),0) : new Vector2(0,Mathf.Sign(lastFacing.y));
    Vector2 center = origin + dir * dist;
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(center, rad);
}


    public void AttackEnd() { LockMove(false); }  // đặt ở cuối clip

    void LockMove(bool on)
    {
        if (controller) controller.canMove = !on;
        rb.velocity = Vector2.zero;
    }
}
