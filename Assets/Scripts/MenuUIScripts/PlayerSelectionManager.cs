using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerSelectionManager : MonoBehaviour
{
    public static PlayerSelectionManager instance;
    private int maxPlayers = 4;
    private bool[] playerJoined = new bool[4];
    private int nextAvailableID =1;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if(nextAvailableID > maxPlayers) return;

        int playerID = nextAvailableID;
        nextAvailableID++;
    }


    public void StartGame()
    {
        if(gameManager.instance.playerCount > 1)
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.Log("Need at least 2 players to start the game");
        }
    }

}

