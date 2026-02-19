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
    [SerializeField] private Vector2 attack3Size;

    [SerializeField] private int attack1Damage;
    [SerializeField] private int attack2Damage;
    [SerializeField] private int attack3Damage;

    [SerializeField] private GameObject windHitBox;
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
                RegisterAttackInput();
            }
        }

        if(Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
            animator.SetInteger("ComboStep", 0);
        }
    }

    private void RegisterAttackInput()
    {
        if(!isAttacking)
        {
            StartAttack();
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
        canNextCombo = false;
    }

    //Air
    private void AirAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        player.isAttacking = true;
        player.canFlip = false;

        player.rb.velocity = new Vector2(0, player.rb.velocity.y);

        animator.SetTrigger("AirAttack");
    }
    //Bat tat bang animation event
    public void EnableWhirlwind()
    {
        windHitBox.SetActive(true);
    }

    public void DisableWhirlwind()
    {
        windHitBox.SetActive(false);
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
            EnemyHealth health = c.GetComponent<EnemyHealth>();
            if(health != null)
            {
                Vector2 direction = (c.transform.position - transform.position).normalized;
                health.TakeDamage(airDamage, direction, airKnockBack);
                energy.GainEnergy(2);
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
                size = attack3Size;
                damage = attack3Damage;
                break;
        }

        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            attackPoint.position,
            size,
            0f,
            enemyLayerMask
        );

        foreach (Collider2D enemy in enemies )
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if( health != null )
            { 
                Vector2 direction = (enemy.transform.position - transform.position).normalized;
                health.TakeDamage(damage, direction, knockbackForce);
                energy.GainEnergy(3);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        if (airAttackPoint == null) return;
        Gizmos.color = Color.red;

        Vector2 size = attack1Size;

        if (comboStep == 2) size = attack2Size;
        if (comboStep == 3) size = attack3Size;

        Gizmos.DrawWireCube(attackPoint.position, size);
        Gizmos.DrawWireCube(airAttackPoint.position, airSize);
    }
}
