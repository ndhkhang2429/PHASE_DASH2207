using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class WalkerEnemy : EnemyBase
{
    [Header("Patrol Points")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    [Header("Walker Settings")]
    [SerializeField] private float detectRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooldown;

    [Header ("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius;
    [SerializeField] private int attackDamage;
    [SerializeField] private LayerMask playerLayer;

    private Transform player; //Luu transform cua player de khoi phai tim moi frame
    private float lastAttackTime;
    private int moveDirection = 1;
    private float leftLimit;
    private float rightLimit;


    protected override void Awake()
    {
        base.Awake();

        leftLimit = leftPoint.position.x;
        rightLimit = rightPoint.position.x;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    //update chay moi frame voi moi frame goi ham move
    protected override void LogicUpdate()
    {
        if (player == null) return;

        float distance = Mathf.Abs(player.position.x - transform.position.x);// tinh khoang cach tu enemy den player

        //Neu player vao vung detect => duoi player
        if (distance <= detectRange && IsInsidePatrolZone())
        {
            if(distance > attackRange)
            {
                Chase();
            }
            else
            {
                TryAttack();
            }
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        if (hasRoomLimits)
        {
            // Nếu đi lố qua giới hạn trái của phòng -> quay phải
            if (transform.position.x <= roomLeftLimit)
            {
                SetDirection(1);
            }
            // Nếu đi lố qua giới hạn phải của phòng -> quay trái
            else if (transform.position.x >= roomRightLimit)
            {
                SetDirection(-1);
            }
        }

        if (moveDirection == 1 && transform.position.x >= rightLimit)
            SetDirection(-1);
        else if (moveDirection == -1 && transform.position.x <= leftLimit)
            SetDirection(1);

        animator.SetBool("isRunning", true);
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

        // Không cho vượt khỏi patrol zone
        if ((directionToPlayer == 1 && transform.position.x >= rightPoint.position.x) ||
            (directionToPlayer == -1 && transform.position.x <= leftPoint.position.x))
        {
            Patrol();
            return;
        }

        SetDirection(directionToPlayer);

        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        animator.SetBool("isRunning", true);
    }

    private void TryAttack()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("isRunning", false);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");

            // Sau này có thể thêm damage bằng Animation Event
        }
    }

    private void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            playerLayer);

        foreach (Collider2D hit in hits)
        {
            Player playerHealth = hit.GetComponent<Player>();
            if (playerHealth != null)
            {
                playerHealth.TakeDame(attackDamage);
            }
        }
    }

    private bool IsInsidePatrolZone()
    {
        return transform.position.x >= leftLimit &&
               transform.position.x <= rightLimit;
    }

    // Hiển thị vùng attack trong Scene view
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
