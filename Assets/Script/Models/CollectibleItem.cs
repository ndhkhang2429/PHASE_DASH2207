using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public enum ItemType { Health, Energy }

    [Header("Setting")]
    public ItemType type; // Chọn loại trong Inspector
    public int amount;   // Lượng hồi phục (ví dụ: 10, 20)

    [Header("Sound")]
    public AudioClip pickupSound; // Kéo file mp3 vào đây

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có đụng trúng thằng Player không
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            if (player != null)
            {
                if (type == ItemType.Health)
                {
                    player.Heal(amount);
                }
                else if (type == ItemType.Energy)
                {
                    player.GetComponent<PlayerEnergy>().GainEnergy(amount);
                }

                // Phát âm thanh nếu có
                if (AudioController.Instance != null && pickupSound != null)
                {
                    AudioController.Instance.PlaySFX(pickupSound);
                }

                // Ăn xong thì biến mất
                Destroy(gameObject);
            }
        }
    }
}