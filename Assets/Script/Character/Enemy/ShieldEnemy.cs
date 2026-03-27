using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShieldEnemy : EnemyBase
{
    private enum State
    {
        Patrol,
        Shield,
        Attack,
    }

    [Header("Patrol")]
    [SerializeField] private Transform PointA;
    [SerializeField] private Transform PointB;

    // Biến lưu giới hạn gốc (An toàn khi Boss đẻ quái)
    private float defaultLeftLimit;
    private float defaultRightLimit;
    private int moveDirection = 1;

    [Header("Combat")]
    [SerializeField] private float detectRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float turnDelay = 0.5f;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackSize;
    [SerializeField] private int attackDamage;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackKnockbackForce;

    private State currentState;
    private Transform player;

    private bool isShielding = false; // Mặc định đi tuần là không giương khiên
    private float attackTimer;
    private bool isAttacking;
    private float turnTimer = 0f;

    protected override void Awake()
    {
        base.Awake();

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        if (health == null) health = GetComponent<EnemyHealth>();

        // CHỐT GIỚI HẠN GỐC AN TOÀN
        if (PointA != null && PointB != null)
        {
            defaultLeftLimit = Mathf.Min(PointA.position.x, PointB.position.x);
            defaultRightLimit = Mathf.Max(PointA.position.x, PointB.position.x);
            PointA.parent = null;
            PointB.parent = null;
        }
        else
        {
            defaultLeftLimit = transform.position.x;
            defaultRightLimit = transform.position.x;
        }

        currentState = State.Patrol;
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
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        attackTimer += Time.deltaTime;

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic(distance);
                break;

            case State.Shield:
                ShieldLogic(distance);
                break;

            case State.Attack:
                // Trạng thái Attack được khóa chết bởi animation, không làm gì ở đây
                break;
        }
    }

    // Patrol
    private void PatrolLogic(float distance)
    {
        isShielding = false;
        animator.SetBool("isRunning", true);
        animator.SetBool("isShield", false);

        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        float currentLeft = GetCurrentLeftLimit();
        float currentRight = GetCurrentRightLimit();

        // GỘP CHUNG KIỂM TRA QUAY ĐẦU
        if (moveDirection == 1 && transform.position.x >= currentRight)
        {
            SetDirection(-1);
        }
        else if (moveDirection == -1 && transform.position.x <= currentLeft)
        {
            SetDirection(1);
        }

        // Chuyển sang Shield nếu thấy Player
        if (distance <= detectRange)
        {
            currentState = State.Shield;
        }
    }

    // Shield (Bám theo mặt Player và canh me đánh)
    private void ShieldLogic(float distance)
    {
        // Khi giương khiên thì đứng im
        rb.velocity = new Vector2(0, rb.velocity.y);
        isShielding = true;

        animator.SetBool("isRunning", false);
        animator.SetBool("isShield", true);

        // Từ từ quay mặt về phía Player
        FacePlayer();

        // LOGIC CHUYỂN TRẠNG THÁI:
        // 1. Đánh nếu đủ gần và hồi chiêu xong
        if (distance <= attackRange && attackTimer >= attackCooldown)
        {
            AttackLogic();
        }
        // 2. Quay lại đi tuần nếu Player chạy xa
        else if (distance > detectRange)
        {
            currentState = State.Patrol;
            turnTimer = 0f; // Reset đồng hồ quay mặt
        }
    }

    private void AttackLogic()
    {
        if (isAttacking) return;

        isAttacking = true;
        isShielding = false; // Bỏ khiên xuống để chém
        currentState = State.Attack;

        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetBool("isShield", false);
        animator.SetTrigger("Attack");
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

    // Face player có độ trễ (Giữ nguyên logic cực hay của bạn)
    private void FacePlayer()
    {
        bool playerIsOnRight = player.position.x > transform.position.x;

        if ((playerIsOnRight && !isFacingRight) || (!playerIsOnRight && isFacingRight))
        {
            turnTimer += Time.deltaTime;
            if (turnTimer >= turnDelay)
            {
                SetDirection(playerIsOnRight ? 1 : -1);
                turnTimer = 0f;
            }
        }
        else
        {
            turnTimer = 0f;
        }
    }

    // Animation Event: Gọi ở frame vũ khí chém trúng
    public void DealDamage()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(attackPoint.position, attackSize, 0f, playerLayer);

        foreach (Collider2D p in hitPlayers)
        {
            Player playerHealth = p.GetComponent<Player>();
            if (playerHealth != null)
            {
                Vector2 knockDir = (p.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(attackDamage, knockDir, attackKnockbackForce);
            }
        }
    }

    // Animation Event: Gọi ở frame cuối của Animation Attack
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        attackTimer = 0f;

        // Sau khi chém xong, tự động lui về thủ
        currentState = State.Shield;
    }

    // Hàm gọi khi nhận sát thương (Dùng chung với EnemyHealth)
    public bool TryTakeDamage(int damage, Transform attacker, float knockbackForce)
    {
        if (health == null || health.IsDead) return false;

        Vector2 direction = attacker.position - transform.position;

        // Xác định đòn đánh đến từ phía trước
        bool attackerInFront = (isFacingRight && direction.x > 0) || (!isFacingRight && direction.x < 0);

        // Nếu ĐANG THỦ và ĐÁNH TỪ MẶT TRƯỚC -> Chặn sát thương
        if (isShielding && attackerInFront)
        {
            Debug.Log("Đã dùng Khiên đỡ thành công!");
            // Tùy chọn: Chạy animation "Block" ở đây nếu có
            return false;
        }

        // Nếu ĐÁNH LÉN TỪ SAU LƯNG hoặc ĐANG CHÉM HỞ SƯỜN -> Nhận sát thương
        Vector2 knockbackDir = (transform.position - attacker.position).normalized;
        health.TakeDamage(damage, knockbackDir, knockbackForce);

        if (health.IsDead)
        {
            Die();
            return true;
        }

        // Bị đánh đau quá thì bỏ chém, quay về thủ ngay lập tức
        isAttacking = false;
        currentState = State.Shield;
        return true;
    }

    private void Die()
    {
        // Ghi đè thay vì dùng OnDeath() của Base vì bạn có logic riêng
        isDead = true;
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Die");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log("Shield Enemy Dead");
    }

    // Dùng cho Animation Event: Gọi ở cuối anim chết để dọn dẹp xác
    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(attackPoint.position, attackSize);
    }
}