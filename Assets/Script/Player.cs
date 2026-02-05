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

    private Rigidbody2D rb;
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Move();
        Debug.Log(isGrounded);
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
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);

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
