using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    [Header("Patrol")] //khu vuc bay tu A->B cua enemy
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    [Header("Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float detectRange;
    [SerializeField] private float fireCooldown;

    private Vector3 pointAPosition;
    private Vector3 pointBPosition;
    private Vector3 targetPoint;

    private float fireTimer;
    private Transform player;

    protected override void Awake()
    {
        base.Awake();
        isFacingRight = false;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // LƯU WORLD POSITION
        pointAPosition = leftPoint.position;
        pointBPosition = rightPoint.position;

        targetPoint = pointAPosition;
    }

    protected override void LogicUpdate()
    {
        Patrol();
        DetectAndShoot();
    }

    private void Patrol()
    {
        //di chuyen tu vi tri hien tai den target point
        transform.position = Vector2.MoveTowards
            (
                transform.position,
                targetPoint,
                moveSpeed * Time.deltaTime
            );

        if (Vector2.Distance(transform.position, targetPoint) < 0.3f)
        {
            targetPoint = targetPoint == pointAPosition ? pointBPosition : pointAPosition;
            Flip();
        }
        animator.SetBool("isFlying", true);
    }

    private void DetectAndShoot()
    {
        if (player == null) return;

        fireTimer += Time.deltaTime;
     
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance < detectRange && fireTimer >= fireCooldown)
        {
            Shoot(player.transform);
            fireTimer = 0f;
        }
    }

    private void Shoot(Transform player)
    {
        if (player.position.x > transform.position.x && !isFacingRight)
            Flip();
        else if (player.position.x < transform.position.x && isFacingRight)
            Flip();

        Vector2 direction = (player.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<FlyingEnemyBullet>().SetDirection(direction);//tao vien dan tai firePoint
        animator.SetTrigger("Shoot");
    }
}
