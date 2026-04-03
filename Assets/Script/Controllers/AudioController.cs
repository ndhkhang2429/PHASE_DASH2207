using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip buttonClickSFX;

    [Header("Background Music")]
    public AudioClip menuBGM;
    public AudioClip inGameBGM;
    public AudioClip BossBGM;
    public AudioClip EndGameBGM;
    public AudioClip WinGameBGM;

    [Header("FireWork")]
    public AudioClip fireWork;

    [Header("SFX Clips")]
    public AudioClip walkSound;
    public AudioClip dashSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip attackAirSound;
    public AudioClip[] comboSounds;
    public AudioClip hurtSound;

    private const string InGameSceneName = "Main";
    private const string BGMVolumeKey = "BGMVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private Coroutine fadeCoroutine;

    public float VolumeBGM => bgmAudioSource.volume;
    public float VolumeSFX => sfxAudioSource.volume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PlayBGM(menuBGM);
    }

    private void LoadSettings()
    {
        var bgmVolume = PlayerPrefs.GetFloat(BGMVolumeKey, 1f);
        SetVolume(bgmAudioSource, bgmVolume);

        var sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
        SetVolume(sfxAudioSource, sfxVolume);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.Equals(scene.name, InGameSceneName))
        {
            PlayBGM(inGameBGM);
        }
    }

    public void SetVolumeBGM(float volume)
    {
        SetVolume(bgmAudioSource, volume);
        PlayerPrefs.SetFloat(BGMVolumeKey, volume);
    }

    public void PlayButtonSFX()
    {
        if (buttonClickSFX != null)
        {
            PlaySFX(buttonClickSFX, 1f);
        }
    }

    public void SetVolumeSFX(float volume)
    {
        SetVolume(sfxAudioSource, volume);
        PlayerPrefs.SetFloat(SFXVolumeKey, volume);
    }

    private void SetVolume(AudioSource audioSource, float value)
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.volume = value;
    }

    public void PlaySFX(AudioClip clip, float pitch = 1f)
    {
        if (sfxAudioSource == null || clip == null) return;
        sfxAudioSource.pitch = pitch;
        sfxAudioSource.PlayOneShot(clip);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmAudioSource == null || clip == null)
        {
            return;
        }

        if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying) return;
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeBGM(clip, 1f));
    }
    private System.Collections.IEnumerator FadeBGM(AudioClip newClip, float fadeDuration)
    {
        float startVolume = bgmAudioSource.volume;
        float targetVolume = PlayerPrefs.GetFloat(BGMVolumeKey, 1f); // Lấy âm lượng gốc

        // 1. Fade Out (Nhỏ dần nhạc cũ)
        if (bgmAudioSource.isPlaying)
        {
            float timer = 0f;
            while (timer < fadeDuration / 2f)
            {
                timer += Time.deltaTime;
                bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, timer / (fadeDuration / 2f));
                yield return null;
            }
        }

        // 2. Đổi nhạc
        bgmAudioSource.Stop();
        bgmAudioSource.clip = newClip;
        bgmAudioSource.loop = true;
        bgmAudioSource.Play();

        // 3. Fade In (To dần nhạc mới lên bằng với âm lượng setting)
        float timerIn = 0f;
        while (timerIn < fadeDuration / 2f)
        {
            timerIn += Time.deltaTime;
            bgmAudioSource.volume = Mathf.Lerp(0f, targetVolume, timerIn / (fadeDuration / 2f));
            yield return null;
        }

        bgmAudioSource.volume = targetVolume; // Đảm bảo chốt lại đúng âm lượng
    }
}