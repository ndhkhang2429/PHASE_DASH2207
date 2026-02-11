using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; //signleton pattern
    public bool IsGameOver { get; private set; }

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
    }
}
