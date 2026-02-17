using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletNormal;
    [SerializeField] private Transform target;
    [SerializeField] private float fireRate;

    [SerializeField] private float detectRange;
    private float fireTimer;

    private void Update()
    {
        if (GameManager.Instance.IsGameOver || target == null) return; //neu game over, eney ngung hoat dong

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > detectRange)
        {
            return;
        }

        if (distance <= detectRange)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0)
            {
                Shoot();
                fireTimer = fireRate;
            }
        }
    }


    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletNormal, firePoint.position, Quaternion.identity);

        // Tính hướng từ Enemy → Player
        Vector2 direction = (target.position - firePoint.position).normalized;

        // Truyền hướng cho đạn
        bullet.GetComponent<EnemyBullet>().SetDirection(direction);
    }


}
