using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBossRoomController : MonoBehaviour
{
    [Header("Room Bounds (Cửa 2 bên)")]
    public GameObject leftDoor;  // Bức tường/Cửa chặn bên trái
    public GameObject rightDoor; // Bức tường/Cửa chặn bên phải

    [Header("Boss Reference")]
    public SpawnBoss boss;

    private bool isLocked = false;

    private void Start()
    {
        // Ban đầu mở cửa cho Player đi vào
        if (leftDoor != null) leftDoor.SetActive(false);
        if (rightDoor != null) rightDoor.SetActive(false);

        // Báo cho Boss biết ai là quản lý phòng để nó gọi khi chết
        if (boss != null)
        {
            boss.roomManager = this;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isLocked)
        {
            LockRoom();
        }
    }

    private void LockRoom()
    {
        isLocked = true;
        Debug.Log("Cửa đã khóa! Boss xuất hiện!");

        // 1. Đóng 2 cửa lại
        if (leftDoor != null) leftDoor.SetActive(true);
        if (rightDoor != null) rightDoor.SetActive(true);

        // 2. Đánh thức Boss
        if (boss != null)
        {
            boss.ActivateBoss();
        }
    }

    // Hàm này sẽ được con Boss gọi khi nó chết
    public void UnlockRoom()
    {
        Debug.Log("Boss đã chết! Mở cửa phòng!");
        if (leftDoor != null) leftDoor.SetActive(false);
        if (rightDoor != null) rightDoor.SetActive(false);
    }
}
