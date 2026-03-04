using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargerEnemy : EnemyBase
{
    private enum State
    {
        Patrol,
        Windup,
        Charge,
        Stunned
    };

    [Header("Patrol")]
    [SerializeField] private Transform PointA;
    [SerializeField] private Transform PointB;

    [Header("Detection")]
    [SerializeField] private float detectRange;

    [Header("Charge")]
    [SerializeField] private float windupTime;
    [SerializeField] private float chargeSpeed;
    [SerializeField] private float chargeDuration;
    [SerializeField] private float stunTimeWall;
    [SerializeField] private float stunTimePlayer;

    [Header("Attack")]
    [SerializeField] private int damage;
    [SerializeField] private float knockbackForce;


    private State currentState;
    private Transform player;
    private Vector2 chargeDirection;

    private bool isCharging = false;
    private int moveDirection = 1;

    private float leftLimit;
    private float rightLimit;

    protected override void Awake()
    {
        base.Awake();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        currentState = State.Patrol;
        leftLimit = PointA.position.x;
        rightLimit = PointB.position.x;
        isFacingRight = false;
    }

    protected override void LogicUpdate()
    {
        if (player == null) return;

        //khoang cach tu enemy den player
        float distance = Vector2.Distance(transform.position, player.position);

        switch(currentState)
        {
            case State.Patrol:
                Patrol();

                if(distance < detectRange)
                {
                    StartCoroutine(WindupRoutine());
                }
                break;

            case State.Charge:
                rb.velocity = new Vector2(chargeDirection.x * chargeSpeed, rb.velocity.y);
                break;

            case State.Stunned:
                rb.velocity = Vector2.zero;
                break;
        }
    }

    private void Patrol()
    {
        animator.SetBool("isRunning", true);

        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        if (moveDirection == 1 && transform.position.x >= rightLimit)
            SetDirection(-1);
        else if (moveDirection == -1 && transform.position.x <= leftLimit)
            SetDirection(1);
    }

    private void SetDirection(int dir)
    {
        moveDirection = dir;

        if ((moveDirection == 1 && !isFacingRight) ||
            (moveDirection == -1 && isFacingRight))
        {
            Flip();
        }
    }

    //chuan bi dash, dung di chuyen, lock huong de dash
    private IEnumerator WindupRoutine()
    {
        currentState = State.Windup;
        animator.SetBool("isRunning", false);
        rb.velocity = Vector2.zero;

        float timer = 0f;
        Vector3 originalScale = transform.localScale;

        while(timer < windupTime)
        {
            timer += Time.deltaTime;

            // rung nhẹ để cảnh báo
            float scaleOffset = Mathf.Sin(Time.time * 25f) * 0.05f;
            transform.localScale = originalScale * (1f + scaleOffset);

            yield return null;
        }

        transform.localScale = originalScale;

        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        chargeDirection = new Vector2(dirX, 0f);

        StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        currentState = State.Charge;
        animator.SetBool("isRunning", true);
        isCharging = true;

        float timer = 0f;

        while (timer < chargeDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(StunRoutine(stunTimeWall));
    }

    private IEnumerator StunRoutine(float duration)
    {
        currentState = State.Stunned;
        isCharging = false;

        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        currentState = State.Patrol;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isCharging) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Gây damage
            collision.gameObject
                .GetComponent<Player>()
                ?.TakeDame(damage);

            // Knockback player
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockDir =
                    (collision.transform.position - transform.position).normalized;

                playerRb.AddForce(knockDir * knockbackForce,
                    ForceMode2D.Impulse);
            }

            StartCoroutine(StunRoutine(stunTimePlayer));
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            StartCoroutine(StunRoutine(stunTimeWall));
        }
    }
    public bool IsCharging()
    {
        return currentState == State.Charge;
    }
}
