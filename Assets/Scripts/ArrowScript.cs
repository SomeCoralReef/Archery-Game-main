using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class ArrowScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasHit = false;
    
    public bool isStuck = false;
    [SerializeField]public int playerIDnumber;

    Vector3 startingScale;
    // Start is called before the first frame update
    void Start()
    {
        //string arrowLayerName = "Arrow" + playerIDnumber;
        //arrow.layer = LayerMask.NameToLayer(arrowLayerName);
        rb = GetComponent<Rigidbody2D>();
        hasHit = false;
        startingScale = transform.localScale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(!hasHit && collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Arrow hit the ground");
            hasHit = true;
            Vector2 lastVelocity = rb.velocity;
            float angle = Mathf.Atan2(lastVelocity.y, lastVelocity.x) * Mathf.Rad2Deg;
            
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.isKinematic = true;

            /*GroundController groundController = collision.gameObject.GetComponent<GroundController>();
            if (groundController != null)
            {
                groundController.AttachArrow(transform);
            }*/
            StickArrow(angle,collision);
            Debug.DrawRay(collision.contacts[0].point, collision.contacts[0].normal, Color.red, 5);
        } else if(!hasHit && collision.gameObject.CompareTag("Player") && collision.gameObject.layer != gameObject.layer)
        {
            hasHit = true;
            Vector2 lastVelocity = rb.velocity;
            float angle = Mathf.Atan2(lastVelocity.y, lastVelocity.x) * Mathf.Rad2Deg;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.isKinematic = true;
            collision.gameObject.GetComponent<PlayerScript>().TakeDamage();
            Debug.Log("Arrow hit the player");
        }
    }

    private void StickArrow(float angle, Collision2D collision)
    {
        // Stick the arrow to the object it hit
        transform.localScale = startingScale;
        transform.rotation = Quaternion.Euler(0,0,angle);
        isStuck = true;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        // Check if the arrow is moving
        if (!hasHit && rb.velocity != Vector2.zero)
        {
            // Update rotation to match the direction of velocity
            Vector2 v = GetComponent<Rigidbody2D>().velocity;
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
