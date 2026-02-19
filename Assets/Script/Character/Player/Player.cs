using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header ("Move")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpSpeed;
    private float horizontal;

    [Header ("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Dash")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration; //dash trong bao nhieu lau
    [SerializeField] private float dashCoolDown; //thoi gian cho truoc khi dash lai
    [SerializeField] private int dashEnergyCost;

    [Header("Jump System")]
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    private int jumpCount;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private PlayerEnergy energy;
    public Rigidbody2D rb;
    public int facingDirection { get; private set; } = 1;

    [Header("Health")]
    [SerializeField] private int maxHealth;
    private int currentHealth;

    [Header("Hit Effect")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor;
    [SerializeField] private float hitFlashTime;

    [Header("Skill")]
    [SerializeField] private GameObject projectilePerfab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int skillEnergyCost;

    //Kiem tra o mat dat
    public bool IsGrounded { get; private set;}
    [SerializeField] public Color baseColor;

    //kiem tra trang thai dash
    private bool isDashing;
    private float dashTimer; //dem nguoc thoi gian dash
    private float dashCooldownTimer; //dem nguoc hoi chieu
    private float dashDirection; //luu lai thoi gian luc bat dau dash
    public bool isAttacking { get; set; }
    [HideInInspector] public bool canFlip = true;

    private bool isInvincible;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        baseColor = spriteRenderer.color;
        energy = GetComponent<PlayerEnergy>();
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

        if(!isDashing)
        {
            if (!isAttacking || !IsGrounded)
            {
                Move();
            }
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

    public void TakeDame(int dame)
    {
        if(isInvincible)
        {
            return;
        }

        currentHealth -= dame;

        if(currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitEffect());
        }
    }

    private IEnumerator HitEffect()
    {
        isInvincible = true;
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashTime);

        spriteRenderer.color = baseColor;
        isInvincible = false;
    }

    private void Die()
    {
        GameManager.Instance.GameOver();//player chet ->ket thuc game
        Destroy(gameObject);
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
            rb.velocity = Vector2.zero;
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

    private void Dash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCoolDown;
        dashDirection = facingDirection;
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
            6 * facingDirection,
            6,
            1
            );
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
