using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("UI sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    void Start()
    {
        if (bgmSource != null && bgmSlider != null)
        {
            bgmSlider.value = bgmSource.volume;
        }

        if (sfxSource != null && sfxSlider != null)
        {
            sfxSlider.value = sfxSource.volume;
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
            bgmSource.volume = volume;
    }
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
            sfxSource.volume = volume;
    }
}
