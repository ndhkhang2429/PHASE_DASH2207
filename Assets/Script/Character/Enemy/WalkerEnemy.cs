using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerEnemy : EnemyBase
{
    [Header("Patrol Points")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    // Biến lưu giới hạn gốc (An toàn khi Boss đẻ quái)
    private float defaultLeftLimit;
    private float defaultRightLimit;
    private int moveDirection = 1;

    [Header("Walker Settings")]
    [SerializeField] private float detectRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooldown;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius;
    [SerializeField] private int attackDamage;
    [SerializeField] private LayerMask playerLayer;

    private Transform player;
    private float lastAttackTime;

    protected override void Awake()
    {
        base.Awake();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // CHỐT GIỚI HẠN AN TOÀN
        if (leftPoint != null && rightPoint != null)
        {
            defaultLeftLimit = Mathf.Min(leftPoint.position.x, rightPoint.position.x);
            defaultRightLimit = Mathf.Max(leftPoint.position.x, rightPoint.position.x);
            leftPoint.parent = null;
            rightPoint.parent = null;
        }
        else
        {
            defaultLeftLimit = transform.position.x;
            defaultRightLimit = transform.position.x;
        }

        moveDirection = isFacingRight ? 1 : -1;
    }

    // --- LẤY GIỚI HẠN TỔNG HỢP ---
    private float GetCurrentLeftLimit()
    {
        return hasRoomLimits ? roomLeftLimit : defaultLeftLimit;
    }

    private float GetCurrentRightLimit()
    {
        return hasRoomLimits ? roomRightLimit : defaultRightLimit;
    }
    // -----------------------------

    protected override void LogicUpdate()
    {
        if (player == null || isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // NẾU: Đủ gần VÀ đang đứng trong giới hạn tuần tra
        if (distanceToPlayer <= detectRange && IsInsidePatrolZone())
        {
            // Tấn công nếu trong tầm đánh
            if (distanceToPlayer <= attackRange)
            {
                TryAttack();
            }
            // Ngược lại thì đuổi
            else
            {
                Chase();
            }
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        animator.SetBool("isRunning", true);
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        float currentLeft = GetCurrentLeftLimit();
        float currentRight = GetCurrentRightLimit();

        // KIỂM TRA QUAY ĐẦU (Gộp chung)
        if (moveDirection == 1 && transform.position.x >= currentRight)
        {
            SetDirection(-1);
        }
        else if (moveDirection == -1 && transform.position.x <= currentLeft)
        {
            SetDirection(1);
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

    private void Chase()
    {
        int directionToPlayer = player.position.x > transform.position.x ? 1 : -1;

        float currentLeft = GetCurrentLeftLimit();
        float currentRight = GetCurrentRightLimit();

        // Không cho đuổi vượt khỏi vùng cho phép
        if ((directionToPlayer == 1 && transform.position.x >= currentRight) ||
            (directionToPlayer == -1 && transform.position.x <= currentLeft))
        {
            // Chạm mép thì dừng lại, không rượt nữa
            rb.velocity = new Vector2(0, rb.velocity.y);
            SetDirection(directionToPlayer); // Nhìn theo Player
            animator.SetBool("isRunning", false);
            return;
        }

        SetDirection(directionToPlayer);
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
        animator.SetBool("isRunning", true);
    }

    private void TryAttack()
    {
        // Khóa di chuyển khi đang vung vuốt
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetBool("isRunning", false);

        // Quay mặt về phía Player trước khi cào
        int directionToPlayer = player.position.x > transform.position.x ? 1 : -1;
        SetDirection(directionToPlayer);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");
        }
    }

    // Animation Event
    private void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            Player playerHealth = hit.GetComponent<Player>();
            if (playerHealth != null)
            {
                // Truyền sát thương (Tùy theo cấu trúc hàm TakeDamage của Player mà bạn có thể thêm Knockback)
                playerHealth.TakeDamage(attackDamage, Vector2.zero, 0f);
            }
        }
    }

    // Kiểm tra xem quái có đang đứng trong vùng an toàn không
    private bool IsInsidePatrolZone()
    {
        float pX = transform.position.x;
        // Mở rộng thêm 0.5f để quái không bị "khựng" khi vừa chạm sát ranh giới
        return pX >= GetCurrentLeftLimit() - 0.5f && pX <= GetCurrentRightLimit() + 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}