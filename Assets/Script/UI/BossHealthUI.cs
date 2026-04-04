using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public static BossHealthUI Instance; // Dùng Singleton để gọi mọi nơi mà không cần kéo thả

    [Header("UI References")]
    public GameObject healthPanel; 
    public Image fillImage;
    public GameObject rawImage;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Giấu thanh máu đi khi mới vào game
        if (healthPanel != null) healthPanel.SetActive(false);
        if (rawImage != null) rawImage.SetActive(false);
    }

    // Hàm để Boss gọi khi nó thức dậy
    public void ShowHealthBar()
    {
        if (healthPanel != null) healthPanel.SetActive(true);
        if (rawImage != null) rawImage.SetActive(true);
        if (fillImage != null) fillImage.fillAmount = 1f; // Bơm đầy máu
    }

    // Hàm để Boss gọi mỗi khi bị đánh trúng
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = (float)currentHealth / maxHealth;
        }

        // Nếu máu <= 0 thì giấu thanh máu đi
        if (currentHealth <= 0 && healthPanel != null)
        {
            healthPanel.SetActive(false);
        }
    }
}