using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParallaxItem
{
    public Transform transform;
    [Range(0f, 1f)] public float speedFactor;
}

public class ParallaxScroller_Main : MonoBehaviour
{
    [SerializeField] private ParallaxItem[] items;
    [SerializeField] private Transform cameraTransform;

    private Vector3 _lastCameraPosition;

    private void Start()
    {
        _lastCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 positionDelta = cameraTransform.position - _lastCameraPosition;
        foreach (var item in items)
        {
            float moveX = positionDelta.x * item.speedFactor;
            float moveY = positionDelta.y * item.speedFactor;
            item.transform.position += new Vector3(moveX, moveY, 0f);
        }

        _lastCameraPosition = cameraTransform.position;
    }
}
