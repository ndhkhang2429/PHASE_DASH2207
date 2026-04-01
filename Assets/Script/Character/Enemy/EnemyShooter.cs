using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class EnemyShooter : EnemyBase
{
    [Header("Patrol")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    public bool canMove = true;
    private int moveDirection = 1; // 1: Phải, -1: Trái

    // Biến lưu giới hạn gốc (an toàn cho Boss đẻ quái)
    private float defaultLeftLimit;
    private float defaultRightLimit;

    [Header("Shoot")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootRange;
    [SerializeField] private float shootCooldown;
    [SerializeField] private float bulletSpeed;

    private Transform player;
    private float lastShootTime;

    protected override void Awake()
    {
        base.Awake();

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        // CHỐT GIỚI HẠN GỐC (Tránh lỗi Null khi Boss đẻ quái)
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

        // Đảm bảo hướng đi ban đầu khớp với sprite
        moveDirection = isFacingRight ? 1 : -1;
    }

    // --- LẤY GIỚI HẠN HIỆN TẠI (Ưu tiên Lock Room) ---
    private float GetCurrentLeftLimit()
    {
        return hasRoomLimits ? roomLeftLimit : defaultLeftLimit;
    }

    private float GetCurrentRightLimit()
    {
        return hasRoomLimits ? roomRightLimit : defaultRightLimit;
    }
    // ------------------------------------------------

    protected override void LogicUpdate()
    {
        if (player == null || isDead) return;

        if (PlayerInRange())
        {
            AttackState();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (!canMove)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetBool("isWalking", false);
            return;
        }

        animator.SetBool("isWalking", true);
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

    private bool PlayerInRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= shootRange;
    }

    private void AttackState()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetBool("isWalking", false);

        FacePlayer();

        if (Time.time >= lastShootTime + shootCooldown)
        {
            lastShootTime = Time.time;
            animator.SetTrigger("Shoot");
        }
    }
    public void PlayFakeHurt()
    {
        enemyAudio.PlayCustom(enemyAudio.attackSound, 0.4f, 0.1f); // Pitch cực thấp để tạo tiếng "Hự" trầm
        enemyAudio.PlayHurt();
    }
    public void FireBullet() // Hàm này gọi bằng Animation Event
    {
        if (player == null) return;
        if (enemyAudio != null) enemyAudio.PlayAttack();

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Vector2 direction = (player.position - firePoint.position).normalized;

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.velocity = direction * bulletSpeed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void FacePlayer()
    {
        int directionToPlayer = player.position.x > transform.position.x ? 1 : -1;
        SetDirection(directionToPlayer);
    }
}