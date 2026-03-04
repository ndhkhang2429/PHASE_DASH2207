using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class ExploderEnemy : EnemyBase
{
    private enum State
    {
        Patrol, //di qua lai giua 2 diem
        Chase, //duoi theo player
        ExplodeWindup,//chuan bi no
        Explode//phat no
    };

    [Header("Patrol")]
    [SerializeField] private Transform PointA;
    [SerializeField] private Transform PointB;

    [Header("Detection")]
    [SerializeField] private float detectRange; //khoang cach bat dau duoi
    [SerializeField] private float explodeRange;//khoangcach kich hoat no

    [Header("Explosion")]
    [SerializeField] private float windupTime;//thoi gian cho truoc khi no
    [SerializeField] private float explosionRadius;//ban kinh gay dame
    [SerializeField] private int damage;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed;

    private State currentState;
    private Transform player;
    private Transform targetPoint;

    private int moveDirection = 1;
    private float leftLimit;
    private float rightLimit;

    private bool isExploding = false;
    private Vector3 originalScale;

    protected override void Awake()
    {
        base.Awake();
        leftLimit = PointA.position.x;
        rightLimit = PointB.position.x;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        currentState = State.Patrol;
        targetPoint = PointB;
        originalScale = transform.localScale;
    }

    protected override void LogicUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();

                if (distance < detectRange)
                    currentState = State.Chase;
                break;

            case State.Chase:
                Chase();

                if (distance < explodeRange && !isExploding)
                {
                    isExploding = true;

                    StartCoroutine(ExplodeRoutine());
                }
                break;

            case State.ExplodeWindup:
                break;

            case State.Explode:
                break;
        }
    }

    //Patrol
    private void Patrol()
    {
        animator.SetBool("isRunning", true);
        
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        if (moveDirection == 1 && transform.position.x >= rightLimit)
            SetDirection(-1);
        else if (moveDirection == -1 && transform.position.x <= leftLimit)
            SetDirection(1);

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

    //Chase
    private void Chase()
    {
        animator.SetBool("isRunning", true);
        int directionToPlayer = player.position.x > transform.position.x ? 1 : -1;

        // Không cho vượt khỏi patrol zone
        if ((directionToPlayer == 1 && transform.position.x >= PointA.position.x) ||
            (directionToPlayer == -1 && transform.position.x <= PointB.position.x))
        {
            rb.velocity = new Vector2(moveDirection * chaseSpeed, rb.velocity.y);

            if (moveDirection == 1 && transform.position.x >= rightLimit)
                SetDirection(-1);
            else if (moveDirection == -1 && transform.position.x <= leftLimit)
                SetDirection(1);
            return;
        }

        SetDirection(directionToPlayer);

        rb.velocity = new Vector2(moveDirection * chaseSpeed, rb.velocity.y);
        
    }

    //Explode Routine
    private IEnumerator ExplodeRoutine()
    {
        currentState = State.ExplodeWindup;
        animator.SetBool("isRunning", false);

        rb.velocity = Vector2.zero;

        float timer = 0f;

        Color originalColor = spriteRenderer.color;
        Vector3 startScale = originalScale;
        Vector3 maxScale = originalScale * 1.2f;

        //Windup phase: nhấp nháy + phình to
        while (timer < windupTime)
        {
            timer += Time.deltaTime;

            float t = timer / windupTime;

            //scale phinh dan
            transform.localScale = Vector3.Lerp(startScale, maxScale, t);

            //nhap nhay
            float blink = Mathf.PingPong(Time.time * 8f, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, Color.red, blink);

            yield return null;
        }

        // Reset scale + color trước khi nổ
        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;

        currentState = State.Explode;
        animator.SetTrigger("Explode");
        Explode();

        yield return new WaitForSeconds(0.5f);

        Destroy(gameObject);
    }

    //Explode
    public void Explode()
    {
       
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, LayerMask.GetMask("Player"));

        foreach(Collider2D hit in hits)
        {
            Player player = hit.GetComponent<Player>();
            if (player != null)
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                player.TakeDamage(damage, dir, 5f);
            }
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explodeRange);
    }
}
