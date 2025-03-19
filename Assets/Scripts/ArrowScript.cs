using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class ArrowScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasHit = false;
    public bool isStuck = false;
    private GameObject shooter;

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
    public void SetShooter(GameObject shooter)
    {
        this.shooter = shooter;
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), shooter.GetComponent<Collider2D>(),true);
        Invoke("EnableCollision", 0.1f);
    }

    private void EnableCollision()
    {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), shooter.GetComponent<Collider2D>(),false);
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

            StickArrow(angle,collision);
            Debug.DrawRay(collision.contacts[0].point, collision.contacts[0].normal, Color.red, 5);
        } else if(!hasHit && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit player");
            hasHit = true;
            Vector2 lastVelocity = rb.velocity;
            float angle = Mathf.Atan2(lastVelocity.y, lastVelocity.x) * Mathf.Rad2Deg;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.isKinematic = true;

            Vector2 hitDirection = lastVelocity.normalized*-1;
            collision.gameObject.GetComponent<PlayerScript>().TakeDamage(hitDirection);
            Destroy(this.gameObject);
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
