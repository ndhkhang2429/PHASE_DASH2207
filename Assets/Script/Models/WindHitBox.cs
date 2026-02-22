using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindHitBox : MonoBehaviour
{
    [SerializeField] private int dame;
    [SerializeField] private float knockBack;
    [SerializeField] private LayerMask enemyLayerMask;

    private Player player;
    private HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void OnEnable()
    {
        hitEnemies.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayerMask) == 0)
            return;

        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();

        if (enemy == null) return;

        if (hitEnemies.Contains(enemy)) return;

        Vector2 dir = new Vector2(player.facingDirection, 0);

        enemy.TakeDamage(dame, dir, knockBack);

        hitEnemies.Add(enemy);
    }
}
