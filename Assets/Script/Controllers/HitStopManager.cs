using System.Collections;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Stop(float duration)
    {
        StartCoroutine(DoStop(duration));
    }

    IEnumerator DoStop(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}
