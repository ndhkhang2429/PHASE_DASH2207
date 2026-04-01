using System.Collections;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    public TextMeshProUGUI manaWarningText;

    private Coroutine warningCoroutine;

    private void Awake()
    {
        // Khởi tạo Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowManaWarning()
    {
        // Nếu đang có một cảnh báo chạy dở, dừng nó lại để chạy cái mới
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }

        warningCoroutine = StartCoroutine(DisplayWarningRoutine());
    }

    private IEnumerator DisplayWarningRoutine()
    {
        manaWarningText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        manaWarningText.gameObject.SetActive(false);
    }
}