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
    [SerializeField] float attackDistance = 0.6f; // tâm hit trước mặt
    [SerializeField] float hitRadius = 0.35f;
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
        if (!v.isPressed || cd > 0) return;
        var it = inv?.CurrentItem;
        if (it == null || it.category != ItemCategory.Weapon || it.weaponType != WeaponType.Sword) return;

        LockMove(true);                       // khóa di chuyển
        anim?.ResetTrigger("Attack");
        anim?.SetTrigger("Attack");           // Any State -> Attack theo Trigger
        if (sprite) sprite.flipX = lastFacing.x < 0f;
        cd = Mathf.Max(minCooldown, it.cooldown);
    }

    // Animation Events trên clip Attack:
    public void AttackStart() { LockMove(true); } // đặt ở đầu clip

    public void AttackHit()
    {                     // đặt đúng frame chém trúng
        var it = inv?.CurrentItem; if (it == null) return;
        Vector2 origin = rb.position;
        Vector2 dir = (Mathf.Abs(lastFacing.x) >= Mathf.Abs(lastFacing.y)) ? new Vector2(Mathf.Sign(lastFacing.x), 0) : new Vector2(0, Mathf.Sign(lastFacing.y));
        Vector2 center = origin + dir * attackDistance;

        var hits = Physics2D.OverlapCircleAll(center, hitRadius, enemyMask);
        foreach (var c in hits) c.GetComponentInParent<IDamageable>()?.TakeHit(it.power);
        Debug.DrawRay(origin, dir * attackDistance, Color.yellow, 0.15f);
    }

    public void AttackEnd() { LockMove(false); }  // đặt ở cuối clip

    void LockMove(bool on)
    {
        if (controller) controller.canMove = !on;
        rb.velocity = Vector2.zero;
    }
}
