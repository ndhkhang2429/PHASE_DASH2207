using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("UI sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Start()
    {
        if (bgmSlider != null)
        {
            bgmSlider.value = AudioController.Instance.VolumeBGM;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioController.Instance.VolumeSFX;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetBGMVolume(float volume)
    {
        AudioController.Instance.SetVolumeBGM(volume);
    }

    public void SetSFXVolume(float volume)
    {
        AudioController.Instance.SetVolumeSFX(volume);
    }
}