using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargerEnemy : MonoBehaviour
{
    private enum State
    {
        Patrol,
        Windup,
        Dash,
        CoolDown
    };

    [Header("Patrol")]
    [SerializeField] private Transform PointA;
    [SerializeField] private Transform PointB;
    [SerializeField] private float patrolSpeed;

    [Header("Detection")]
    [SerializeField] private float detectRange;

    [Header("Dash")]
    [SerializeField] private float windupTime;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashCoolDown;

    private State currentState;
    private Rigidbody2D rb;//tránh lún xuống đất
    private Transform player;
    private Transform targetPoint;

    private Vector2 dashDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        targetPoint = PointB;
        currentState = State.Patrol;
    }

    private void Update()
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
        }
    }

    private void Patrol()
    {
        float direction = Mathf.Sign(targetPoint.position.x - transform.position.x);
        rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);

        if (Mathf.Abs(transform.position.x - targetPoint.position.x) < 0.1f)
        {
            targetPoint = targetPoint == PointA ? PointB : PointA;
            Flip();
        }
    }

    //chuan bi dash, dung di chuyen, lock huong de dash
    private IEnumerator WindupRoutine()
    {
        currentState = State.Windup;
        rb.velocity = Vector2.zero;

        dashDirection = (player.position - transform.position).normalized;

        transform.localScale *= 1.2f;

        yield return new WaitForSeconds(windupTime);

        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        currentState = State.Dash;

        float timer = 0f;

        while (timer < dashDuration)
        {
            rb.velocity = dashDirection * dashSpeed;
            timer += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        currentState = State.CoolDown;
        rb.velocity = Vector2.zero;

        // Trả scale về bình thường
        transform.localScale = new Vector3(
            Mathf.Sign(transform.localScale.x),
            1,
            1
        );

        yield return new WaitForSeconds(dashCoolDown);
        currentState = State.Patrol;
    }
    
    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
