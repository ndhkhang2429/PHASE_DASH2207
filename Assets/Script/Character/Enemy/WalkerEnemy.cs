using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class WalkerEnemy : EnemyBase
{
    [Header("Move")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private Transform groundCheck;//kiem tra co dat phia duoi ko
    [SerializeField] private Transform wallCheck;//kiem tra co tuong phia truoc ko
    [SerializeField] private float checkDistance;//do dai tia raycast
    public LayerMask groundLayer;//layer nao duoc xem la mat dat

    private bool movingRight = true;

    //update chay moi frame voi moi frame goi ham move
    private void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
        RaycastHit2D groundInfo = Physics2D.Raycast(//kiem tra dat phia duoi

            groundCheck.position,//ban tia tu groundcheck
            Vector2.down,//huong xuong duoi
            checkDistance,//dai chechDistance
            groundLayer//chi va cham voi groundLayer
        );

        RaycastHit2D wallInfo = Physics2D.Raycast(//kiem tra tuong phia truoc
            wallCheck.position,
            movingRight ? Vector2.right : Vector2.left,
            checkDistance,
            groundLayer
        );

        if(!groundInfo.collider || wallInfo.collider)
        {
            Flip();//quay dau
        }

    }

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.GameOver();
        }
    }
}
