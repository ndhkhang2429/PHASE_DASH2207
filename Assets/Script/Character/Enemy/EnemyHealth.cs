using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth;

    public int CurrentHealth { get; private set; }

    [Header("Component")]
    [SerializeField] private Animator animator;

    [Header("Hit Effect")]
    [SerializeField] private float hitStunTime;

    [Header("Energy Reward")]
    [SerializeField] private int energyReward;

    [Header("UI")]
    [SerializeField] private DamageText damageTextPrefab; // Kéo thả prefab DamageText vào đây
    [SerializeField] private Color damageColor = Color.red;

    public bool IsDead { get; private set; } = false;
    public bool IsStunned { get; private set; }

    private void Start()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int dame, Vector2 knockbackDirection, float knockbackForce)
    {
        if (IsDead) return;

        ChargerEnemy charger = GetComponent<ChargerEnemy>();

        if(charger != null && charger.IsCharging())
        {
            dame = Mathf.CeilToInt(dame * 0.5f); // giảm 50% damage khi đang charge
            Debug.Log("Charger dang Charge -> giam 50% damage");
        }

        CurrentHealth -= dame;
        Debug.Log(gameObject.name + " bi trung " + dame + " damage. Mau con lai: " + CurrentHealth);

        if (animator != null)
        {
            animator.SetTrigger("Hurt"); // Nhớ kiểm tra chữ "Hurt" có viết hoa chữ H giống hệt trong Animator không nhé!
        }

        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0f, 1f, -1f);
            // Cộng thêm một chút offset (ví dụ Vector3.up) nếu muốn chữ hiện cao hơn gốc của quái
            DamageText dmgText = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

            // Gọi hàm SetData để gán số damage và màu
            dmgText.SetData(dame.ToString(), damageColor);
        }

        if (CurrentHealth <= 0)
        {
            // NẾU HẾT MÁU: Chỉ gọi chết, không gọi Hurt, không văng lùi
            Die();
        }
        else
        {
            // NẾU CHƯA HẾT MÁU: Mới gọi Hurt và tính toán Knockback, Stun
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }

            if (!(charger != null && charger.IsCharging()))
            {
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = knockbackDirection * knockbackForce;
                }
                StartCoroutine(HitStun());
            }
        }
    }

    private IEnumerator HitStun()
    {
        IsStunned = true;
        yield return new WaitForSeconds(hitStunTime);
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
        IsStunned = false;
    }

    private void Die()
    {
        IsDead = true;
        PlayerEnergy energy = FindAnyObjectByType<PlayerEnergy>();
        if (energy != null)
        {
            energy.GainEnergy(energyReward);
        }

        EnemyBase enemy = GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.OnDeath();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
