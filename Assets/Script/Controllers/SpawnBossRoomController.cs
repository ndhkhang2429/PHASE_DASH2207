using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SpawnBossRoomController : MonoBehaviour
{
    [Header("Room Setup")]
    public GameObject leftDoor;
    public GameObject rightDoor;
    public SpawnBoss boss;
    private Animator leftDoorAnim;
    private Animator rightDoorAnim;

    [Header("Cinemachine Cameras")]
    public CinemachineVirtualCamera zoomBossCam; // Cam 1: Nhìn sát Boss
    public CinemachineVirtualCamera arenaCam;    // Cam 2: Nhìn toàn cảnh phòng

    [Header("Cutscene Timing")]
    public float zoomDuration = 1.5f; // Ngắm mặt Boss trong bao lâu?
    public float panDuration = 1.5f;  // Thời gian lùi máy ra toàn cảnh

    [Header("Audio")]
    public AudioClip doorCloseSFX; // Kéo file âm thanh tiếng sập cửa vào đây

    private bool isLocked = false;

    private void Start()
    {
        if (leftDoor != null) leftDoorAnim = leftDoor.GetComponent<Animator>();
        if (rightDoor != null) rightDoorAnim = rightDoor.GetComponent<Animator>();
        //if (leftDoorAnim != null) leftDoorAnim.SetTrigger("Raise");
        //if (rightDoorAnim != null) rightDoorAnim.SetTrigger("Raise");

        if (boss != null)
        {
            boss.roomManager = this;
        }

        // Đảm bảo ban đầu 2 Camera này đang tắt (Priority thấp hơn Player Cam)
        if (zoomBossCam != null) zoomBossCam.Priority = 9;
        if (arenaCam != null) arenaCam.Priority = 9;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isLocked)
        {
            isLocked = true;
            StartCoroutine(BossIntroCutscene());
        }
    }

    private IEnumerator BossIntroCutscene()
    {
        // 1. SẬP CỬA & PHÁT TIẾNG ĐỘNG
        if (leftDoorAnim != null) leftDoorAnim.SetTrigger("Slam");
        if (rightDoorAnim != null) rightDoorAnim.SetTrigger("Slam");

        if (AudioController.Instance != null && doorCloseSFX != null)
        {
            AudioController.Instance.PlaySFX(doorCloseSFX);
        }

        // 2. ĐỔI NHẠC BOSS
        if (AudioController.Instance != null && AudioController.Instance.BossBGM != null)
        {
            AudioController.Instance.PlayBGM(AudioController.Instance.BossBGM);
        }

        // 3. ZOOM SÁT MẶT BOSS
        if (zoomBossCam != null)
        {
            zoomBossCam.Priority = 11; // Lớn hơn 10 (của Player Cam), máy sẽ tự động lia mượt mà tới Boss
        }

        // Chờ diễn viên Boss "tạo dáng"
        yield return new WaitForSeconds(zoomDuration);

        // 4. LÙI MÁY RA TOÀN CẢNH PHÒNG
        if (arenaCam != null)
        {
            arenaCam.Priority = 12; // Lớn hơn 11, máy sẽ lia từ mặt Boss ra giữa phòng
        }

        // Chờ máy lùi xong
        yield return new WaitForSeconds(panDuration);

        // 5. KẾT THÚC CUTSCENE, GỌI BOSS DẬY
        Debug.Log("FIGHT!");
        if (boss != null)
        {
            boss.ActivateBoss();
        }
    }

    public void UnlockRoom()
    {
        Debug.Log("Boss đã chết! Mở cửa phòng!");
        if (leftDoorAnim != null) leftDoorAnim.SetTrigger("Raise");
        if (rightDoorAnim != null) rightDoorAnim.SetTrigger("Raise");

        //// 1. TRẢ CAMERA LẠI CHO PLAYER (Hạ hết Priority xuống 9)
        //if (zoomBossCam != null) zoomBossCam.Priority = 9;
        //if (arenaCam != null) arenaCam.Priority = 9;

        // 2. ĐỔI NHẠC VỀ LẠI BÌNH THƯỜNG
        if (AudioController.Instance != null && AudioController.Instance.inGameBGM != null)
        {
            AudioController.Instance.PlayBGM(AudioController.Instance.inGameBGM);
        }
    }
}