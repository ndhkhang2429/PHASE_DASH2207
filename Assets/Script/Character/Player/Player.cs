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
    [SerializeField] private float dashAttackWindow = 0.2f;
    private float dashAttackTimer;

    //kiem tra trang thai dash
    private float originalGravity;//vung bien private
    private bool isDashing;
    private float dashTimer; //dem nguoc thoi gian dash
    private float dashCooldownTimer; //dem nguoc hoi chieu
    private float dashDirection; //luu lai thoi gian luc bat dau dash
    private int playerLayer;
    private int enemyLayer;

    [Header("Jump System")]
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    private int jumpCount;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float jumpCooldownTimer;

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

        //xet layer khi dash
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        // 1. Cập nhật các bộ đếm thời gian (Timers)
        if (coyoteTimer > 0) coyoteTimer -= Time.deltaTime;
        if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime;
        if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;

        // 2. Chuyển Ground Check lên Update để đồng bộ chuẩn xác với Animator
        CheckGround();

        // 3. Nhận Input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        // 4. Xử lý nhảy ngay trong Update để input không bị delay
        HandleJump();

        // 5. Cập nhật Animator
        animator.SetFloat("Speed", Mathf.Abs(horizontal));
        animator.SetBool("IsGround", IsGrounded);
        animator.SetFloat("YVelocity", rb.velocity.y);


        HandleDashInput();
        UpdateDash();

        if(Input.GetKeyDown(KeyCode.K))
        {
            TryCastSkill();
        }

        HandleFlip();
    }

    private void FixedUpdate()
    {
        if (!isDashing && !isAttacking)
        {
            Move();
        }
    }

    private void Move()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    private void CheckGround()
    {
        if (jumpCooldownTimer > 0f)
        {
            IsGrounded = false;
            return;
        }

        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        // Reset lại số lần nhảy và Coyote Time khi chạm đất an toàn
        if (IsGrounded && rb.velocity.y <= 0)
        {
            coyoteTimer = coyoteTime;
            jumpCount = 0;
        }
    }

    private void HandleJump()
    {
        if (jumpBufferTimer > 0f)
        {
            // Cho phép nhảy nếu đang trên đất/rơi nhẹ (coyote) HOẶC số lần nhảy < max
            if (coyoteTimer > 0f || jumpCount < maxJumpCount)
            {
                PerformJump();
            }
        }
    }

    private void PerformJump()
    {
        // FIX DOUBLE JUMP: Triệt tiêu trọng lực rơi trước khi áp dụng lực nhảy mới
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);

        jumpCount++;
        jumpBufferTimer = 0f; // Dọn bộ nhớ đệm
        coyoteTimer = 0f;     // Hủy coyote

        // Kích hoạt khiên chống nhiễu mặt đất
        jumpCooldownTimer = 0.1f;
        IsGrounded = false;
    }


    private void HandleFlip()
    {
        if (!canFlip) return;
        if (horizontal == 0) return;

        facingDirection = horizontal > 0 ? 1 : -1;

        // BỎ flipX. Dùng Scale lật toàn bộ hệ thống vật lý để né rãnh nứt Tilemap.
        // Hãy đảm bảo Offset X của Collider = 0 để không bị khựng lúc quay đầu.
        if (facingDirection == 1)
        {
            transform.localScale = baseScale;
        }
        else
        {
            transform.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);
        }
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
        if (!isAttacking)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }

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
        if (!isAttacking)
        {
            animator.SetTrigger("Hurt");
        }

        spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(hitFlashTime);

        spriteRenderer.color = baseColor;

        canFlip = true;
        isInvincible = false;

        PlayerAttack atk = GetComponent<PlayerAttack>();
        if (atk != null)
        {
            atk.CancelAttack();
        }
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
        dashAttackTimer = dashAttackWindow;

        rb.gravityScale = 0; // tat gravity
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        animator.SetTrigger("Dash");
    }

    //kiem tra dash trong tung frame hinh 
    private void UpdateDash()
    {
        if (!isDashing) return;

        dashTimer -= Time.deltaTime;

        dashAttackTimer -= Time.deltaTime;

        // Ép vận tốc khi dash
        rb.velocity = new Vector2(dashDirection * dashSpeed, 0);

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

        projectile.GetComponent<EnergyProjectile>().SetDirection(facingDirection);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
