using System;
using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerScript : MonoBehaviour
{
    private ArcheryInputs inputs;
    
    //This needs to be set up dynamically : i.e the playerIDnumber; 
    [Header("Player Set Up")]
    public int playerIDnumber;
    
    

    [Header("PlayerSetup")]
    public float speed;
    public float jumpForce;
    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;
    private int lastDashInputDirection;
    
    [Header("Arrow Variables")]
    [SerializeField]
    private int maxNumberofArrows = 3;

    [SerializeField]
    private int currentNumberofArrows = 2;

   [Header("Arrow Variables")]
    public float dashSpeed = 10f;
    public float doubleTapTime;
    private float lastTapTime;
    private KeyCode lastKeyCode;
    public float dashCooldown = 1.0f;
    private float lastDashTime;
    private int onDashDir;

    [Header("Aiming Renderer")]
    public LineRenderer circleRenderer;
    public LineRenderer directionRenderer;
    public int circleSegments = 50;
    public float circleRadius = 3.0f;
    public float reducedcircleRadius = 1.5f;
    private bool jumpRequest = false;
    private int dashDirection = 0;

    
    [Header("Sub-Aiming Renderer")]
    public Transform aimingCircle1;
    public Transform aimingCircle2;
    private float holdTime = 0f;
    public float holdTimeToMaxAccuracy = 2.0f;  // Time in seconds to reach maximum accuracy
    public float maxAimingDistance = 1.5f;  // Max distance the aiming circles can be apart
    public GameObject arrowPrefab;

    private string inputMethod; //stores input type

    private void OnEnable()
    {
        inputs.Player.Enable();
    }
    
    private void OnDisable()
    {
        inputs.Player.Disable();
    }

    private void InitializeInputs()
    {
        inputs = new ArcheryInputs();
        inputs.Player.Move.performed += OnMove;
        inputs.Player.Move.canceled += OnMove;
        inputs.Player.Dash.started += context => onDashDir = context.ReadValue<float>() > 0 ? 1 : -1;
        inputs.Player.Dash.performed += OnDash;
        inputs.Player.Jump.performed += OnJump;
        inputs.Player.Shoot.canceled += OnFire;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupCircle();
        InitializeInputs();
    }


    void Start()
    {
       
        AssignPlayerLayer();
    }

    public void SetInputMethod(string inputMethod)
    {
       // if (playerInput != null)
       // {
       //     playerInput.SwitchCurrentControlScheme(inputMethod);
       //     Debug.Log("Player" + playerIDnumber + " has joined using " + inputMethod);
       // }
    }
    
    void AssignPlayerLayer()
    {
        string layerName = "Player" + playerIDnumber;
        int layerNumber = LayerMask.NameToLayer(layerName);

        if(layerNumber != -1)
        {
            gameObject.layer = layerNumber;
        }
        else 
        {
            Debug.LogError("Layer " + layerName + " does not exist!");
        }
    }
    
    void SetupCircle()
    {
        circleRenderer.positionCount = circleSegments + 1;
        circleRenderer.useWorldSpace = false;
        UpdateCircle();

        directionRenderer.positionCount = 2;
        directionRenderer.useWorldSpace = false;
    }

    void UpdateCircle()
    {
        float deltaTheta = (2f * Mathf.PI) / circleSegments;
        float theta = 0f;

        for (int i = 0; i < circleSegments + 1; i++)
        {
            float x = circleRadius * Mathf.Cos(theta);
            float y = circleRadius * Mathf.Sin(theta);

            Vector3 pos = new Vector3(x, y, 0);
            circleRenderer.SetPosition(i, pos);

            theta += deltaTheta;
        }
    }

    void UpdateDirection(Vector3 direction)
    {
        directionRenderer.SetPosition(0, Vector3.zero);
        directionRenderer.SetPosition(1, direction * circleRadius);
    }
    
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        // {
        //     jumpRequest = true;
        // }
        //
        // if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.A)) && (Time.time >= lastDashTime + dashCooldown))
        // {
        //     KeyCode currentKeyCode = Input.GetKeyDown(KeyCode.D) ? KeyCode.D : KeyCode.A;
        //     if (currentKeyCode == lastKeyCode && (Time.time - lastTapTime) < doubleTapTime)
        //     {
        //         dashDirection = (currentKeyCode == KeyCode.D ? 1 : -1);
        //     }
        //     lastTapTime = Time.time;
        //     lastKeyCode = currentKeyCode;
        // }

        // TODO: i woudnt use Camera.main, its better to directly ref the camera
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()); // Get mouse position (NEW INPUT SYSTEM)
        mousePos.z = 0;
        Vector3 direction = (mousePos - transform.position).normalized;

        if (inputs.Player.Shoot.inProgress)
        {
            holdTime += Time.deltaTime;
            circleRenderer.enabled = true;
            UpdateDirection(direction);
            directionRenderer.enabled = true;
            reducedcircleRadius = circleRadius - maxAimingDistance;
            float angleOffset = Mathf.Rad2Deg * (1f / reducedcircleRadius);  // angle in degrees
            
            // Base position directly in the direction of the mouse
            Vector3 basePosition = direction * reducedcircleRadius;
            
            // Rotated positions for aiming circles
            Vector3 initialPos1 = Quaternion.Euler(0, 0, angleOffset) * basePosition;
            Vector3 initialPos2 = Quaternion.Euler(0, 0, -angleOffset) * basePosition;
            
            // Calculate convergence towards the base position
            float convergence = Mathf.Lerp(0, 1, holdTime / holdTimeToMaxAccuracy);
            Vector3 finalPos1 = Vector3.Lerp(initialPos1, basePosition, convergence);
            Vector3 finalPos2 = Vector3.Lerp(initialPos2, basePosition, convergence);
            
            aimingCircle1.position = transform.position + finalPos1;
            aimingCircle2.position = transform.position + finalPos2;
        }
        else
        {
            holdTime = 0f;  // Reset hold time on mouse release
            circleRenderer.enabled = false;
            directionRenderer.enabled = false;
        }
        
        // TODO: old shoot
        // if(Input.GetMouseButtonUp(0) && currentNumberofArrows > 0)
        // {
        //     currentNumberofArrows--;
        //     Shoot(direction,playerIDnumber);
        // }
    }

    
    private void OnFire(InputAction.CallbackContext context)
    {
        if(currentNumberofArrows > 0)
        {
            Shoot2(playerIDnumber);
            currentNumberofArrows--;
        }
    }
    
    void Shoot2(int playerIDnumber)
    {
        // Get positions relative to the player
        Vector3 pos1 = aimingCircle1.position - transform.position;
        Vector3 pos2 = aimingCircle2.position - transform.position;

        // Generate a random factor between 0 and 1 to pick a direction between pos1 and pos2
        float randomFactor = UnityEngine.Random.Range(0f, 1f);
        Vector3 shootDirection = Vector3.Lerp(pos1, pos2, randomFactor);
        
        // Normalize to ensure consistent force is applied
        shootDirection.Normalize();
        //Set arrowLayerName to the ArrowX ID
        int arrowLayerNumber = playerIDnumber + 5;
        
        // Create the arrow and apply force
        GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        
        //assign the Arrow to the arrow
        arrow.layer = arrowLayerNumber;
        arrow.GetComponent<ArrowScript>().playerIDnumber = playerIDnumber;
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        arrowRb.AddForce(shootDirection * 25f, ForceMode2D.Impulse); // Use a constant force magnitude
    }

    void FixedUpdate()
    {
        Move();
        /*if (jumpRequest)
        {
            Jump();
            jumpRequest = false;
        }
        if (dashDirection != 0)
        {
            Dash(dashDirection);
            dashDirection = 0;
        }*/
    }
    
    // handling arrow up
    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Player" + playerIDnumber + " collided with " + col.name);
        if(col.CompareTag("Arrow") && (currentNumberofArrows < maxNumberofArrows))
        {
            HandleArrowPickUp(col.gameObject);
        }
    }

    void HandleArrowPickUp(GameObject arrow)
    {
        ArrowScript arrowScript = arrow.GetComponent<ArrowScript>();
        if(arrowScript.isStuck == true)
        {
            Destroy(arrow);
            currentNumberofArrows++;
        }
    }
    
    private void Move()
    {
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (context.interaction is not MultiTapInteraction) return;
        var currentDashInputDir = onDashDir;
        if (lastDashInputDirection == currentDashInputDir)
        {
            Dash(currentDashInputDir);
            Debug.Log("Dash");
        }
        lastDashInputDirection = currentDashInputDir;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
    }

    public void TakeDamage()
    {
        Invoke("Die", 3f);
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.right * 10f, ForceMode2D.Impulse);
    }
    
    private void Shoot()
    {
        GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
        arrowRb.AddForce(Vector2.right * 20f, ForceMode2D.Impulse); // Example direction
    }

    private void Dash(int direction)
    {
        Debug.Log("Dash fired with direction: " + direction);
        Vector2 dashForce = new Vector2(dashSpeed * direction, 0);
        rb.AddForce(dashForce, ForceMode2D.Impulse);
        lastDashTime = Time.time;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
        // Check if the collision normal indicates that the player is landing on a horizontal surface
            foreach (ContactPoint2D contact in col.contacts)
            {
                if (contact.normal.y > 0.5f) // Adjust this threshold if needed
                {
                    isGrounded = true;
                    return;
                }
            }
            foreach (ContactPoint2D contact in col.contacts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.red, 1f);
            }
        }
    }


    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
