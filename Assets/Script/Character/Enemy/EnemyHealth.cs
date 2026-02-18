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

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int dame, Vector2 knockbackDirection, float knockbackForce)
    {
        if (isDead) return;

        currentHealth -= dame;
        Debug.Log(gameObject.name + " bi trung " + dame + " damage. Mau con lai: " + currentHealth);

        //Knockback(day lui)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);//day manh tuc thi
        }

        //Hit stun
        StartCoroutine(HitStun());

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitStun()
    {
        MonoBehaviour[] scripts = GetComponentsInChildren<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        yield return new WaitForSeconds(hitStunTime);

        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = true;
            }
        }
    }

    private void Die()
    {
        PlayerEnergy energy = FindAnyObjectByType<PlayerEnergy>();
        if (energy != null)
        {
            energy.GainEnergy(energyReward);
        }
        isDead = true;
        Destroy(gameObject);
    }
}
