using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
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


    private Rigidbody2D rb;

    [Header("Health")]
    [SerializeField] private int maxHealth;
    private int currentHealth;

    [Header("Hit Effect")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor;
    [SerializeField] private float hitFlashTime;
    //Kiem tra o mat dat
    private bool isGrounded;
    [SerializeField] public Color baseColor;

    //kiem tra trang thai dash
    private bool isDashing;
    private float dashTimer; //dem nguoc thoi gian dash
    private float dashCooldownTimer; //dem nguoc hoi chieu
    private float dashDirection; //luu lai thoi gian luc bat dau dash

    
    private bool isInvincible;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        baseColor = spriteRenderer.color;
    }

    private void Update()
    {
        HandleDashInput();
        UpdateDash();

        if (!isDashing)
        {
            Move();
        }
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
            Dash();

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
        //di chuyen sang trai sang phai
        horizontal = Input.GetAxisRaw("Horizontal");
        transform.Translate(Vector2.right * horizontal * speed * Time.deltaTime);

        //kiem tra huong cua nhan vat quay sang trai hay sang phai, neu sang trai thi lat nguoc hinh lai
        if (horizontal >= 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if(horizontal < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        //kiem tra nhan vat co dang dung tren mat dat hay khong
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        //Neu dung tren mat dat thi co the nhay
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2 (rb.velocity.x, jumpSpeed);
        }


    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
