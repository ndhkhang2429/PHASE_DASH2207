using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioEnemyController : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Common Sounds")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip dieSound;

    [Header("Action Sounds")]
    public AudioClip attackSound;
    [SerializeField] private AudioClip spotSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // Thiết lập mặc định để âm thanh nghe hay hơn trong game 2D
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Âm thanh 3D
    }

    public void PlayCustom(AudioClip clip, float basePitch = 1f, float randomRange = 0.1f)
    {
        if (clip == null || audioSource == null) return;

        // Tính toán Pitch: Lấy basePitch cộng thêm một khoảng ngẫu nhiên nhỏ
        float finalPitch = basePitch + Random.Range(-randomRange, randomRange);

        audioSource.pitch = finalPitch;
        audioSource.PlayOneShot(clip);
    }
    public void PlayHurt() => PlayCustom(hurtSound, 1f, 0.15f);
    public void PlayDie() => PlayCustom(dieSound, 0.9f, 0.05f); // Chết thì ít random hơn cho nghiêm túc
    public void PlayAttack() => PlayCustom(attackSound, 1f, 0.1f);
    public void PlaySpot() => PlayCustom(spotSound, 1f, 0.1f);
}