using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private Player player;
    private PlayerEnergy energy;
    private Rigidbody2D rb;

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

    [Header("Ulti Setting")]
    [SerializeField] private int ultiEnergyCost = 50;
    [SerializeField] private int ultiDamage = 20;
    [SerializeField] private Vector2 ultiSize = new Vector2(4f, 4f);
    [SerializeField] private Transform ultiCenter;

    [Header("Feel & Physics")]
    [SerializeField] private float knockbackForce;
    [SerializeField] private float comboResetTime = 1f;
    [SerializeField] private float recoilForce = 4f;

    private int comboStep = 0;
    private float lastAttackTime;

    private bool isAttacking = false;
    private bool comboInputBuffered = false;


    private void Awake()
    {
        energy = GetComponent<PlayerEnergy>();
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            if (player.isUsingUlti)
            {
                return;
            }
            if (!player.IsGrounded)
            {
                AirAttack();
            }
            else
            {
                RegisterAttackInput();
            }
        }

        if (isAttacking)
        {
            if (player.isUsingUlti == false)
            {
                if (Time.time - lastAttackTime > 0.5f)
                {
                    EndAttack();
                }
            }
        }

        // reset combo nếu quá lâu không attack
        if (Time.time - lastAttackTime > comboResetTime)
        {
            ResetCombo();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            TryUseUlti();
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
        rb.velocity = new Vector2(0, rb.velocity.y);

        comboStep++;

        if (comboStep > 3)
        {
            comboStep = 1;
        }

        lastAttackTime = Time.time;
        isAttacking = true;

        player.isAttacking = true;
        player.canFlip = false;

        animator.SetBool("isAttacking", true);
        animator.SetInteger("ComboStep", comboStep);
        animator.Play("Attack" + comboStep, 0, 0f);
        if (AudioController.Instance != null && AudioController.Instance.comboSounds.Length >= comboStep)
        {
            AudioClip currentHitSound = AudioController.Instance.comboSounds[comboStep - 1];
            if (currentHitSound != null)
            {
                AudioController.Instance.PlaySFX(currentHitSound, Random.Range(0.9f, 1.1f));
            }
        }
    }

    //Goi bang animation event o giua attack
    public void EnableComboWindow()
    {
        if(comboInputBuffered)
        {
            comboInputBuffered = false;
            StartAttack();
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        player.isAttacking = false;
        player.canFlip = true;

        comboInputBuffered = false;
        animator.SetBool("isAttacking", false);
    }

    public void CancelAttack()
    {
        isAttacking = false;
        comboInputBuffered = false;

        player.isAttacking = false;

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

        rb.velocity = new Vector2(rb.velocity.x, 0);

        isAttacking = true;
        player.isAttacking = true;
        player.canFlip = false;

        lastAttackTime = Time.time;

        animator.SetBool("isAttacking", true);

        animator.Play("Air_Attack", 0, 0f);
        if (AudioController.Instance != null)
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.attackAirSound, Random.Range(0.9f, 1.1f));
        }
    }

    private void TryUseUlti()
    {
        if (isAttacking) return;

        if (energy.UseEnergy(ultiEnergyCost))
        {
            if (energy.UseEnergy(ultiEnergyCost))
            {
                player.isUsingUlti = true;
                isAttacking = true;
                player.isAttacking = true;
                player.canFlip = false;
                lastAttackTime = Time.time;

                player.SetUntargetable(true);

                // 2. Chạy Animation
                animator.SetTrigger("Ulti");
            }
            else // Nếu UseEnergy trả về false (không đủ mana)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowManaWarning();
                }
            }
            
        }
    }

    private void ProcessDamage(Collider2D[] enemies, int damage, float kbForce)
    {
        bool hitAnything = false;

        foreach (Collider2D c in enemies)
        {
            Vector2 direction = (c.transform.position - transform.position).normalized;
            bool didDamage = false;

            // Kiểm tra Shield
            ShieldEnemy shield = c.GetComponent<ShieldEnemy>();
            if (shield != null)
            {
                didDamage = shield.TryTakeDamage(damage, transform, kbForce);
            }
            else
            {
                EnemyHealth health = c.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage, direction, kbForce);
                    didDamage = true;
                }
            }

            if (didDamage)
            {
                hitAnything = true;
                energy.GainEnergy(player.IsGrounded ? 3 : 1);
            }
        }

        if (hitAnything)
        {
            // Hiệu ứng "đứng hình" nhẹ và đẩy lùi nhân vật
            HitStopManager.Instance.Stop(0.05f);
            ApplyRecoil();
        }
    }


    private void ApplyRecoil()
    {
        // Đẩy Player lùi lại một chút khi chém trúng
        float recoilDir = -player.facingDirection;
        rb.velocity = Vector2.zero; // Reset vận tốc trước khi đẩy
        rb.AddForce(new Vector2(recoilDir * recoilForce, 0), ForceMode2D.Impulse);
    }

    private void DealDamage()
    {
        if (comboStep == 3) return; // Bước 3 bắn Wind

        Vector2 size = (comboStep == 1) ? attack1Size : attack2Size;
        int damage = (comboStep == 1) ? attack1Damage : attack2Damage;

        Collider2D[] enemies = Physics2D.OverlapBoxAll(attackPoint.position, size, 0f, enemyLayerMask);
        ProcessDamage(enemies, damage, knockbackForce);
    }

    private void DealAirDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapBoxAll(airAttackPoint.position, airSize, 0f, enemyLayerMask);
        ProcessDamage(enemies, airDamage, knockbackForce * 0.8f);
    }

    public void DealUltiDamageTick()
    {
        Vector2 checkPosition = transform.position;
        if (ultiCenter != null)
        {
            checkPosition = ultiCenter.position;
        }

        Collider2D[] enemies = Physics2D.OverlapBoxAll(checkPosition, ultiSize, 0f, enemyLayerMask);
        ProcessUltiDamage(enemies, ultiDamage);
    }

    // Viết riêng một hàm xử lý damage cho Ulti để KHÔNG bị giật lùi (ApplyRecoil)
    private void ProcessUltiDamage(Collider2D[] enemies, int damage)
    {
        bool hitAnything = false;

        foreach (Collider2D c in enemies)
        {
            Vector2 direction = (c.transform.position - transform.position).normalized;
            bool didDamage = false;

            ShieldEnemy shield = c.GetComponent<ShieldEnemy>();
            if (shield != null)
            {
                // Force đẩy lùi = 0 để quái không bị văng ra khỏi vùng chém
                didDamage = shield.TryTakeDamage(damage, transform, 0f);
            }
            else
            {
                EnemyHealth health = c.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage, direction, 0f);
                    didDamage = true;
                }
            }

            if (didDamage)
            {
                hitAnything = true;
            }
        }

        if (hitAnything)
        {
            // Chỉ khựng hình nhẹ để tạo cảm giác chém trúng, tuyệt đối không gọi ApplyRecoil()
            HitStopManager.Instance.Stop(0.02f);
        }
    }

    public void EndUlti()
    {
        player.isUsingUlti = false;
        EndAttack(); // Reset cờ isAttacking và canFlip (hàm có sẵn của bạn)
        player.SetUntargetable(false); // Hiện hình lại, nhận va chạm bình thường
    }

    public void SpawnWind()
    {
        GameObject wind = Instantiate(windPrefab, attack3firePoint.position, Quaternion.identity);
        WindProjectile projectile = wind.GetComponent<WindProjectile>();
        if (projectile != null) projectile.SetDirection(player.facingDirection);
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
        Vector2 centerPos = transform.position;
        if (ultiCenter != null)
        {
            centerPos = ultiCenter.position;
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(centerPos, ultiSize);
    }
}
