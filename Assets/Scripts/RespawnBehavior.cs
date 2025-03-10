using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnBehavior : MonoBehaviour
{
    public Transform respawnPoint; // Assign the respawn point in the Inspector
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) // Ensure the object is the player
        {
            Debug.Log("Player entered trigger!");
            col.transform.position = respawnPoint.transform.position;// Move the player to a new position

            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            Debug.Log("Player respawned via trigger!");
        }
    }
}
