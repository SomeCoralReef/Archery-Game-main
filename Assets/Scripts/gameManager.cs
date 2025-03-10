using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;
    private Dictionary<int, string> playerInputs = new Dictionary<int, string>();

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

    public void SetPlayerInputs(Dictionary<int, string> inputs)
    {
        playerInputs = new Dictionary<int, string>(inputs);
    }

    public Dictionary<int,string> GetPlayerInputs()
    {
        return playerInputs;
    }

    public void SpawnPlayers()
    {
        Dictionary<int,string> playerInputs = GetPlayerInputs();
        int i = 0;
        foreach(var entry in playerInputs)
        {
            int playerID = entry.Key;
            string inputMethod = entry.Value;

            if(i>= spawnPoints.Length) break;

            GameObject player = Instantiate(playerPrefab, spawnPoints[i].position, Quaternion.identity);
            PlayerScript playerScript = player.GetComponent<PlayerScript>();

            playerScript.playerIDnumber = playerID;
            playerScript.SetInputMethod(inputMethod);

            Debug.Log("Player " + playerID + " has joined using " + inputMethod);
            i++;
        }
    }
}
