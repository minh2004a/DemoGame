using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] Animator anim;
    [SerializeField] SpriteRenderer sprite;
    public bool canMove = true;

    Rigidbody2D rb;
    Vector2 moveInput;
    Vector2 lastFacing = Vector2.right;

    public bool MoveLocked { get; private set; }
    public void SetMoveLock(bool locked)
    {
        MoveLocked = locked;
        if (locked) rb.velocity = Vector2.zero;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void OnMove(InputValue v)
{
    if (!canMove) { moveInput = Vector2.zero; return; }
    moveInput = v.Get<Vector2>();

    if (moveInput.sqrMagnitude > 0.0001f)
    {
        // Ưu tiên ngang khi chéo với W (AW/DW)
        if (moveInput.y > 0.01f && Mathf.Abs(moveInput.x) > 0.01f)
            lastFacing = new Vector2(Mathf.Sign(moveInput.x), 0f);
        else if (Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y))
            lastFacing = new Vector2(Mathf.Sign(moveInput.x), 0f);   // ưu tiên ngang mọi chéo
        else
            lastFacing = new Vector2(0f, Mathf.Sign(moveInput.y));   // lên/xuống
    }
}


    void Update()
    {
        if (sprite) sprite.flipX = lastFacing.x < 0f;
        if (anim)
        {
            anim.SetFloat("Horizontal", Mathf.Abs(lastFacing.x));
            anim.SetFloat("Vertical",   lastFacing.y);
            anim.SetFloat("Speed",      MoveLocked ? 0f : moveInput.sqrMagnitude);
        }
    }

    void FixedUpdate()
    {
        rb.velocity = canMove ? moveInput.normalized * moveSpeed : Vector2.zero;
    }
}
