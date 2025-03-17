using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;
    public int playerCount = 2;
    //private Dictionary<int, string> playerInputs = new Dictionary<int, string>();
    private List<PlayerScript> players = new List<PlayerScript>();
    public GameObject playerPrefab; // The player prefab
    public Transform [] spawnPoints;
    
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
    public void AddPlayer(PlayerScript player)
    {
        players.Add(player);
    }

    void Start()
    {
        SpawnPlayers();
    }
    public void SpawnPlayers()
    {
        //players.Clear(); // Clear any previous player data
        Debug.Log("Spawning " + playerCount + " players");
        for (int i = 0; i < playerCount; i++)
        {
            if (i >= spawnPoints.Length) 
            {
                Debug.LogError("Not enough spawn points for all players!");
                break;
            }

        GameObject player = Instantiate(playerPrefab, spawnPoints[i].position, Quaternion.identity);
        PlayerScript playerScript = player.GetComponent<PlayerScript>();

        playerScript.playerIDnumber = i + 1; // Assign ID based on spawn order
        AddPlayer(playerScript); // Register player in GameManager

        Debug.Log("Spawned Player " + playerScript.playerIDnumber);
        }
    }
}
