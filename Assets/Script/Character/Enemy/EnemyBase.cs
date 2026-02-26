using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    [SerializeField] protected EnemyHealth health;
    [SerializeField] protected float moveSpeed;

    protected bool isFacingRight = true;
    protected bool isDead = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
    }

    protected virtual void Update()
    {
        if (isDead) return;
        if (health != null)
        {
            if (health.IsStunned) return;
        }

        LogicUpdate();
    }

    protected virtual void LogicUpdate()
    {
        // Override ở enemy con
    }

    public virtual void OnDeath()
    {
        if (isDead) return;

        isDead = true;
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        rb.velocity = Vector2.zero;
        rb.simulated = false;

        Destroy(gameObject, 1.2f);
    }

    public virtual void PlayHurtAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }
    }

    // Hàm hỗ trợ flip (dùng chung)
    protected void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
