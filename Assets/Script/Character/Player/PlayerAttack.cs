using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Setting")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] float attackRadius;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private int damage;
    [SerializeField] private float knockbackForce;
    private int comboStep = 0;
    private float lastAttackTime;
    [SerializeField] private float comboResetTime;
    [SerializeField] private float attackDuration; // thoi gian khoa player
    private bool isAttacking = false;
    private PlayerEnergy energy;

    private void Awake()
    {
        energy = GetComponent<PlayerEnergy>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            if(!GetComponent<Player>().IsGrounded)
            {
                AirAttack();
            }
            else
            {
                HandleAttack();
            }
        }

        if(Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }
    }

    private void HandleAttack()
    {
        if (isAttacking) return;

        comboStep++;
        lastAttackTime = Time.time;

        if(comboStep > 3)
        {
            comboStep = 1;
        }

        StartCoroutine(AttackCoroutine(comboStep));
    }

    private IEnumerator AttackCoroutine(int step)
    {
        isAttacking = true;
        GetComponent<Player>().isAttacking = true;
        PerformAttack(step);

        yield return new WaitForSeconds(attackDuration);
        GetComponent<Player>().isAttacking = false;
        isAttacking = false;
    }
    private void AirAttack()
    {
        if (isAttacking) return;
        StartCoroutine(AirAttackCoroutine());
    }

    private IEnumerator AirAttackCoroutine()
    {
        isAttacking = true;

        GetComponent<Player>().isAttacking = true;

        float airRadius = attackRadius * 1.1f;

        Collider2D[] enemies = Physics2D.OverlapCircleAll
            (
                attackPoint.position,
                airRadius,
                enemyLayerMask
            );

        foreach(Collider2D enemy in enemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                Vector2 direction = (enemy.transform.position - transform.position).normalized;
                health.TakeDamage(damage, direction, knockbackForce * 0.8f);
                energy.GainEnergy(3);
            }
        }
        yield return new WaitForSeconds(attackDuration * 0.8f);

        GetComponent<Player>().isAttacking = false;
        isAttacking = false;
    }
    private void PerformAttack(int step)
    {
        float currentKnockBack = knockbackForce;
        int currentDame = damage;

        if(step == 3)
        {
            currentKnockBack *= 1.5f;
            currentDame += 1;
        }
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayerMask
        );

        foreach (Collider2D enemy in enemies )
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if( health != null )
            { 
                Vector2 direction = (enemy.transform.position - transform.position).normalized;
                health.TakeDamage(currentDame, direction, currentKnockBack);
                energy.GainEnergy(3);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
