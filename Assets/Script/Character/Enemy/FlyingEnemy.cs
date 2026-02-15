using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Patrol")] //khu vuc bay tu A->B cua enemy
    [SerializeField] private Transform PointA;
    [SerializeField] private Transform PointB;
    [SerializeField] private float speed;

    [Header("Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float detectRange;
    [SerializeField] private float fireCooldown;

    private Transform targetPoint;
    private float fireTimer;

    private void Start()
    {
        targetPoint = PointB;
    }

    private void Update()
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
                targetPoint.position,
                speed * Time.deltaTime
            );

        //neu distance < 0.1, enemy da toi noi, sau do flip
        if(Vector2.Distance( transform.position, targetPoint.position ) < 0.3f )
        {
            targetPoint = targetPoint == PointA ? PointB : PointA;
            Flip();
        }
    }

    private void DetectAndShoot()
    {
        fireTimer += Time.deltaTime;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance < detectRange && fireTimer >= fireCooldown)
        {
            Shoot(player.transform);
            fireTimer = 0f;
        }
    }

    private void Shoot(Transform player)
    {
        Vector2 direction = (player.position - firePoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<FlyingEnemyBullet>().SetDirection(direction);//tao vien dan tai firePoint
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
