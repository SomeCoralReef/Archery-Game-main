using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ShowNumberOfPlayersConnected : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int numberOfPlayersConnected = MainMenuManager.Instance.playerCount;
       
    }

    // Update is called once per frame
    void Update()
    {
         // Get the number of players connected from the server
        // For now, let's just set it to 2
        
        GetComponent<TextMeshProUGUI>().text = "Number of players connected: " + MainMenuManager.Instance.playerCount;
    }
}
