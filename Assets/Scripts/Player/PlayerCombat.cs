using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Equip")]
    [SerializeField] WeaponSO sword;
    [SerializeField] WeaponSO bow;

    [Header("Refs")]
    [SerializeField] Animator anim;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] LayerMask enemyMask;
    [SerializeField] PlayerController mover;   // để khóa di chuyển

    Rigidbody2D rb;
    Vector2 moveInput, lastFacing = Vector2.down;
    float cd;
    WeaponSO lastUsed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!anim)   anim   = GetComponent<Animator>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        if (!mover)  mover  = GetComponent<PlayerController>();

        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void OnMove(InputValue v)
    {
        moveInput = v.Get<Vector2>();
        if (moveInput.sqrMagnitude > 0.0001f)
            lastFacing = (Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y))
                ? new Vector2(Mathf.Sign(moveInput.x), 0)
                : new Vector2(0, Mathf.Sign(moveInput.y));
    }

    public void OnUse(InputValue v)
    {
        if (!v.isPressed || cd > 0) return;

        WeaponSO w = (bow != null && IsBowSelected()) ? bow : sword;
        if (w == null) return;

        lastUsed = w;

        if (w.kind == WeaponKind.Sword)
        {
            anim.SetTrigger("Attack");           // Attack clip có AttackStart/AttackHit/AttackEnd
            cd = w.cooldown;
        }
        else // Bow
        {
            // Khoá ngay, mở sau theo cooldown hoặc Animation Event nếu bạn có clip bắn
            mover?.SetMoveLock(true);
            ShootArrow(w);
            StartCoroutine(UnlockAfter(w.cooldown));
            cd = w.cooldown;
        }
    }

    void Update()
    {
        if (sprite) sprite.flipX = lastFacing.x < 0f;
        cd = Mathf.Max(0, cd - Time.deltaTime);
        var st = anim.GetCurrentAnimatorStateInfo(0);
        if (mover.MoveLocked && !st.IsTag("Action") && !anim.IsInTransition(0))
            mover.SetMoveLock(false);
    }

    // ===== Sword (gọi bằng Animation Events) =====
    public void AttackStart() { mover?.SetMoveLock(true); }   // đặt event ở frame đầu
    public void AttackHit()
    {
        if (lastUsed == null || lastUsed.kind != WeaponKind.Sword) return;
        Vector2 dir = (moveInput.sqrMagnitude > 0.0001f) ? moveInput.normalized : lastFacing;
        Vector2 center = rb.position + dir.normalized * lastUsed.range;
        var cols = Physics2D.OverlapCircleAll(center, lastUsed.hitRadius, enemyMask);

        var visited = new System.Collections.Generic.HashSet<Transform>();
        foreach (var c in cols)
        {
            var root = (c.attachedRigidbody ? c.attachedRigidbody.transform : c.transform).root;
            if (!visited.Add(root)) continue;
            var d = root.GetComponentInParent<IDamageable>();
            if (d != null) d.TakeHit(lastUsed.damage);
        }
    }
    public void AttackEnd()  { mover?.SetMoveLock(false); }   // đặt event ở frame cuối

    // ===== Bow =====
    void ShootArrow(WeaponSO w)
    {
        if (!w.projectilePrefab) return;
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouse.z = 0;
        Vector2 dir = ((Vector2)mouse - rb.position).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = (lastFacing == Vector2.zero) ? Vector2.down : lastFacing;

        Vector2 spawn = rb.position + dir * w.range;
        var go = Instantiate(w.projectilePrefab, spawn, Quaternion.identity);
        var proj = go.GetComponent<ArrowProjectile>();
        if (proj) proj.Fire(dir, w.projectileSpeed, w.damage, transform);
    }

    System.Collections.IEnumerator UnlockAfter(float t)
    {
        yield return new WaitForSeconds(t);
        mover?.SetMoveLock(false);
    }

    bool IsBowSelected()
    {
        // tạm thời: giữ chuột phải để chọn cung
        return Mouse.current.rightButton.isPressed;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || lastUsed == null || lastUsed.kind != WeaponKind.Sword) return;
        Vector2 dir = (moveInput.sqrMagnitude > 0.0001f) ? moveInput.normalized : lastFacing;
        Vector2 c = (Vector2)transform.position + dir * lastUsed.range;
        Gizmos.DrawWireSphere(c, lastUsed.hitRadius);
    }
#endif
}
