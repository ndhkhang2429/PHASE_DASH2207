using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void OnClickPlayButton()
    {
        AudioController.Instance.PlayButtonSFX();
        SceneManager.LoadScene("Main");
    }

    public void OnClickQuitButton()
    {
        AudioController.Instance.PlayButtonSFX();
        Application.Quit();
    }
}
