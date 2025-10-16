using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI2D : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Transform player;
    public Transform homeCenter; // nếu null sẽ lấy vị trí ban đầu làm tâm

    [Header("Vùng hoạt động")]
    public float homeRadius = 8f;          // bán kính vùng hoạt động
    public float leashFactor = 1.2f;       // cho phép dí ra ngoài 20% rồi mới quay về

    [Header("Di chuyển")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float detectionRadius = 6f;
    public float waypointTolerance = 0.3f;
    public float repathEvery = 2f;         // giây: đổi mục tiêu tuần tra

    [Header("Tránh vật cản")]
    public LayerMask obstacleMask;
    public float obstacleProbeRadius = 0.3f;
    public float obstacleProbeDistance = 1.0f;
    public float obstacleAvoidForce = 3f;

    [Header("Né đồng loại")]
    public LayerMask allyMask;             // đặt tất cả Enemy cùng Layer
    public float separationRadius = 1.2f;
    public float separationForce = 2f;

    Rigidbody2D rb;
    Vector2 homePos;
    Vector2 patrolTarget;
    float nextRepath;

    enum State { Patrol, Chase, Return }
    State state = State.Patrol;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Start()
    {
        homePos = homeCenter ? (Vector2)homeCenter.position : (Vector2)transform.position;
        PickNewPatrolTarget();
    }

    void FixedUpdate()
    {
        Vector2 pos = rb.position;
        float distHome = Vector2.Distance(pos, homePos);
        bool playerValid = player != null;
        bool playerInDetect = playerValid && Vector2.Distance(pos, player.position) <= detectionRadius;

        // Chuyển trạng thái
        switch (state)
        {
            case State.Patrol:
                if (playerInDetect) state = State.Chase;
                if (Time.time >= nextRepath || Vector2.Distance(pos, patrolTarget) <= waypointTolerance)
                    PickNewPatrolTarget();
                break;

            case State.Chase:
                if (!playerValid) { state = State.Return; break; }
                if (distHome > homeRadius * leashFactor) state = State.Return;
                if (!playerInDetect && distHome <= homeRadius) state = State.Patrol;
                break;

            case State.Return:
                if (distHome <= homeRadius * 0.95f)
                    state = playerInDetect ? State.Chase : State.Patrol;
                break;
        }

        // Mục tiêu và tốc độ theo trạng thái
        Vector2 target;
        float speed;
        if (state == State.Chase && playerValid)
        {
            target = (Vector2)player.position;
            speed = chaseSpeed;
        }
        else if (state == State.Return)
        {
            target = homePos;
            speed = chaseSpeed;
        }
        else
        {
            target = patrolTarget;
            speed = patrolSpeed;
        }

        // Hướng cơ sở
        Vector2 desired = (target - pos);
        if (desired.sqrMagnitude > 1f) desired.Normalize();

        // Tránh vật cản bằng CircleCast phía trước
        if (desired.sqrMagnitude > 0.0001f)
        {
            RaycastHit2D hit = Physics2D.CircleCast(pos, obstacleProbeRadius, desired, obstacleProbeDistance, obstacleMask);
            if (hit.collider)
            {
                Vector2 away = Vector2.Reflect(desired, hit.normal); // lệch khỏi bề mặt va chạm
                desired += away.normalized * obstacleAvoidForce;
            }
        }

        // Né đồng loại (separation)
        Collider2D[] allies = Physics2D.OverlapCircleAll(pos, separationRadius, allyMask);
        foreach (var c in allies)
        {
            if (c.attachedRigidbody && c.attachedRigidbody != rb)
            {
                Vector2 diff = pos - c.attachedRigidbody.position;
                float d = diff.magnitude;
                if (d > 0.01f) desired += diff.normalized * (separationForce / d);
            }
        }

        desired = desired.normalized;
        rb.velocity = desired * speed;

        // Xoay theo hướng di chuyển (tùy chọn 2D top-down)
        // if (rb.velocity.sqrMagnitude > 0.01f)
        //     transform.right = rb.velocity.normalized;
    }

    void PickNewPatrolTarget()
    {
        Vector2 offset = Random.insideUnitCircle * (homeRadius * 0.9f);
        patrolTarget = homePos + offset;
        nextRepath = Time.time + repathEvery;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = homeCenter ? homeCenter.position : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, homeRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere((Vector3)patrolTarget, 0.15f);
    }
}
