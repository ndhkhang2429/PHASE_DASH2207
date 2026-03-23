using UnityEngine;

// Khai báo một cấu trúc để gom nhóm Layer và Tốc độ lại với nhau
[System.Serializable]
public struct ParallaxLayer
{
    public Transform layerObject; // Kéo thả tấm ảnh vào đây
    public float parallaxSpeed;   // Chỉnh tốc độ (0 đến 1)
}

public class ParallaxManager : MonoBehaviour
{
    [Header("Danh sách 11 lớp Background của bạn")]
    public ParallaxLayer[] backgroundLayers; // Mảng chứa 11 lớp

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    void Start()
    {
        // Lấy vị trí ban đầu của Camera
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        // Tính toán khoảng cách Camera vừa di chuyển
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Cho cả 11 lớp cùng trôi qua 1 vòng lặp (Cực kỳ tối ưu)
        foreach (ParallaxLayer bg in backgroundLayers)
        {
            if (bg.layerObject != null)
            {
                bg.layerObject.position += new Vector3(deltaMovement.x * bg.parallaxSpeed, deltaMovement.y * bg.parallaxSpeed, 0);
            }
        }

        // Cập nhật lại vị trí camera cho khung hình tiếp theo
        lastCameraPosition = cameraTransform.position;
    }
}
