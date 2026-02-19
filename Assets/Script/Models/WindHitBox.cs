using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindHitBox : MonoBehaviour
{
    [SerializeField] private int dame;
    [SerializeField] private float knockBack;
    public LayerMask enemyLayerMask;

    private Player player;

    private void Start()
    {
        player = GetComponentInParent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(((1 << collision.gameObject.layer) & enemyLayerMask) != 0)
        {
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
            if(enemy != null)
            {
                Vector2 dir = new Vector2(player.facingDirection, 0);
                enemy.TakeDamage(dame, dir, knockBack);
            }
        }
    }
}
