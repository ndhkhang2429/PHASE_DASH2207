using UnityEngine;

public class UIHelper : MonoBehaviour
{
    // Hàm này làm nhiệm vụ "gọi hộ" AudioController
    public void PlayButtonSound()
    {
        // Kiểm tra xem AudioController đã được mang từ Menu sang chưa
        if (AudioController.Instance != null)
        {
            AudioController.Instance.PlayButtonSFX();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy AudioController. Chắc bạn chạy thẳng từ Scene Main mà chưa qua Menu đúng không?");
        }
    }
}