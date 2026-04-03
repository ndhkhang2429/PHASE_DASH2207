using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 20; // Sát thương của sét
    public float lifetime = 0.5f; // Thời gian tồn tại của tia sét (khớp với Animation)

    private void Start()
    {
        SpawnBoss boss = FindObjectOfType<SpawnBoss>();
        if (boss != null)
        {
            AudioEnemyController bossAudio = boss.GetComponent<AudioEnemyController>();
            if (bossAudio != null)
            {
                // Tạo độ trầm bổng ngẫu nhiên để các tia sét không bị kêu y hệt nhau
                float randomPitch = 1f + Random.Range(-0.2f, 0.2f);

                // Gọi hàm phát tiếng sét mà ta vừa thêm ở AudioEnemyController
                bossAudio.PlayLightning(randomPitch);
            }
        }
        // Tự động xóa tia sét sau một khoảng thời gian để tránh rác bộ nhớ
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem thứ chạm vào có phải là Player không
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                // Gọi hàm nhận sát thương của Player
                player.TakeDame(damage);
            }
        }
    }
}
