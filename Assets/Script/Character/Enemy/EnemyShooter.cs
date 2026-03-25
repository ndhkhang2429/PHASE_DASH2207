using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : EnemyBase
{
    [Header("Patrol")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    public bool canMove = true;

    [Header("Shoot")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform target;
    [SerializeField] private float shootRange;
    [SerializeField] private float shootCooldown;
    [SerializeField] private float bulletSpeed;

    private Transform player;
    private float lastShootTime;
    private bool movingRight = true;
    private bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        leftPoint.parent = null;
        rightPoint.parent = null;
    }
    protected override void LogicUpdate()
    {
        if (isDead) return;

        if (PlayerInRange())
        {
            isAttacking = true;
            AttackState();
        }
        else
        {
            isAttacking = false;
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

        if (movingRight)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);

            if (transform.position.x >= rightPoint.position.x)
            {
                movingRight = false;
                Flip();
            }
        }
        else
        {
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);

            if (transform.position.x <= leftPoint.position.x)
            {
                movingRight = true;
                Flip();
            }
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

    public void FireBullet()
    {
        if (player == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Vector2 direction = (player.position - firePoint.position).normalized;

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.velocity = direction * bulletSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void FacePlayer()
    {
        if (player.position.x > transform.position.x && !movingRight)
        {
            movingRight = true;
            Flip();
        }
        else if (player.position.x < transform.position.x && movingRight)
        {
            movingRight = false;
            Flip();
        }
    }
}
