using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParallaxItem
{
    public Transform transform;
    [Range(0f, 1f)] public float speedFactor;

    [Header("Cài đặt Lặp vô tận")]
    public bool loopX = true;  // Tích vào nếu muốn lặp ngang (Bầu trời, núi)
    public bool loopY = false; // Tích vào nếu muốn lặp dọc (Mây, khi rơi xuống vực)
    [HideInInspector] public float width;
    [HideInInspector] public float height;
}

public class ParallaxScroller_Main : MonoBehaviour
{
    [SerializeField] private ParallaxItem[] items;
    [SerializeField] private Transform cameraTransform;

    private Vector3 _lastCameraPosition;
    private bool _isFirstFrame = true;

    private void Start()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        _lastCameraPosition = cameraTransform.position;

        // Tự động đo chiều dài/cao của ảnh để biết lúc nào cần lặp
        foreach (var item in items)
        {
            if (item.transform != null)
            {
                SpriteRenderer sr = item.transform.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    item.width = sr.bounds.size.x;
                    item.height = sr.bounds.size.y;
                }
            }
        }
    }

    private void LateUpdate()
    {
        // Nâng cấp 1: Bỏ qua frame đầu tiên để Cinemachine ổn định vị trí, tránh background bị văng mất
        if (_isFirstFrame)
        {
            _lastCameraPosition = cameraTransform.position;
            _isFirstFrame = false;
            return;
        }

        // --- LOGIC GỐC CỦA BẠN (Giữ nguyên hoàn toàn) ---
        Vector3 positionDelta = cameraTransform.position - _lastCameraPosition;

        foreach (var item in items)
        {
            if (item.transform == null) continue;

            float moveX = positionDelta.x * item.speedFactor;
            float moveY = positionDelta.y;
            item.transform.position += new Vector3(moveX, moveY, 0f);

            // --- Nâng cấp 2: Dịch chuyển ảnh xoay vòng khi Camera đi quá xa ---

            // Xử lý lặp ngang (Trục X)
            if (item.loopX && item.width > 0)
            {
                // Tính xem Camera đã đi xa khỏi tâm bức ảnh bao nhiêu
                float distFromCameraX = cameraTransform.position.x - item.transform.position.x;

                // Nếu khoảng cách lớn hơn chiều dài bức ảnh -> Dịch ảnh tới phía trước Camera
                if (Mathf.Abs(distFromCameraX) >= item.width)
                {
                    float offset;
                    if (distFromCameraX > 0)
                    {
                        offset = item.width;
                    }
                    else
                    {
                        offset = -item.width;
                    }
                    item.transform.position += new Vector3(offset, 0f, 0f);
                }
            }

            // Xử lý lặp dọc (Trục Y) - Khắc phục lỗi video lúc nãy
            if (item.loopY && item.height > 0)
            {
                float distFromCameraY = cameraTransform.position.y - item.transform.position.y;

                if (Mathf.Abs(distFromCameraY) >= item.height)
                {
                    float offset;
                    if (distFromCameraY > 0)
                    {
                        offset = item.height;
                    }
                    else
                    {
                        offset = -item.height;
                    }
                    item.transform.position += new Vector3(0f, offset, 0f);
                }
            }
        }

        _lastCameraPosition = cameraTransform.position;
    }
}
