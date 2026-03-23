using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
            {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDame(9999);
                StartCoroutine(TriggerGameOverDelay());
            }
        }
    }

    private IEnumerator TriggerGameOverDelay()
    {
        // Chờ 0.5 giây (hoặc 1 giây tùy bạn) để người chơi nhận ra mình đã rơi
        yield return new WaitForSeconds(0.5f);

        // Gọi thẳng UI Game Over từ GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
