using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; //signleton pattern
    public bool IsGameOver { get; private set; }
    public GameObject gameOverPanel;

    //Để mỗi lần vào GameScene nó tự reset.
    private void Start()
    {
        Time.timeScale = 1f;
    }

    private void Awake()
    {
        //dam bao chi 1 gamemanager ton tai, neu ton tai 2 cai, se huy cai t2
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GameOver()
    {
        IsGameOver = true;
        Debug.Log("GAME OVER");
        Time.timeScale = 0f;

        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    //khi nhan vat chet nhan vao restart se choi lai
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void OnClickQuitButton()
    {
        SceneManager.LoadScene("Menu");
    }
}
