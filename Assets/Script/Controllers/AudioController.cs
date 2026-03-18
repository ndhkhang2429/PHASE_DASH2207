using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [SerializeField] private AudioSource sfxAudioSource; // hieu ung
    [SerializeField] private AudioSource bgmAudioSource; // nhac nen

    [Header("Sound Effects")]
    [SerializeField] private List<AudioClip> buttonClickSFXList = new();

    [Header("Background Music")]
    public AudioClip menuBGM;
    public AudioClip inGameBGM;
    public AudioClip BossBGM;
    public AudioClip EndGameBGM;

    [Header("SFX Clips (Hiệu ứng)")]
    public AudioClip walkSound;
    public AudioClip dashSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip attackSound;
    public AudioClip hurtSound;

    private const string InGameSceneName = "Main";
    private const string BGMVolumeKey = "BGMVolume";
    private const string SFXVolumeKey = "SFXVolume";

    public float VolumeBGM => bgmAudioSource.volume;
    public float VolumeSFX => sfxAudioSource.volume;

    private void Awake()
    {
        if (Instance != null || Instance != this)
        {
            Destroy(Instance);
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

    public void PlayPlayerSFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }

    //chay Hieu ung
    private void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource == null || clip == null)
        {
            return;
        }

        sfxAudioSource.PlayOneShot(clip);
    }


    //chay BGM cho background
    public void PlayBGM(AudioClip clip)
    {
        if (bgmAudioSource == null || clip == null)
        {
            return;
        }
        bgmAudioSource.Stop();
        bgmAudioSource.loop = true;
        bgmAudioSource.clip = clip;
        bgmAudioSource.Play();
    }
}
