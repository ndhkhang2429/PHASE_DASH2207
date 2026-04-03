using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private int maxDamage = 40;
    [SerializeField] private float maxKnockbackForce = 12f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int touchDamage = 5;


    [Header("Chase")]
    [SerializeField] private float chaseSpeed;

    private State currentState;
    private Transform player;
    private int moveDirection = 1;

    // Biến lưu giới hạn gốc (nếu có PointA, PointB)
    private float defaultLeftLimit;
    private float defaultRightLimit;

    private bool isExploding = false;
    private Vector3 originalScale;

    protected override void Awake()
    {
        base.Awake();

        // 1. Chốt giới hạn gốc nếu có cài đặt ở Inspector
        if (PointA != null && PointB != null)
        {
            // Tìm bên nào trái, bên nào phải cho chắc ăn
            defaultLeftLimit = Mathf.Min(PointA.position.x, PointB.position.x);
            defaultRightLimit = Mathf.Max(PointA.position.x, PointB.position.x);

            // Ngắt kết nối để không bị bê đi theo quái
            PointA.parent = null;
            PointB.parent = null;
        }
        else
        {
            // Nếu Boss đẻ ra (không có PointA, B), tạm thời lấy vị trí đứng làm gốc
            defaultLeftLimit = transform.position.x;
            defaultRightLimit = transform.position.x;
        }

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        currentState = State.Patrol;
    }

    // --- HÀM CỐT LÕI: Lấy giới hạn thực tế tại thời điểm hiện tại ---
    private float GetCurrentLeftLimit()
    {
        return hasRoomLimits ? roomLeftLimit : defaultLeftLimit;
    }

    private float GetCurrentRightLimit()
    {
        return hasRoomLimits ? roomRightLimit : defaultRightLimit;
    }
    // ----------------------------------------------------------------

    protected override void LogicUpdate()
    {
        if (isDead || player == null || isExploding) return;

        float distance = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                if (distance < detectRange)
                {
                    // 1. Phát tiếng "Á!" (Pitch cao cho the thé)
                    if (enemyAudio != null)
                    {
                        enemyAudio.PlayCustom(enemyAudio.spotSound, 1.8f, 0.1f);
                    }

                    // 2. Chuyển sang trạng thái rượt đuổi
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                ChaseLogic(distance); // Truyền distance vào để xử lý
                break;
        }
    }

    // Patrol
    private void PatrolLogic()
    {
        animator.SetBool("isRunning", true);
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        float currentLeft = GetCurrentLeftLimit();
        float currentRight = GetCurrentRightLimit();

        // Kiểm tra quay đầu dựa trên giới hạn TỔNG HỢP
        if (moveDirection == 1 && transform.position.x >= currentRight)
        {
            SetDirection(-1);
        }
        else if (moveDirection == -1 && transform.position.x <= currentLeft)
        {
            SetDirection(1);
        }
    }

    // Chase
    private void ChaseLogic(float distanceToPlayer)
    {
        animator.SetBool("isRunning", true);

        // 1. Kiểm tra xem có nổ không
        if (distanceToPlayer <= explodeRange)
        {
            StartCoroutine(ExplodeRoutine());
            return;
        }

        // 2. Kiểm tra xem Player có chạy mất không
        if (distanceToPlayer > detectRange + 2f)
        {
            currentState = State.Patrol;
            return;
        }
        // 3. Tiến hành đuổi
        int directionToPlayer = player.position.x > transform.position.x ? 1 : -1;
        float currentLeft = GetCurrentLeftLimit();
        float currentRight = GetCurrentRightLimit();

        // NẾU PLAYER NẰM NGOÀI GIỚI HẠN: Quái sẽ chạy đến mép giới hạn rồi đứng nhìn
        if ((directionToPlayer == 1 && transform.position.x >= currentRight) ||
            (directionToPlayer == -1 && transform.position.x <= currentLeft))
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // Đứng im ở mép
            SetDirection(directionToPlayer); // Vẫn quay mặt nhìn theo
        }
        // NẾU PLAYER NẰM TRONG GIỚI HẠN: Quái chạy đuổi theo
        else
        {
            SetDirection(directionToPlayer);
            rb.velocity = new Vector2(moveDirection * chaseSpeed, rb.velocity.y);
        }
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

    //Explode Routine

    private IEnumerator ExplodeRoutine()
    {
        isExploding = true;
        currentState = State.ExplodeWindup;
        animator.SetBool("isRunning", false);
        rb.velocity = Vector2.zero;

        if (enemyAudio != null)
        {
            // Dùng windupSound, Pitch tăng dần để tạo sự dồn dập
            enemyAudio.PlayCustom(enemyAudio.windupSound, 1f, 0.1f);
        }

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
        Destroy(gameObject, 0.1f);
    }

    public override void OnDeath()
    {
        if (isExploding) return; // Nếu đang gồng nổ thì không cho chạy anim chết thường

        base.OnDeath(); // Chạy logic chết bình thường của EnemyBase
    }

    private void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                // Gọi hàm trừ máu (Giữ đúng tên hàm TakeDame của bạn)
                player.TakeDame(touchDamage);
            }
        }
    }
}