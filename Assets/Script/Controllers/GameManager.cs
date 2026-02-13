using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance; //signleton pattern
    
    public GameObject gameOverPanel;
    public GameObject gamePausedPanel;

    public bool IsGameOver { get; private set; }
    private bool IsGamePause = false;

    //Để mỗi lần vào GameScene nó tự reset.
    private void Start()
    {
        Time.timeScale = 1f;
        IsGameOver = false;
        IsGamePause = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gamePausedPanel != null)
            gamePausedPanel.SetActive(false);
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !IsGameOver)
        {
            if(IsGamePause)
            {
                UnpauseGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        IsGamePause = true;
        Time.timeScale = 0f;
        if (gamePausedPanel != null)
            gamePausedPanel.SetActive(true);

    }

    public void UnpauseGame()
    {
        IsGamePause = false;
        Time.timeScale = 1f;
        gamePausedPanel.SetActive(false);
    }

    //Singleton phai clear khi object bi destroy
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
        IsGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void OnClickQuitButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
