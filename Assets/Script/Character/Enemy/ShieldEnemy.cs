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

    [Header("Combat")]
    [SerializeField] private float detectRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooldown;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackSize;
    [SerializeField] private int attackDamage;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackKnockbackForce;

    private State currentState;
    private Transform player;
    private Transform targetPoint;

    private float leftLimit;
    private float rightLimit;
    private int moveDirection = 1;

    private bool isShielding = true;
    private float attackTimer;
    private bool isAttacking;

    protected override void Awake()
    {
        base.Awake();

        leftLimit = PointA.position.x;
        rightLimit = PointB.position.x;

        currentState = State.Patrol;
        targetPoint = PointB;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        health = GetComponent<EnemyHealth>();
    }

    protected override void LogicUpdate()
    {
        if (player == null) return;

        float distance = Mathf.Infinity;
        if (player != null)
            distance = Vector2.Distance(transform.position, player.position);

        attackTimer += Time.deltaTime;

        switch (currentState)
        {
            case State.Patrol:
                Patrol();

                if(distance < detectRange)
                {
                    currentState = State.Shield;
                }
                break;

            case State.Shield:
                ShieldState(distance);
                break;

            case State.Attack:
                AttackState();
                break;
        }
    }

    //Patrol
    private void Patrol()
    {
        isShielding = false;
        animator.SetBool("isRunning", true);
        animator.SetBool("isShield", false);

        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        if (moveDirection == 1 && transform.position.x >= rightLimit)
            SetDirection(-1);
        else if (moveDirection == -1 && transform.position.x <= leftLimit)
            SetDirection(1);

        
    }

    //Shield
    private void ShieldState(float distance)
    {
        rb.velocity = Vector2.zero;

        animator.SetBool("isRunning", false);
        animator.SetBool("isShield", true);

        isShielding = true;

        FacePlayer();

        if (distance < attackRange && attackTimer >= attackCooldown)
        { 
            isShielding = false;
            animator.SetBool("isShield", false);
            animator.SetTrigger("Attack");
            isAttacking = true;
            currentState = State.Attack;
        }

        if (distance > detectRange)
        {
            currentState = State.Patrol;
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

    //Attack
    private void AttackState()
    {
        if (isAttacking) return;

        isAttacking = true;
        isShielding = false;

        animator.SetBool("isShield", false);
        animator.SetTrigger("Attack");

        rb.velocity = Vector2.zero;
    }

    private void DealDamage()
    {
        Collider2D[] players = Physics2D.OverlapBoxAll(
        attackPoint.position,
        attackSize,
        0f,
        playerLayer
        );

        foreach (Collider2D p in players)
        {
            Player health = p.GetComponent<Player>();
            if (health != null)
            {
                Vector2 knockDir = (p.transform.position - transform.position).normalized;
                health.TakeDamage(attackDamage, knockDir, attackKnockbackForce);
            }
        }
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        attackTimer = 0;
        currentState = State.Shield;
    }

    //Face player
    private void FacePlayer()
    {
        if (player.position.x > transform.position.x && !isFacingRight)
            Flip();
        else if (player.position.x < transform.position.x && isFacingRight)
            Flip();
    }

    public bool TryTakeDamage(int damage, Transform attacker, float knockbackForce)
    {
        if (health == null) return false;

        Vector2 direction = attacker.position - transform.position ;

        bool attackerInFront =
            (isFacingRight && direction.x > 0) ||
            (!isFacingRight && direction.x < 0);

        if (isShielding && attackerInFront)
        {
            Debug.Log("Blocked by Shield");
            return false;
        }

        Vector2 knockbackDir = (transform.position - attacker.position).normalized;

        health.TakeDamage(damage, knockbackDir, knockbackForce);

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(attackPoint.position, attackSize);
    }
}
