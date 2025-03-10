using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public string currentMapName; // The current map's name
    public GameObject playerPrefab; // The player prefab
    public GameObject player2Prefab; // The second player prefab
    public int numberOfPlayers;  
    public Transform[] spawnPoints; // Array of spawn points

    public UnityEvent eventOne;
    // Start is called before the first frame update
    void Start()
    {
        numberOfPlayers = PlayerPrefs.GetInt("PlayerCount"); // Get the number of players from PlayerPrefs
        SpawnPlayers(numberOfPlayers); // Call the SpawnPlayers method
        Debug.Log("Number of players: " + numberOfPlayers);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnPlayers(int playerCount)
    {
        for (int i = 0; i < playerCount; i++)
        {
            GameObject player = Instantiate(playerPrefab, spawnPoints[i].position, Quaternion.identity); // Spawn a player at the spawn point
            player.name = "Player" + (i + 1); // Set the player's name
        }
    }
}
