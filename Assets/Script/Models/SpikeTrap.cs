using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Sát thương của gai")]
    public int damage = 20;

    // Kích hoạt khi Player chạm vào vùng Trigger của gai
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem người chạm vào có đúng là Player không
        if (collision.CompareTag("Player"))
        {
            // Lấy script Player ra
            Player playerScript = collision.GetComponent<Player>();

            if (playerScript != null)
            {
                // Tính toán hướng nảy lùi lại (Knockback) 
                // Nảy lên trên một chút để văng ra khỏi bẫy gai
                Vector2 knockbackDirection = new Vector2(0, 1f).normalized;

                // Gọi hàm trừ máu và nảy lùi trong code của bạn (Lực nảy là 10f)
                playerScript.TakeDamage(damage, knockbackDirection, 5f);
            }
        }
    }
}
