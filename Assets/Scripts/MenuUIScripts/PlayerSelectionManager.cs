using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerSelectionManager : MonoBehaviour
{
    public static PlayerSelectionManager instance;
    private Dictionary<int,string> playerInputs = new Dictionary<int, string>();
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

        string controlType = playerInput.currentControlScheme;
        playerInputs[playerID] = controlType;

        Debug.Log("Player " + playerID + " has joined using " + controlType);
    }


    public void StartGame()
    {
        if(playerInputs.Count > 1)
        {
            gameManager.instance.SetPlayerInputs(playerInputs);
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.Log("Need at least 2 players to start the game");
        }
    }

}

