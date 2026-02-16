using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class ExploderEnemy : MonoBehaviour
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
    [SerializeField] private float patrolSpeed;

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
    private Rigidbody2D rb;

    private bool isExploding = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = State.Patrol;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        targetPoint = PointB;
    }

    private void Update()
    {
        float distance = Mathf.Infinity;
        if(player != null)
        {
            distance = Vector2.Distance(transform.position, player.position);
        }

        switch(currentState)
        {
            case State.Patrol:
                Patrol();

                if (player != null && distance < detectRange)
                    currentState = State.Chase;
                break;

            case State.Chase:
                if (player == null || distance > detectRange)
                {
                    currentState = State.Patrol;
                    break;
                }

                Chase();

                if (distance < explodeRange && !isExploding)
                    StartCoroutine(ExplodeRoutine());

                break;

            case State.ExplodeWindup:
                // Đang chờ nổ – không làm gì thêm
                break;
        }
    }

    //Patrol
    private void Patrol()
    {
        //transform.position = Vector2.MoveTowards
        //(
        //    transform.position,
        //    targetPoint.position,
        //    patrolSpeed * Time.deltaTime
        //);

        float direction = Mathf.Sign(targetPoint.position.x - transform.position.x);
        rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);

        if (Mathf.Abs(transform.position.x - targetPoint.position.x) < 0.1f)
        {
            targetPoint = targetPoint == PointA ? PointB : PointA;
            Flip();
        }
    }

    //Chase
    private void Chase()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        transform.position += (Vector3)direction * chaseSpeed * Time.deltaTime;

        if(direction.x > 0)
        {
            transform.localScale = Vector3.one;
        }
        else if(direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    //Explode Routine
    private IEnumerator ExplodeRoutine()
    {
        isExploding = true;
        currentState = State.ExplodeWindup;

        yield return new WaitForSeconds(windupTime);
        Explode();
    }

    //Explode
    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach(Collider2D hit in hits)
        {
            if(hit.CompareTag("Player"))
            {
                GameManager.Instance.GameOver();
            }
        }
        Destroy(gameObject);
    }

    // ================= FLIP =================
    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
