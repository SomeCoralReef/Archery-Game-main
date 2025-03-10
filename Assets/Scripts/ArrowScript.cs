using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasHit = false;

    public bool isStuck = false;

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
            hasHit = true;
            Vector2 lastVelocity = rb.velocity;
            float angle = Mathf.Atan2(lastVelocity.y, lastVelocity.x) * Mathf.Rad2Deg;
            
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.isKinematic = true;

            GroundController groundController = collision.gameObject.GetComponent<GroundController>();
            if (groundController != null)
            {
                groundController.AttachArrow(transform);
            }
            StickArrow(angle,collision);
        } else if(!hasHit && collision.gameObject.layer == LayerMask.NameToLayer("Player 2"))
        {
            hasHit = true;
            Vector2 lastVelocity = rb.velocity;
            float angle = Mathf.Atan2(lastVelocity.y, lastVelocity.x) * Mathf.Rad2Deg;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.isKinematic = true;

        }
    }

    private void StickArrow(float angle, Collision2D collision)
    {
        // Stick the arrow to the object it hit
        transform.localScale = startingScale;

        transform.rotation = Quaternion.Euler(0,0,angle);

        transform.parent = collision.transform;
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
