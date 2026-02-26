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
            if (enemy != null)
            {
                Vector2 dir = new Vector2(direction * 0.3f, 1f).normalized;
                enemy.TakeDamage(damage, dir, knockBack);
            }

            Destroy(gameObject);
        }
    }
}
