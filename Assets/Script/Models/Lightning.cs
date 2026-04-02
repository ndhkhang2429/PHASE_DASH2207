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
        // Tự động xóa tia sét sau một khoảng thời gian để tránh rác bộ nhớ
        Destroy(gameObject, lifetime);

        // [Tùy chọn] Nếu bạn có AudioController, phát tiếng sét đánh ở đây
        // AudioController.Instance.PlaySFX(AudioController.Instance.lightningSound);
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
