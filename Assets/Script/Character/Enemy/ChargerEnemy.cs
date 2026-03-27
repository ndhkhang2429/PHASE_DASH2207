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
        Stunned,
        Cooldown
    };

    [Header("Patrol")]
    [SerializeField] private Transform PointA;
    [SerializeField] private Transform PointB;

    [Header("Detection")]
    [SerializeField] private float detectRangeX = 6f;
    [SerializeField] private float detectRangeY = 2f;
    [SerializeField] private float chargeCooldown = 2f;

    [Header("Charge")]
    [SerializeField] private float windupTime = 0.8f;
    [SerializeField] private float chargeSpeed = 12f;
    [SerializeField] private float chargeDuration = 1.5f; // Giới hạn tầm xa của cú tông
    [SerializeField] private float stunTimeWall = 2f;
    [SerializeField] private float stunTimePlayer = 1f;

    [Header("Attack")]
    [SerializeField] private int damage;
    [SerializeField] private float knockbackForce;


    private State currentState;
    private Transform player;
    private Vector2 chargeDirection;

    private bool canCharge = true;
    private int moveDirection = 1;

    private float leftLimit;
    private float rightLimit;

    protected override void Awake()
    {
        base.Awake();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentState = State.Patrol;
        leftLimit = Mathf.Min(PointA.position.x, PointB.position.x);
        rightLimit = Mathf.Max(PointA.position.x, PointB.position.x);
        isFacingRight = false;
    }

    protected override void LogicUpdate()
    {
        if (player == null || isDead) return;

        //khoang cach tu enemy den player
        float distanceX = Mathf.Abs(transform.position.x - player.position.x);
        float distanceY = Mathf.Abs(transform.position.y - player.position.y);

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                // Chỉ tông nếu có thể (canCharge) và thấy Player
                if (canCharge && distanceX < detectRangeX && distanceY < detectRangeY)
                {
                    StartCoroutine(WindupRoutine());
                }
                break;

            case State.Charge:
                rb.velocity = new Vector2(chargeDirection.x * chargeSpeed, rb.velocity.y);
                break;

            default:
                // Các trạng thái Windup, Stunned, Cooldown thì đứng yên
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
    }

    private void PatrolLogic()
    {
        animator.SetBool("isRunning", true);

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
        canCharge = false;
        animator.SetBool("isRunning", false);

        float dirToPlayer = Mathf.Sign(player.position.x - transform.position.x);
        SetDirection((int)dirToPlayer);

        float timer = 0f;
        Vector3 originalScale = transform.localScale;

        while(timer < windupTime)
        {
            timer += Time.deltaTime;

            // rung nhẹ để cảnh báo
            float scaleOffset = Mathf.Sin(Time.time * 30f) * 0.05f;
            transform.localScale = originalScale * (1f + scaleOffset);

            yield return null;
        }

        transform.localScale = originalScale;

        chargeDirection = new Vector2(dirToPlayer, 0f);

        StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        currentState = State.Charge;
        animator.SetBool("isRunning", true);

        yield return new WaitForSeconds(chargeDuration);

        if (currentState == State.Charge) // Nếu vẫn đang lao mà chưa va chạm
        {
            StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator StunRoutine(float duration)
    {
        currentState = State.Stunned;
        animator.SetBool("isRunning", false);

        yield return new WaitForSeconds(duration);

        StartCoroutine(CooldownRoutine());
    }
    private IEnumerator CooldownRoutine()
    {
        currentState = State.Cooldown;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(chargeCooldown);
        canCharge = true;
        currentState = State.Patrol;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState != State.Charge) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Player p = collision.gameObject.GetComponent<Player>();
            if (p != null)
            {
                Vector2 knockDir = (collision.transform.position - transform.position).normalized;
                p.TakeDamage(damage, knockDir, knockbackForce);
            }
            StartCoroutine(StunRoutine(stunTimePlayer));
        }
        else if (collision.gameObject.CompareTag("Wall")) // Nhớ đặt tag Wall cho map
        {
            StartCoroutine(StunRoutine(stunTimeWall));
        }
    }
    public bool IsCharging()
    {
        return currentState == State.Charge;
    }
}
