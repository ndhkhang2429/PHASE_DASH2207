using System.Collections;
using UnityEngine;

public class VictoryManager : MonoBehaviour
{
    [Header("Phân cảnh 1: Cúp & Pháo hoa")]
    [SerializeField] private GameObject trophyObject;
    [SerializeField] private GameObject[] fireworkObjects;

    [Header("Phân cảnh 2: Giao diện")]
    [SerializeField] private GameObject victoryPanel;

    public void StartVictorySequence()
    {
        StartCoroutine(VictoryRoutine());
    }

    private IEnumerator VictoryRoutine()
    {
        // 1. CHỜ BOSS CHẾT (1.5 giây)
        yield return new WaitForSeconds(1.5f);

        // 2. HIỆN CÚP
        if (trophyObject != null)
        {
            trophyObject.SetActive(true);
        }

        // 3. BẮN TOÀN BỘ PHÁO HOA (Duyệt qua mảng và bật từng cái lên)
        foreach (GameObject firework in fireworkObjects)
        {
            if (firework != null)
            {
                if (AudioController.Instance != null)
                {
                    AudioController.Instance.PlaySFX(AudioController.Instance.fireWork);
                }
                firework.SetActive(true);

            }
        }

        // 4. CHỜ NGƯỜI CHƠI TẬN HƯỞNG (3 giây)
        yield return new WaitForSeconds(3f);

        // 5. HIỆN UI VICTORY TỪ DƯỚI LÊN
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // 6. CHỜ UI CHẠY XONG RỒI DỪNG GAME (1 giây)
        yield return new WaitForSeconds(1.5f);

        Time.timeScale = 0f;
    }
}
