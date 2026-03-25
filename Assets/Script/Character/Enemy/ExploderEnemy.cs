using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class ExploderEnemy : EnemyBase
{
    private enum State
    {
        Patrol, //di qua lai giua 2 diem
        Chase, //duoi theo player
        ExplodeWindup,//chuan bi no
        Explode//phat no
    };

    [Header("VFX")]
    [SerializeField] private GameObject explosionVFXPrefab;

    [Header("Patrol")]
    [SerializeField] private Transform PointA;
    [SerializeField] private Transform PointB;

    [Header("Detection")]
    [SerializeField] private float detectRange; //khoang cach bat dau duoi
    [SerializeField] private float explodeRange;//khoangcach kich hoat no

    [Header("Explosion")]
    [SerializeField] private float windupTime;//thoi gian cho truoc khi no
    [SerializeField] private float explosionRadius;//ban kinh gay dame
    [SerializeField] private int damage;
    [SerializeField] private int maxDamage = 40;
    [SerializeField] private float maxKnockbackForce = 12f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed;

    private State currentState;
    private Transform player;
    private Transform targetPoint;

    private int moveDirection = 1;
    private float leftLimit;
    private float rightLimit;

    private bool isExploding = false;
    private Vector3 originalScale;

    protected override void Awake()
    {
        base.Awake();
        leftLimit = PointA.position.x;
        rightLimit = PointB.position.x;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        currentState = State.Patrol;
        targetPoint = PointB;

    }

    protected override void LogicUpdate()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                if (distance < detectRange) currentState = State.Chase;
                break;

            case State.Chase:
                ChaseLogic();
                if (distance < explodeRange && !isExploding)
                {
                    StartCoroutine(ExplodeRoutine());
                }
                else if (distance > detectRange + 2f) // Player cắt đuôi thành công
                {
                    currentState = State.Patrol;
                }
                break;
        }
    }

    //Patrol
    private void PatrolLogic()
    {
        animator.SetBool("isRunning", true);
        
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        if (moveDirection == 1 && transform.position.x >= rightLimit)
            SetDirection(-1);
        else if (moveDirection == -1 && transform.position.x <= leftLimit)
            SetDirection(1);

    }
    private void SetDirection(int direction)
    {
        moveDirection = direction;
        if ((moveDirection == 1 && !isFacingRight) ||
        (moveDirection == -1 && isFacingRight))
        {
            Flip();
        }
    }

    //Chase
    private void ChaseLogic()
    {
        animator.SetBool("isRunning", true);
        int directionToPlayer = player.position.x > transform.position.x ? 1 : -1;

        // Không cho vượt khỏi patrol zone
        if ((directionToPlayer == 1 && transform.position.x >= PointA.position.x) ||
            (directionToPlayer == -1 && transform.position.x <= PointB.position.x))
        {
            rb.velocity = new Vector2(moveDirection * chaseSpeed, rb.velocity.y);

            if (moveDirection == 1 && transform.position.x >= rightLimit)
                SetDirection(-1);
            else if (moveDirection == -1 && transform.position.x <= leftLimit)
                SetDirection(1);
            return;
        }

        SetDirection(directionToPlayer);

        rb.velocity = new Vector2(moveDirection * chaseSpeed, rb.velocity.y);
        
    }

    //Explode Routine
    private IEnumerator ExplodeRoutine()
    {
        isExploding = true;
        currentState = State.ExplodeWindup;
        animator.SetBool("isRunning", false);

        rb.velocity = Vector2.zero;

        float timer = 0f;

        Color originalColor = spriteRenderer.color;
        Vector3 startScale = originalScale;
        Vector3 maxScale = originalScale * 1.2f;

        //Windup phase: nhấp nháy + phình to
        while (timer < windupTime)
        {
            timer += Time.deltaTime;

            float progress = timer / windupTime;

            //scale phinh dan
            transform.localScale = Vector3.Lerp(startScale, maxScale, progress);

            //nhap nhay
            float blinkSpeed = Mathf.Lerp(5f, 25f, progress);
            float blink = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, Color.red, blink);

            yield return null;
        }

        ExecuteExplosion();
    }

    public void ExecuteExplosion()
    {
        if (currentState == State.Explode) return; // Tránh nổ 2 lần
        currentState = State.Explode;

        if (explosionVFXPrefab != null)
        {
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
        }

        // Tìm tất cả vật thể trong bán kính nổ
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            Player p = hit.GetComponent<Player>();
            if (p != null)
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);

                // Tỉ lệ khoảng cách: 1 ở tâm, 0 ở rìa
                float proximity = Mathf.Clamp01(1 - (distance / explosionRadius));

                // Game Feel: Tính toán Dame và Knockback giảm dần
                int finalDamage = Mathf.CeilToInt(maxDamage * proximity);
                float finalKnockback = maxKnockbackForce * proximity;
                Vector2 knockDir = (hit.transform.position - transform.position).normalized;

                // Tối thiểu vẫn có một chút lực đẩy nếu lỡ dính rìa
                finalKnockback = Mathf.Max(finalKnockback, 2f);

                p.TakeDamage(finalDamage, knockDir, finalKnockback);
            }
        }

        Destroy(gameObject, 0.5f);
    }

    // Override lại hàm nhận Dame để nếu quái chết thì nổ luôn
    public void TakeDamage(int damage) // Giả sử hàm này từ EnemyBase hoặc EnemyHealth
    {
        // Nếu bạn có hệ thống máu, khi máu <= 0 thì gọi ExecuteExplosion()
        // Để tránh việc quái chết im lìm.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explodeRange);
    }
}
