using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;

    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.GameOver();
            Destroy(gameObject);
        }

        if(collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
