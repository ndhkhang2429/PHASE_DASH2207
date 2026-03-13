using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Component")]
    public Rigidbody2D rb;
    [SerializeField] private Animator animator;

    [Header ("Move")]
    [SerializeField] private float speed;
    private float horizontal;

    [Header ("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayerMask;

    public bool IsGrounded { get; private set; }

    [Header("Dash")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration; //dash trong bao nhieu lau
    [SerializeField] private float dashCoolDown; //thoi gian cho truoc khi dash lai
    [SerializeField] private int dashEnergyCost;

    //kiem tra trang thai dash
    private float originalGravity;//vung bien private
    private bool isDashing;
    private float dashTimer; //dem nguoc thoi gian dash
    private float dashCooldownTimer; //dem nguoc hoi chieu
    private float dashDirection; //luu lai thoi gian luc bat dau dash

    [Header("Jump System")]
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    private int jumpCount;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private PlayerEnergy energy;

    public int facingDirection { get; private set; } = 1;
    public bool canFlip = true;

    [Header("Health")]
    [SerializeField] private int maxHealth;
    private int currentHealth;
    [SerializeField] private HealthBar healthBar;

    [Header("Hit Effect")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor;
    [SerializeField] private float hitFlashTime;
    private Color baseColor;
    private bool isInvincible;

    [Header("Skill")]
    [SerializeField] private GameObject projectilePerfab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int skillEnergyCost;

    public bool isAttacking { get; set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        baseColor = spriteRenderer.color;
        energy = GetComponent<PlayerEnergy>();
        originalGravity = rb.gravityScale;

        healthBar.UpdateBar(currentHealth, maxHealth);
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        animator.SetFloat("Speed", Mathf.Abs(horizontal));
        animator.SetBool("IsGround", IsGrounded);
        animator.SetFloat("YVelocity", rb.velocity.y);

        HandleDashInput();
        UpdateDash();

        if(Input.GetKeyDown(KeyCode.K))
        {
            TryCastSkill();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
        }

       HandleFlip();
    }

    private void FixedUpdate()
    {
        CheckGround();

        if(!isDashing && !isAttacking)
        {
            Move();
        }

        HandleJump();
    }

    private void HandleJump()
    {
        jumpBufferTimer -= Time.fixedDeltaTime;

        if (IsGrounded)
        {
            coyoteTimer = coyoteTime;
            jumpCount = 0;
        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime;
        }

        if (jumpBufferTimer > 0)
        {
            if (coyoteTimer > 0 || jumpCount < maxJumpCount)
            {
                PerformJump();
                jumpBufferTimer = 0;
            }
        }
    }

    private void PerformJump()
    {
        Debug.Log($"jb={jumpBufferTimer}, cb={coyoteTimer}, jc={jumpCount}");
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        jumpCount++;
        coyoteTimer = 0;
    }

    private void CheckGround()
    {
        IsGrounded = Physics2D.OverlapCircle
        (
            groundCheck.position,
            groundCheckRadius,
            groundLayerMask
        );
    }

    private void Move()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    public void TakeDame(int damage)
    {
        Vector2 dir = Vector2.zero;
        TakeDamage(damage, dir, 2f);
    }

    public void TakeDamage(int dame, Vector2 knockbackDir, float knockbackForce)
    {
        if(isInvincible)
        {
            return;
        }

        if (currentHealth <= 0) return;

        currentHealth -= dame;

        // Knockback
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitEffect());
        }
        healthBar.UpdateBar(currentHealth, maxHealth);
    }

    private IEnumerator HitEffect()
    {
        isInvincible = true;
        canFlip = false;
        isAttacking = false;
        animator.SetTrigger("Hurt");
        spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(hitFlashTime);

        spriteRenderer.color = baseColor;

        canFlip = true;
        isInvincible = false;
    }

    private void Die()
    {
        isInvincible = true;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;

        animator.SetTrigger("Die");

        this.enabled = false; // tắt script điều khiển
    }

    private void OnDeathAnimationEnd()
    {
        GameManager.Instance.GameOver();
        Destroy(gameObject);
    }


    private void Dash()
    {
        isDashing = true;
        isInvincible = true;
        canFlip = false;

        dashTimer = dashDuration;
        dashCooldownTimer = dashCoolDown;
        dashDirection = facingDirection;

        rb.gravityScale = 0; // tat gravity
        animator.SetTrigger("Dash");
    }

    //kiem tra dash trong tung frame hinh 
    private void UpdateDash()
    {
        if (!isDashing) return;

        dashTimer -= Time.deltaTime;

        // Ép vận tốc khi dash
        rb.velocity = new Vector2(dashDirection * dashSpeed, 0);

        if (dashTimer <= 0)
        {
            isDashing = false;
            isInvincible = false;
            canFlip = true;

            rb.gravityScale = originalGravity;
        }
    }

    //Quan ly bam nut dash
    private void HandleDashInput()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.E) && dashCooldownTimer <= 0 && !isDashing)
        {
            if(energy != null && energy.UseEnergy(dashEnergyCost))
            {
                Dash();
            }
        }

    }

    private void TryCastSkill()
    {
        if (energy == null) return;

        if (!energy.UseEnergy(skillEnergyCost)) return;

        GameObject projectile = Instantiate
        (
            projectilePerfab,
            firePoint.position,
            Quaternion.identity
        );

        float direction = facingDirection;
        projectile.GetComponent<EnergyProjectile>().SetDirection(direction);
    }

    private void HandleFlip()
    {
        if(!canFlip) return;
        if (horizontal == 0) return;

        facingDirection = horizontal > 0 ? 1 : -1;

        transform.localScale = new Vector3(
            1 * facingDirection,
            1,
            1
            );
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
