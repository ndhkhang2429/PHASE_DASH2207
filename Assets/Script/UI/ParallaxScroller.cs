using UnityEngine;
using UnityEngine.UI;

public class ParallaxScroller : MonoBehaviour
{
    private RawImage backgroundLayer;

    [Header("Tốc độ cuộn")]
    public float scrollSpeedX = 0.1f;
    public float scrollSpeedY = 0f;

    void Start()
    {
        // Lấy component RawImage tự động
        backgroundLayer = GetComponent<RawImage>();
    }

    void Update()
    {
        // Tính toán độ dời của ảnh theo thời gian
        Rect currentRect = backgroundLayer.uvRect;
        currentRect.x += scrollSpeedX * Time.deltaTime;
        currentRect.y += scrollSpeedY * Time.deltaTime;

        // Gán lại vào ảnh
        backgroundLayer.uvRect = currentRect;
    }
}