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
        if (moveInput.sqrMagnitude > 0.0001f) lastFacing = moveInput.normalized;
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
