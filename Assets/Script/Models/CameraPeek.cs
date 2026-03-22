using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPeek : MonoBehaviour
{
    [Header("Settings Camera")]
    public CinemachineVirtualCamera virtualCamera;
    public float panDownAmount = -5f; // Thông số âm để camera nhìn xuống

    private float originalOffsetY;
    private CinemachineFramingTransposer transposer;

    void Start()
    {
        // Lấy bộ phận điều khiển khung hình của Cinemachine
        transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        // Lưu lại vị trí Y ban đầu để sau này trả về
        originalOffsetY = transposer.m_TrackedObjectOffset.y;
    }

    // Khi nhân vật BƯỚC VÀO mép vực
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Đảm bảo đối tượng chạm vào là Player (Nhớ gắn tag "Player" cho nhân vật)
        if (collision.CompareTag("Player"))
        {
            transposer.m_TrackedObjectOffset.y = panDownAmount;
        }
    }

    // Khi nhân vật RỜI KHỎI mép vực (quay đầu lại hoặc đã nhảy xuống)
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            transposer.m_TrackedObjectOffset.y = originalOffsetY;
        }
    }
}
