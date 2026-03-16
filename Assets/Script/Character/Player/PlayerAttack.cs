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
    [SerializeField] private Transform airAttackPoint;
    [SerializeField] Vector2 airSize;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private int airDamage;

    [Header("Combo Data")]
    [SerializeField] private Vector2 attack1Size;
    [SerializeField] private Vector2 attack2Size;

    [SerializeField] private int attack1Damage;
    [SerializeField] private int attack2Damage;
    [SerializeField] private GameObject windPrefab;
    [SerializeField] private Transform attack3firePoint;

    [SerializeField] private float knockbackForce;
    [SerializeField] private float comboResetTime;

    private int comboStep = 0;
    private float lastAttackTime;

    private bool isAttacking = false;
    private bool comboInputBuffered = false;
    private bool canNextCombo = false;


    private void Awake()
    {
        energy = GetComponent<PlayerEnergy>();
        animator = GetComponentInChildren<Animator>();
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
                RegisterAttackInput();
            }
        }

        // reset combo nếu quá lâu không attack
        if (Time.time - lastAttackTime > comboResetTime)
        {
            ResetCombo();
        }
    }

    private void RegisterAttackInput()
    {
        if(!isAttacking)
        {
            StartAttack();
            return;
        }
        else
        {
            comboInputBuffered = true; //ghi nho input
        }
    }

    private void StartAttack()
    {
        comboStep++;

        if (comboStep > 3)
        {
            comboStep = 1;
        }

        lastAttackTime = Time.time;
        isAttacking = true;

        player.isAttacking = true;

        animator.SetBool("isAttacking", true);
        player.canFlip = false;

        animator.SetInteger("ComboStep", comboStep);
        animator.SetTrigger("Attack");

    }

    //Goi bang animation event o giua attack
    public void EnableComboWindow()
    {
        canNextCombo = true;

        if(comboInputBuffered)
        {
            comboInputBuffered = false;
            canNextCombo = false;
            StartAttack();
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        player.isAttacking = false;
        player.canFlip = true;

        comboInputBuffered = false;
        canNextCombo = false;
        animator.SetBool("isAttacking", false);
    }

    public void CancelAttack()
    {
        isAttacking = false;
        comboInputBuffered = false;
        canNextCombo = false;

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            isAttacking = false;
        }
        player.canFlip = true;

        ResetCombo();
    }

    private void ResetCombo()
    {
        comboStep = 0;
        animator.SetInteger("ComboStep", 0);
    }

    //Air
    private void AirAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        player.isAttacking = true;
        player.canFlip = false;

        //player.rb.velocity = new Vector2(0, player.rb.velocity.y);

        animator.SetTrigger("AirAttack");
    }

    private void DealAirDamage()
    {
        float airKnockBack = knockbackForce * 0.8f;

        Collider2D[] enemies = Physics2D.OverlapBoxAll
        (
                airAttackPoint.position,
                airSize,
                0f,
                enemyLayerMask
        );

        foreach (Collider2D c in enemies)
        {
            ShieldEnemy shield = c.GetComponent<ShieldEnemy>();

            Vector2 direction = (c.transform.position - transform.position).normalized;

            bool didDamage = false;
            if (shield != null)
            {
                didDamage = shield.TryTakeDamage(airDamage, transform, knockbackForce * 0.8f);
            }
            else
            {
                EnemyHealth health = c.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(airDamage, direction, knockbackForce * 0.8f);
                    didDamage = true;
                }
            }

            if( didDamage )
            {
                energy.GainEnergy(1);
                HitStopManager.Instance.Stop(0.04f);
            }
        }
    }

    private void DealDamage()
    {
        Vector2 size = Vector2.zero;
        int damage = 0;

        switch (comboStep)
        {
            case 1:
                size = attack1Size;
                damage = attack1Damage;
                break;

            case 2:
                size = attack2Size;
                damage = attack2Damage;
                break;

            case 3:
                return;
        }

        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            attackPoint.position,
            size,
            0f,
            enemyLayerMask
        );

        foreach (Collider2D enemy in enemies )
        {
            ShieldEnemy shield = enemy.GetComponent<ShieldEnemy>();

            Vector2 direction = (enemy.transform.position - transform.position).normalized;
            bool didDamage = false;
            if (shield != null)
            {
                didDamage = shield.TryTakeDamage(damage, transform, knockbackForce);
            }
            else
            {
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage, direction, knockbackForce);
                    didDamage = true;
                }
            }

            if (didDamage)
            {
                energy.GainEnergy(3);
                HitStopManager.Instance.Stop(0.05f);
            }
        }
    }

    public void SpawnWind()
    {
        GameObject wind = Instantiate(windPrefab, attack3firePoint.position, Quaternion.identity);

        WindProjectile projectile = wind.GetComponent<WindProjectile>();

        if (projectile != null)
        {
            projectile.SetDirection(player.facingDirection);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        if (airAttackPoint == null) return;
        Gizmos.color = Color.red;

        Vector2 size = attack1Size;

        if (comboStep == 2) size = attack2Size;

        Gizmos.DrawWireCube(attackPoint.position, size);
        Gizmos.DrawWireCube(airAttackPoint.position, airSize);
    }
}
