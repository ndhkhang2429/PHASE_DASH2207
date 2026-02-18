using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private Player player;
    private PlayerEnergy energy;

    [Header("Attack Setting")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] Vector2 attackSize;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private int damage;
    [SerializeField] private float knockbackForce;
    [SerializeField] private float comboResetTime;
    [SerializeField] private float[] attackDurations; // thoi gian khoa player

    private int comboStep = 0;
    private float lastAttackTime;
    private bool isAttacking = false;
    

    private void Awake()
    {
        energy = GetComponent<PlayerEnergy>();
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            if(!player.IsGrounded)
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
            animator.SetInteger("ComboStep", 0);
        }
    }

    private void HandleAttack()
    {
        if (isAttacking) return;

        comboStep++;
        if (comboStep > 3)
        {
            comboStep = 1;
        }

        lastAttackTime = Time.time;
        isAttacking = true;

        player.isAttacking = true;
        player.canFlip = false;

        animator.SetInteger("ComboStep", comboStep);
        animator.SetTrigger("Attack");
    }

    public void EndAttack()
    {
        isAttacking = false;
        player.isAttacking = false;
        player.canFlip = true;
    }

    private void AirAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        player.isAttacking = true;
        player.canFlip = false;

        animator.SetTrigger("AirAttack");
    }

    private void DealDamage()
    {
        Vector2 currentSize = attackSize;
        float currentKnockBack = knockbackForce;
        int currentDame = damage;

        if (comboStep == 2)
            currentSize *= 1.15f;

        if (comboStep == 3)
        {
            currentSize *= 1.3f;
            currentKnockBack *= 1.5f;
            currentDame += 1;
        }
        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            attackPoint.position,
            currentSize,
            0f,
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
        Gizmos.DrawWireCube(attackPoint.position, attackSize);
    }
}
