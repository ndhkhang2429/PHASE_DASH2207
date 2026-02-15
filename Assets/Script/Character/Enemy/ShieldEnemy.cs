using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldEnemy : MonoBehaviour
{
    private enum State
    {
        Patrol,
        Chase,
        Attack,
        Turn,
    }

    [Header("Patrol")]
    [SerializeField] private Transform PointA;
    [SerializeField] private Transform PointB;
    [SerializeField] private float patrolSpeed;

    [Header("Combat")]
    [SerializeField] private float detectRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float chaseSpeed;

    [Header("Turn")]
    [SerializeField] private float turnDelay;

    private State currentState;
    private Transform player;
    private Transform targetPoint;

    private bool isFacingRight = true;
    private bool isTurning = false;

    private void Start()
    {
        currentState = State.Patrol;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        targetPoint = PointB;

    }

    private void Update()
    {
        float distance = Mathf.Infinity;
        if (player != null)
            distance = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();

                if(player != null && distance < detectRange)
                {
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                if (player == null || distance > detectRange)
                {
                    currentState = State.Patrol;
                    break;
                }

                Chase();

                if(distance < attackRange)
                {
                    currentState = State.Attack;
                }

                if(IsPlayerBehind() && !isTurning)
                {
                    StartCoroutine(TurnRoutine());
                }
                break;

            case State.Attack:
                if (player == null || distance > detectRange)
                {
                    currentState = State.Patrol;
                    break;
                }

                if (distance > attackRange)
                    currentState = State.Chase;

                Attack();

                if (IsPlayerBehind() && !isTurning)
                    StartCoroutine(TurnRoutine());

                break;
        }
    }

    //Patrol
    private void Patrol()
    {
        transform.position = Vector2.MoveTowards
            (
                transform.position,
                targetPoint.position,
                patrolSpeed * Time.deltaTime
            );

        if(Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            targetPoint = targetPoint == PointA ? PointB : PointA;
            FlipImmediate();
        }
    }

    //Chase
    private void Chase()
    {
        float direction = isFacingRight ? 1 : -1;
        transform.Translate(Vector2.right * direction * chaseSpeed * Time.deltaTime);
    }

    //Attack
    private void Attack()
    {
        Debug.Log("ShieldEnemy Attack");
    }

    //Turn with delay
    private IEnumerator TurnRoutine()
    {
        isTurning = true;
        currentState = State.Turn;

        yield return new WaitForSeconds(turnDelay);

        FlipImmediate();

        currentState = State.Chase;
        isTurning = false;
    }

    private bool IsPlayerBehind()
    {
        if (isFacingRight && player.position.x < transform.position.x)
            return true;

        if (!isFacingRight && player.position.x > transform.position.x)
            return true;

        return false;
    }

    private void FlipImmediate()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
