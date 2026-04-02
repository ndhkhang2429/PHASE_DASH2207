using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    [Header("Patrol")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    // Biến lưu giới hạn gốc (An toàn khi Boss đẻ quái)
    private float defaultLeftLimit;
    private float defaultRightLimit;
    private int moveDirection = 1;

    [Header("Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float detectRange;
    [SerializeField] private float fireCooldown;

    private float fireTimer;
    private Transform player;

    protected override void Awake()
    {
        base.Awake();

        // Đặt mặc định mặt quay sang trái hoặc phải tùy sprite của bạn
        isFacingRight = false;

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

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
        if (isDead) return;

        Patrol();
        DetectAndShoot();
    }

    private void Patrol()
    {
        animator.SetBool("isFlying", true);

        // Di chuyển bằng velocity thay vì MoveTowards để đồng bộ với cơ chế vật lý
        // Lưu ý: Đảm bảo Rigidbody2D của quái bay này có Gravity Scale = 0
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

    private void DetectAndShoot()
    {
        if (player == null) return;

        fireTimer += Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < detectRange && fireTimer >= fireCooldown)
        {
            Shoot(player);
            fireTimer = 0f;
        }
    }

    private void Shoot(Transform targetPlayer)
    {
        if (enemyAudio != null)
        {
            // Air Puff với Pitch cao (1.6f) tạo cảm giác ma thuật/lửa
            enemyAudio.PlayCustom(enemyAudio.attackSound, 1.6f, 0.1f);
        }
        // Kiểm tra và quay mặt về phía Player trước khi bắn
        if (targetPlayer.position.x > transform.position.x && !isFacingRight)
            SetDirection(1);
        else if (targetPlayer.position.x < transform.position.x && isFacingRight)
            SetDirection(-1);

        Vector2 direction = (targetPlayer.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Kiểm tra an toàn trước khi gọi hàm SetDirection của đạn
        FlyingEnemyBullet bulletScript = bullet.GetComponent<FlyingEnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
        }

        animator.SetTrigger("Shoot");
    }
}