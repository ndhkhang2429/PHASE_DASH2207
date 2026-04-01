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
    private ChargerEnemy chargerScript;
    private Rigidbody2D rb;

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

        bool isCharging = (chargerScript != null && chargerScript.IsCharging());
        if (isCharging)
        {
            dame = Mathf.CeilToInt(dame * 0.5f);
            Debug.Log("Charger đang Charge -> Giảm 50% damage");
        }

        CurrentHealth -= dame;

        if (damageTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0f, 1f, -1f);
            // Cộng thêm một chút offset (ví dụ Vector3.up) nếu muốn chữ hiện cao hơn gốc của quái
            DamageText dmgText = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

            // Gọi hàm SetData để gán số damage và màu
            dmgText.SetData(dame.ToString(), damageColor);
        }

        if (gameObject.CompareTag("Boss") && BossHealthUI.Instance != null)
        {
            BossHealthUI.Instance.UpdateHealthBar(CurrentHealth, maxHealth); // Thay bằng tên biến HP của bạn
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

            if (!isCharging)
            {
                ApplyKnockback(knockbackDirection, knockbackForce);
            }
        }
    }
    private void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb != null)
        {
            rb.velocity = direction * force;
        }

        // Ngừng Coroutine cũ nếu đang Stun để tránh chồng chéo
        StopCoroutine(nameof(HitStun));
        StartCoroutine(HitStun());
    }

    private IEnumerator HitStun()
    {
        IsStunned = true;
        yield return new WaitForSeconds(hitStunTime);
        if (!IsDead && rb != null)
            rb.velocity = Vector2.zero;
        IsStunned = false;
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;
        PlayerEnergy energy = FindAnyObjectByType<PlayerEnergy>();
        if (energy != null)
        {
            energy.GainEnergy(energyReward);
        }

        if (gameObject.CompareTag("Boss"))
        {
            VictoryManager victoryManager = FindAnyObjectByType<VictoryManager>();
            if (victoryManager != null)
            {
                victoryManager.StartVictorySequence();
            }
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
