using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 4f;

    Rigidbody2D rb;
    Vector2 moveInput;
    Vector2 lastFacing = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // PlayerInput (Behavior = Send Messages) sẽ tự gọi
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (moveInput.sqrMagnitude > 0.0001f) lastFacing = moveInput.normalized;
    }

    void FixedUpdate()
    {
        rb.velocity = moveInput.normalized * moveSpeed;
    }

    public Vector2 Facing => (moveInput.sqrMagnitude > 0.0001f) ? moveInput.normalized : lastFacing;
}
