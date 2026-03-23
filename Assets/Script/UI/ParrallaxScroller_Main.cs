using UnityEngine;

public class ParallaxScroller_Main: MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    [Header("Setting speed scroll (0.0 đến 1.0)")]
    [Tooltip("0 = Đứng im như mặt đất, 1 = Đứng im trên màn hình (Bầu trời)")]
    public float parallaxMultiplier;

    void Start()
    {
        // Tự động tìm Camera chính trong màn chơi
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        // Tính toán xem Camera đã di chuyển bao nhiêu so với frame trước
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Dịch chuyển tấm nền theo hướng camera di chuyển, nhân với hệ số trôi
        transform.position += new Vector3(deltaMovement.x * parallaxMultiplier, deltaMovement.y * parallaxMultiplier, 0);

        // Lưu lại vị trí camera mới
        lastCameraPosition = cameraTransform.position;
    }
}
