using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public void OnAnimationFinished()
    {
        Destroy(gameObject);
    }

    // Dự phòng: Nếu quên đặt Event, nó sẽ tự xóa sau 1 giây
    private void Start()
    {
        Destroy(gameObject, 1.0f);
    }
}
