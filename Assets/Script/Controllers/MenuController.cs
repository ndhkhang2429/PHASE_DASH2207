using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void OnClickPlayButton()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }
}
