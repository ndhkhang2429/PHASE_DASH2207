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

    private PlayerEnergy energy;
    private Rigidbody2D rb;

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
        float move = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("Speed", Mathf.Abs(move));

        HandleDashInput();
        UpdateDash();

        if(Input.GetKeyDown(KeyCode.K))
        {
            TryCastSkill();
        }

        if (!isDashing)
        {
            Move();
        }

        animator.SetBool("IsGround", IsGrounded);
        animator.SetFloat("YVelocity", rb.velocity.y);
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
        dashDirection = transform.localScale.x;
    }

    private void Move()
    {
        //khoa di chuyen player khi tan cong
        if(isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }
        //di chuyen sang trai sang phai
        horizontal = Input.GetAxisRaw("Horizontal");
        transform.Translate(Vector2.right * horizontal * speed * Time.deltaTime);

        //kiem tra huong cua nhan vat quay sang trai hay sang phai, neu sang trai thi lat nguoc hinh lai
        if (horizontal > 0)
        {
            transform.localScale = new Vector3(6, 6, 1);
        }
        else if (horizontal < 0)
        {
            transform.localScale = new Vector3(-6, 6, 1);
        }

        //kiem tra nhan vat co dang dung tren mat dat hay khong
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        //Neu dung tren mat dat thi co the nhay
        if(Input.GetKeyDown(KeyCode.Space) && IsGrounded)
        {
            rb.velocity = new Vector2 (rb.velocity.x, jumpSpeed);
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

        float direction = transform.localScale.x;
        projectile.GetComponent<EnergyProjectile>().SetDirection(direction);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
