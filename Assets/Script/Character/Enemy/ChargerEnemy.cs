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
    }

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
    [SerializeField] private float chargeDuration = 1.5f;
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

    // Biến lưu giới hạn gốc
    private float defaultLeftLimit;
    private float defaultRightLimit;

    protected override void Awake()
    {
        base.Awake();

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        currentState = State.Patrol;
        isFacingRight = false;

        // CHỐT GIỚI HẠN AN TOÀN
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
    }

    // --- LẤY GIỚI HẠN HIỆN TẠI ---
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

        float distanceX = Mathf.Abs(transform.position.x - player.position.x);
        float distanceY = Mathf.Abs(transform.position.y - player.position.y);

        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                // CHÚ Ý: Player phải ở trong giới hạn đi tuần thì Charger mới lao tới (Tránh lao đâm đầu vào tường phòng)
                float pX = player.position.x;
                bool isPlayerInLimits = (pX >= GetCurrentLeftLimit() && pX <= GetCurrentRightLimit());

                if (canCharge && distanceX < detectRangeX && distanceY < detectRangeY && isPlayerInLimits)
                {
                    StartCoroutine(WindupRoutine());
                }
                break;

            case State.Charge:
                // SỬA: Kiểm tra đụng tường khi đang lao
                ChargeLogic();
                break;

            default:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
    }

    private void PatrolLogic()
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

    // Xử lý khi đang lao
    private void ChargeLogic()
    {
        rb.velocity = new Vector2(chargeDirection.x * chargeSpeed, rb.velocity.y);

        float currentLeft = GetCurrentLeftLimit();
        float currentRight = GetCurrentRightLimit();

        // TỰ ĐỘNG STUN NẾU LAO TRÚNG GIỚI HẠN (Không cần phụ thuộc Tag Wall)
        if ((chargeDirection.x == 1 && transform.position.x >= currentRight) ||
            (chargeDirection.x == -1 && transform.position.x <= currentLeft))
        {
            StopCoroutine(nameof(ChargeRoutine)); // Dừng việc lao
            StartCoroutine(StunRoutine(stunTimeWall));
        }
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

    private IEnumerator WindupRoutine()
    {
        currentState = State.Windup;
        canCharge = false;
        animator.SetBool("isRunning", false);

        if (enemyAudio != null) enemyAudio.PlayWindup();

        float dirToPlayer = Mathf.Sign(player.position.x - transform.position.x);
        SetDirection((int)dirToPlayer);

        float timer = 0f;
        Vector3 originalScale = transform.localScale;

        while (timer < windupTime)
        {
            timer += Time.deltaTime;
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

        if (enemyAudio != null) enemyAudio.PlayAttack();

        yield return new WaitForSeconds(chargeDuration);

        if (currentState == State.Charge)
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
            StopCoroutine(nameof(ChargeRoutine));
            StartCoroutine(StunRoutine(stunTimePlayer));
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            // Lấy hướng phản lực của điểm va chạm đầu tiên
            Vector2 contactNormal = collision.GetContact(0).normal;

            // Nếu mặt phẳng va chạm hướng sang trái hoặc phải (Trục X lớn hơn trục Y), tông vào tường
            if (Mathf.Abs(contactNormal.x) > 0.5f)
            {
                StopCoroutine(nameof(ChargeRoutine));
                StartCoroutine(StunRoutine(stunTimeWall));
                Debug.Log("Tông trúng tường rồi! Choáng!");
            }
            // Ngược lại, nếu tông vào mặt phẳng hướng lên trên (mặt đất) thì chạy tiếp.
        }
    }

    public bool IsCharging()
    {
        return currentState == State.Charge;
    }

    public override void OnDeath()
    {
        StopAllCoroutines();
        base.OnDeath();
    }
}