using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Component")]
    public Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Move")]
    [SerializeField] private float speed;
    private float horizontal;

    [SerializeField] private float walkStepInterval = 0.3f;
    public bool canMove = true;
    private float stepTimer;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayerMask;
    public bool IsGrounded { get; private set; }

    [Header("Dash")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashCoolDown;
    [SerializeField] private int dashEnergyCost;
    [SerializeField] private float dashAttackWindow = 0.2f;

    private float dashAttackTimer;
    private float originalGravity;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDirection;
    private int playerLayer;
    private int enemyLayer;

    [Header("Jump System")]
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private CapsuleCollider2D playerCollider;

    private GameObject currentOneWayPlatform;
    private int jumpCount;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float jumpCooldownTimer;

    // [THÊM MỚI] Cờ hiệu báo cho FixedUpdate biết Player muốn nhảy
    private bool wantsToJump;
    public bool isUsingUlti { get; set; }

    [Header("Effects")]
    [SerializeField] private GameObject doubleJumpEffectPrefab;

    private PlayerEnergy energy;

    public int facingDirection { get; private set; } = 1;
    public bool canFlip = true;

    [Header("Health")]
    [SerializeField] private int maxHealth;
    private int currentHealth;
    [SerializeField] private HealthBar healthBar;

    [Header("Hit Effect")]
    [SerializeField] private Color hitColor;
    [SerializeField] private float hitFlashTime;
    private Color baseColor;
    private bool isInvincible;

    // [THÊM MỚI] Cờ hiệu báo Player đang bị thương (knockback)
    private bool isHurt;

    [Header("Skill")]
    [SerializeField] private GameObject projectilePerfab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int skillEnergyCost;

    public bool isAttacking { get; set; }
    private Vector3 baseScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        baseScale = transform.localScale;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        baseColor = spriteRenderer.color;
        energy = GetComponent<PlayerEnergy>();
        originalGravity = rb.gravityScale;

        healthBar.UpdateBar(currentHealth, maxHealth);

        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    private void Update()
    {
        if (!canMove || isUsingUlti)
        {
            horizontal = 0;
        }
        else
        {
            horizontal = Input.GetAxisRaw("Horizontal");
        }

        // 1. Cập nhật Timers
        if (coyoteTimer > 0) coyoteTimer -= Time.deltaTime;
        if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime;
        if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;

        // 2. Ground Check
        CheckGround();
        if (canMove)
        {
            HandleOneWayPlatform();

            if (Input.GetKeyDown(KeyCode.Space) && !isUsingUlti)
            {
                jumpBufferTimer = jumpBufferTime;
            }

            HandleDashInput();

            if (Input.GetKeyDown(KeyCode.K) && !isUsingUlti)
            {
                TryCastSkill();
            }

            HandleFlip();
        }

        HandleJump();
        UpdateDash();

        animator.SetFloat("Speed", Mathf.Abs(horizontal));
        animator.SetBool("IsGround", IsGrounded);
        animator.SetFloat("YVelocity", rb.velocity.y);
        animator.SetBool("IsDashing", isDashing);

    }

    private void FixedUpdate()
    {
        // [ĐÃ SỬA LẠI TOÀN BỘ CẤU TRÚC FIXED UPDATE]

        // 1. Xử lý các trạng thái Ưu Tiên (Dash, Bị thương)
        if (isDashing)
        {
            rb.velocity = new Vector2(dashDirection * dashSpeed, 0);
        }
        else if (isHurt)
        {
            // Đang bị knockback, KHÔNG gọi hàm Move() để tránh ghi đè lực nảy
        }
        else if (!isAttacking)
        {
            // Trạng thái bình thường -> Cho phép di chuyển
            Move();
        }

        // 2. Xử lý lực Nhảy một cách an toàn trong nhịp vật lý
        if (wantsToJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            wantsToJump = false; // Tắt cờ sau khi nảy lên
        }
    }

    private void Move()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        if (IsGrounded && Mathf.Abs(horizontal) > 0)
        {
            stepTimer -= Time.fixedDeltaTime;
            if (stepTimer <= 0)
            {
                AudioController.Instance.PlaySFX(AudioController.Instance.walkSound);
                stepTimer = walkStepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void CheckGround()
    {
        bool wasGrounded = IsGrounded;
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        if (jumpCooldownTimer > 0f)
        {
            IsGrounded = false;
        }

        if (!wasGrounded && IsGrounded)
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.landSound);
        }

        if (IsGrounded)
        {
            coyoteTimer = coyoteTime;
            jumpCount = 0;
        }
    }

    private void HandleJump()
    {
        if (jumpBufferTimer > 0f)
        {
            if (coyoteTimer > 0f || jumpCount < maxJumpCount)
            {
                PerformJump();
            }
        }
    }

    private void PerformJump()
    {
        wantsToJump = true;

        jumpCount++;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;

        animator.ResetTrigger("Jump");
        animator.SetTrigger("Jump");

        if (jumpCount == 2 && doubleJumpEffectPrefab != null)
        {
            Instantiate(doubleJumpEffectPrefab, groundCheck.position, Quaternion.identity);
        }

        AudioController.Instance.PlaySFX(AudioController.Instance.jumpSound);

        jumpCooldownTimer = 0.1f;
        IsGrounded = false;
    }

    private void HandleFlip()
    {
        if (!canFlip || horizontal == 0) return;

        facingDirection = horizontal > 0 ? 1 : -1;

        if (facingDirection == 1)
        {
            transform.localScale = baseScale;
        }
        else
        {
            transform.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);
        }
    }

    private void HandleOneWayPlatform()
    {
        if (Input.GetKeyDown(KeyCode.S) && currentOneWayPlatform != null)
        {
            StartCoroutine(DisableCollision());
        }
    }

    private IEnumerator DisableCollision()
    {
        if (playerCollider == null)
        {
            playerCollider = GetComponent<CapsuleCollider2D>();
        }
        if (currentOneWayPlatform != null)
        {
            Collider2D platformCollider = currentOneWayPlatform.GetComponent<Collider2D>();

            if (platformCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
                yield return new WaitForSeconds(0.4f);
                Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("OneWayPlatform") || collision.gameObject.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = collision.gameObject;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentOneWayPlatform == null && (collision.collider.CompareTag("OneWayPlatform") || collision.gameObject.CompareTag("OneWayPlatform")))
        {
            currentOneWayPlatform = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWayPlatform") || collision.collider.CompareTag("OneWayPlatform"))
        {
            currentOneWayPlatform = null;
        }
    }

    public void TakeDame(int damage)
    {
        Vector2 dir = Vector2.zero;
        TakeDamage(damage, dir, 2f);
    }

    public void TakeDamage(int dame, Vector2 knockbackDir, float knockbackForce)
    {
        if (isInvincible || currentHealth <= 0)
        {
            return;
        }

        currentHealth -= dame;
        healthBar.UpdateBar(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitEffect(knockbackDir, knockbackForce));
        }
    }

    private IEnumerator HitEffect(Vector2 knockbackDir, float knockbackForce)
    {
        isInvincible = true;
        canFlip = false;
        isAttacking = false;

        //Bật cờ bị thương để ngắt điều khiển Move()
        isHurt = true;
        rb.gravityScale = 0;

        float dir = knockbackDir.x != 0 ? Mathf.Sign(knockbackDir.x) : (facingDirection * -1);

        // Lực nảy này giờ đã an toàn, không bị Move() đè lên nữa
        rb.velocity = new Vector2(dir * knockbackForce, knockbackForce * 0.5f);

        AudioController.Instance.PlaySFX(AudioController.Instance.hurtSound);

        animator.SetTrigger("Hurt");
        spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(0.12f);

        rb.gravityScale = originalGravity;
        canFlip = true;

        // Trả lại quyền di chuyển
        isHurt = false;

        yield return new WaitForSeconds(hitFlashTime - 0.12f);

        spriteRenderer.color = baseColor;
        isInvincible = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !isInvincible)
        {
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            TakeDamage(1, knockbackDirection, 8f);
        }
    }

    private void Die()
    {
        isInvincible = true;

        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;

        animator.SetTrigger("Die");

        this.enabled = false;
        Invoke("TriggerGameOverMenu", 1f);
        AudioController.Instance.PlayBGM(AudioController.Instance.EndGameBGM);
    }

    private void TriggerGameOverMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        Destroy(gameObject);
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
        dashAttackTimer = dashAttackWindow;

        // [ĐÃ XÓA] rb.velocity = new Vector2(rb.velocity.x, 0f); -> Đã được ép lực an toàn ở FixedUpdate

        rb.gravityScale = 0;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        animator.ResetTrigger("Jump");
        animator.SetTrigger("Dash");
        AudioController.Instance.PlaySFX(AudioController.Instance.dashSound);
    }

    private void UpdateDash()
    {
        if (!isDashing) return;

        dashTimer -= Time.deltaTime;
        dashAttackTimer -= Time.deltaTime;

        if (dashTimer <= 0)
        {
            EndDash();
        }
    }

    private void EndDash()
    {
        isDashing = false;
        isInvincible = false;
        canFlip = true;

        rb.gravityScale = originalGravity;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
    }

    private void HandleDashInput()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.E) && dashCooldownTimer <= 0 && !isDashing)
        {
            if (isUsingUlti == false)
            {
                if (energy != null)
                {
                    if (energy.UseEnergy(dashEnergyCost))
                    {
                        Dash();
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
        }
    }

    private void TryCastSkill()
    {
        if (energy == null) return;

        if (!energy.UseEnergy(skillEnergyCost))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowManaWarning();
            }
            return;
        }

        GameObject projectile = Instantiate
        (
            projectilePerfab,
            firePoint.position,
            Quaternion.identity
        );

        projectile.GetComponent<EnergyProjectile>().SetDirection(facingDirection);
    }

    public void SetUntargetable(bool state)
    {
        isInvincible = state;
        if (state)
        {
            // Tắt trọng lực và khóa cứng vị trí
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;

            // Xuyên qua quái (tắt va chạm giữa 2 layer)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        }
        else
        {
            // Trả lại trạng thái bình thường
            rb.gravityScale = originalGravity;
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth; // Chặn không cho máu vượt quá max
        }

        // Cập nhật lên thanh máu (healthBar của mày)
        if (healthBar != null)
        {
            healthBar.UpdateBar(currentHealth, maxHealth);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}