using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth;
    private int currentHealth;

    [Header("Hit Effect")]
    [SerializeField] private float hitStunTime;

    [Header("Energy Reward")]
    [SerializeField] private int energyReward;

    [Header("UI")]
    [SerializeField] private DamageText damageTextPrefab; // Kéo thả prefab DamageText vào đây
    [SerializeField] private Color damageColor = Color.red;

    private bool isDead = false;
    public bool IsStunned { get; private set; }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int dame, Vector2 knockbackDirection, float knockbackForce)
    {
        if (isDead) return;

        ChargerEnemy charger = GetComponent<ChargerEnemy>();

        if(charger != null && charger.IsCharging())
        {
            dame = Mathf.CeilToInt(dame * 0.5f); // giảm 50% damage khi đang charge
            Debug.Log("Charger dang Charge -> giam 50% damage");
        }

        currentHealth -= dame;
        Debug.Log(gameObject.name + " bi trung " + dame + " damage. Mau con lai: " + currentHealth);

        if (damageTextPrefab != null)
        {
            // Cộng thêm một chút offset (ví dụ Vector3.up) nếu muốn chữ hiện cao hơn gốc của quái
            DamageText dmgText = Instantiate(damageTextPrefab, transform.position, Quaternion.identity);

            // Gọi hàm SetData để gán số damage và màu
            dmgText.SetData(dame.ToString(), damageColor);
        }

        if (!(charger != null && charger.IsCharging()))
        {
            //Knockback(day lui)
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);//day manh tuc thi
            }

            //Hit stun
            StartCoroutine(HitStun());
        }
            

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitStun()
    {
        IsStunned = true;
        yield return new WaitForSeconds(hitStunTime);
        IsStunned = false;
    }

    private void Die()
    {
        isDead = true;
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
