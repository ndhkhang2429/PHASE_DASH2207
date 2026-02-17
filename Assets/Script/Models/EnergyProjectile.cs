using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyProjectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private int dame;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float lifeTime;

    private float direction;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    public void SetDirection(float dir)
    {
        direction = dir;
    }

    private void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHealth health = collision.GetComponent<EnemyHealth>();

        if (health != null)
        {
            Vector2 knockDir = (collision.transform.position - transform.position).normalized;
            health.TakeDamage(dame, knockDir, knockbackForce);
            Destroy(gameObject);
        }
    }
}
