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
    public AudioClip spotSound;

    [Header("Boss Special Sounds")]
    [SerializeField] private AudioClip roarSound; 
    [SerializeField] private AudioClip lightningZapSound;
    [SerializeField] private AudioClip spawnSound; // Tiếng mở cổng gọi đệ (SFX)

    [Header("Boss Voice Lines (Giọng nói)")]
    [SerializeField] private AudioClip[] voiceLines; // Kéo 9 file âm thanh của bạn vào đây
    [Range(0f, 1f)][SerializeField] private float voiceChance = 0.4f;

    [Header("Charger Special")]
    public AudioClip windupSound;

    [Header("Shield Special")]
    public AudioClip blockSound; // Tiếng Keng kim loại

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

    //boss
    public void PlayRandomVoiceLine()
    {
        // 1. Kiểm tra xem có file nào trong mảng không
        // 2. Random.value trả về số từ 0.0 đến 1.0. Nếu nhỏ hơn voiceChance (0.4) thì mới nói.
        if (voiceLines != null && voiceLines.Length > 0 && Random.value <= voiceChance)
        {
            // Bốc ngẫu nhiên 1 file trong 9 file
            AudioClip randomVoice = voiceLines[Random.Range(0, voiceLines.Length)];

            // Phát âm thanh. Lưu ý: Giọng nói thì KHÔNG NÊN random Pitch để tránh bị méo giọng
            PlayCustom(randomVoice, 1f, 0f);
        }
    }

    //enemy base
    public void PlayHurt() => PlayCustom(hurtSound, 1f, 0.15f);
    public void PlayDie() => PlayCustom(dieSound, 0.9f, 0.05f); // Chết thì ít random hơn cho nghiêm túc
    public void PlayAttack() => PlayCustom(attackSound, 1f, 0.1f);
    public void PlaySpot() => PlayCustom(spotSound, 1f, 0.1f);
    public void PlaySpawn() => PlayCustom(spawnSound, 1f, 0.1f);

    //Charger(hyena)
    public void PlayWindup() => PlayCustom(windupSound, 1f, 0.05f);

    //Shield
    public void PlayBlock() => PlayCustom(blockSound, 1.2f, 0.2f);

    //Boss
    public void PlayRoar() => PlayCustom(roarSound, 1f, 0.05f);
    public void PlayLightning(float customPitch = 1f)
    {
        if (lightningZapSound != null)
        {
            PlayCustom(lightningZapSound, customPitch, 0f);
        }
    }
}