using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("Sound Effects")]
    [SerializeField] private List<AudioClip> buttonClickSFXList = new();

    [Header("Background Music")]
    public AudioClip menuBGM;
    public AudioClip inGameBGM;
    public AudioClip BossBGM;
    public AudioClip EndGameBGM;

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

    public void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource == null || clip == null) return;

        sfxAudioSource.PlayOneShot(clip);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmAudioSource == null || clip == null)
        {
            return;
        }

        if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying) return;

        bgmAudioSource.Stop();
        bgmAudioSource.loop = true;
        bgmAudioSource.clip = clip;
        bgmAudioSource.Play();
    }
}