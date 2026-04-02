using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindProjectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private int damage;
    [SerializeField] private float lifeTime;
    [SerializeField] private float knockBack;
    [SerializeField] private LayerMask enemyLayer;

    private int direction;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    public void SetDirection(int facing)
    {
        direction = facing;

        if (direction < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
            ShieldEnemy shieldEnemy = collision.GetComponent<ShieldEnemy>();
            Vector2 dir = new Vector2(direction * 0.3f, 1f).normalized;
            if (shieldEnemy != null)
            {
                // Truyền transform.parent hoặc chính projectile này để tính hướng chặn
                // Nếu chặn thành công, hàm này sẽ tự xử lý giảm dame hoặc triệt tiêu
                shieldEnemy.TryTakeDamage(damage, transform, knockBack);
            }
            else if (enemy != null)
            {
                // Nếu không phải quái khiên thì trừ máu như cũ
                enemy.TakeDamage(damage, dir, knockBack);
            }

            Destroy(gameObject);
        }
    }
}
