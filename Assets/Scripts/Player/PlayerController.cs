using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] Animator anim;
    [SerializeField] SpriteRenderer sprite; // gán trong Inspector (hoặc lấy ở child)

    Rigidbody2D rb;
    Vector2 moveInput;
    Vector2 lastFacing = Vector2.right; // clip gốc vẽ hướng phải

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void OnMove(InputValue v) // PlayerInput: Send Messages
    {
        moveInput = v.Get<Vector2>();
        if (moveInput.sqrMagnitude > 0.0001f)
        {
            // ưu tiên ngang khi chéo
            if (Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y))
                lastFacing = new Vector2(Mathf.Sign(moveInput.x), 0f);
            else
                lastFacing = new Vector2(0f, Mathf.Sign(moveInput.y));
        }
    }

    void Update()
    {
        // flip ngang bằng SpriteRenderer
        if (sprite) sprite.flipX = lastFacing.x < 0f; // trái = true, phải = false  :contentReference[oaicite:2]{index=2}

        // blend tree 3 hướng: Up/Down/Side(1,0)
        if (anim)
        {
            anim.SetFloat("Horizontal", Mathf.Abs(lastFacing.x)); // 0 hoặc 1
            anim.SetFloat("Vertical", lastFacing.y);            // -1,0,1
            anim.SetFloat("Speed", moveInput.sqrMagnitude);
        }
    }

    void FixedUpdate()
    {
        rb.velocity = moveInput.normalized * moveSpeed;
    }
}
