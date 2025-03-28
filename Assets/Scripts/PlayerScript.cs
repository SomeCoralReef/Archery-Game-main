using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.Users;

using System.Text;

public class PlayerScript : MonoBehaviour
{
    private ArcheryInputs archeryInputs;
    private PlayerInput playerInput;
    
    [Header("References")]
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private BoxCollider2D wallCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("PlayerSetup")]
    public int playerIDnumber;
    [SerializeField] private float playerScale = 0.4f;
    public float speed;
    public float deceleration;
    public float jumpForce;
    public float jumpTimeLimit;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;
    private float jumpTimer;
    private bool isGrounded;
    private bool isJumping;
    private float moveInput;
    private int lastDashInputDirection;
    private bool ChargeMode = false;
    
    [Header("Arrow Variables")]
    [SerializeField] private int maxNumberofArrows = 3;
    public int currentNumberofArrows = 20;

    [Header("Sliding Variables")]
    private bool isTouchingWall;
    private bool isSlidingWall;
    [SerializeField] private float wallSlideSpeed;

   [Header("Arrow Variables")]
    public float dashSpeed = 10f;
    private float lastTapTime;
    private KeyCode lastKeyCode;
    public float dashCooldown = 1.0f;
    private float lastDashTime;
    private bool isDashing;
    public float dashTime = 0.1f;
    private int currentDashInputDir;

    [Header("Aiming Renderer")]
    public LineRenderer circleRenderer;
    public LineRenderer directionRenderer;
    public int circleSegments = 50;
    public float circleRadius = 3.0f;
    public float reducedcircleRadius = 1.5f;
    
    [Header("Sub-Aiming Renderer")]
    public Transform aimingCircle1;
    public Transform aimingCircle2;
    private float holdTime;
    public float holdTimeToMaxAccuracy = 2.0f;  // Time in seconds to reach maximum accuracy
    public float maxAimingDistance = 1.5f;  // Max distance the aiming circles can be apart
    public GameObject arrowPrefab;

    [Header("Sub-Aiming ChargeMode")]
    public float chargeTime = 4.0f;
    public GameObject ChargeUpUI;
    public GameObject ChargeThresholdUI;
    private float lowerChargeThreshold;
    private float upperChargeThreshold;

    private void OnEnable()
    {
        archeryInputs.Player.Enable();
    }
    
    private void OnDisable()
    {
        archeryInputs.Player.Disable();
    }

    private void InitializeInputs()
    {
        archeryInputs = new ArcheryInputs();
        playerInput = GetComponent<PlayerInput>();
        archeryInputs.asset.devices = playerInput.devices;
        archeryInputs.Enable();
        archeryInputs.Player.Move.performed += OnMove;
        archeryInputs.Player.Move.canceled += context => moveInput = 0;
        archeryInputs.Player.Dash.performed += OnDash;
        archeryInputs.Player.Jump.started += OnJump;
        archeryInputs.Player.Jump.canceled += context => isJumping = false;
        archeryInputs.Player.Shoot.canceled += OnFire;
        archeryInputs.Player.Aim.performed += context => {}; // Ensure Aim action is registered
        archeryInputs.Player.Charge.performed += OnCharge;
        archeryInputs.Player.Charge.canceled += OnChargeRelease;
    }
    

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        SetupCircle();
        InitializeInputs();
    }

    void Start()
    {
        AssignPlayerLayer();
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
        GroundCheck();
        WallCheck();
        
        Vector2 joystickInput = archeryInputs.Player.Aim.ReadValue<Vector2>();
        Vector3 direction;
        if (joystickInput.sqrMagnitude > 0.1f)  // Dead zone check to prevent unwanted movements
        {
            direction = new Vector3(joystickInput.x, joystickInput.y, 0).normalized;
        }
        else
        {
            // Default to mouse aiming when joystick is not in use
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePos.z = 0;
            direction = (mousePos - transform.position).normalized;
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            maxNumberofArrows = 20;
            currentNumberofArrows = 20;
        }

        if (archeryInputs.Player.Shoot.inProgress)
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

        if (archeryInputs.Player.Jump.inProgress && isJumping)
        {
            if (jumpTimer > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimer -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }
    }
    
    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.transform.position, groundCheck.size, 0, groundLayer);
    }
    
    private void WallCheck()
    {
        isTouchingWall = Physics2D.OverlapBox(wallCheck.transform.position, wallCheck.size, 0, groundLayer);
        if (isTouchingWall && !isGrounded && moveInput != 0)
        {
            isSlidingWall = true;
        }
        else
        {
            isSlidingWall = false;
        }
    }
    
    void FixedUpdate()
    {
        Move();
    }
    
    private void OnFire(InputAction.CallbackContext context)
    {
        if(currentNumberofArrows > 0)
        {
            DirectionalShoot(playerIDnumber);
            currentNumberofArrows--;
        }
    }
    
    void DirectionalShoot(int playerIDnumber)
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
        ArrowScript arrowScript = arrow.GetComponent<ArrowScript>();
        arrowScript.SetShooter(gameObject);
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        arrowRb.AddForce(shootDirection * 25f, ForceMode2D.Impulse); // Use a constant force magnitude
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
        animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("yVelocity", rb.velocity.y);
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(-Mathf.Sign(moveInput), 1, 1) * playerScale;
        }
        
        if (moveInput != 0)
        {
            rb.velocity = isDashing
                ? new Vector2(moveInput * dashSpeed, rb.velocity.y)
                : new Vector2(moveInput * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(0, rb.velocity.y), deceleration * Time.fixedDeltaTime);
        }

        if (isSlidingWall)
        {
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();
    }
    

    private void OnDash(InputAction.CallbackContext context)
    {
        if (lastDashTime + dashCooldown < Time.time)
        {
            Vector2 dashForce = new Vector2(dashSpeed * currentDashInputDir, 0);
            rb.AddForce(dashForce, ForceMode2D.Impulse);
            lastDashTime = Time.time;
            isDashing = true;
            lastDashTime = Time.time;
            Invoke(nameof(DashResetInvoke), dashTime);
        }
    }
    
    private void OnCharge(InputAction.CallbackContext context)
    {
        // TODO: Implement charge
        SetIntoChargeMode();
    }
    void SetIntoChargeMode()
    {
        ChargeMode = true;
        float ChargeUpUIScale = 0.5f;

        if(!ChargeUpUI.activeSelf)
        {
            ChargeUpUI.SetActive(true);
        }
        ChargeUpUI.transform.localScale = new Vector3(ChargeUpUIScale, ChargeUpUIScale, ChargeUpUIScale);
        ChargeUpUI.SetActive(true);


        float randomBetween = UnityEngine.Random.Range(0.2f,0.3f);
        ChargeThresholdUI.transform.localScale = new Vector3(randomBetween, randomBetween, randomBetween);
        lowerChargeThreshold = randomBetween -0.1f;
        upperChargeThreshold = randomBetween + 0.1f;
        ChargeThresholdUI.SetActive(true);
        StartCoroutine(ShrinkCircleOverTime(ChargeUpUI, chargeTime));
    }
    
    IEnumerator ShrinkCircleOverTime(GameObject circle, float duration)
    {
    Vector3 startScale = Vector3.one * 0.5f;
    Vector3 endScale = Vector3.zero;
    float elapsed = 0f;

    circle.transform.localScale = startScale;

    while (elapsed < duration)
    {
        float t = elapsed / duration;
        circle.transform.localScale = Vector3.Lerp(startScale, endScale, t);
        elapsed += Time.deltaTime;
        yield return null;
    }

    circle.transform.localScale = endScale;
    circle.SetActive(false);
    }

    private void OnChargeRelease(InputAction.CallbackContext context)
    {
        // TODO: Implement charge release
    }

    private void DashResetInvoke()
    {
        isDashing = false;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            animator.SetBool("isJumping", true);
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            jumpTimer = jumpTimeLimit;
            isJumping = true;
            rb.velocity = Vector2.up * jumpForce;
        }
    }

    public void TakeDamage(Vector2 hitDirection)
    {
        Invoke("Die", 3f);
        OnDisable();
        rb.velocity = Vector2.zero;
        rb.AddForce(-hitDirection.normalized * 5f, ForceMode2D.Impulse);
        sr.color = Color.red;
    }
    
    // handling arrow up
    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("Arrow") && (currentNumberofArrows < maxNumberofArrows))
        {
            HandleArrowPickUp(col.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // if (col.gameObject.CompareTag("Ground"))
        // {
        //     // Check if the collision normal indicates that the player is landing on a horizontal surface
        //     foreach (ContactPoint2D contact in col.contacts)
        //     {
        //
        //         if (contact.normal.y > 0.5f) // Adjust this threshold if needed
        //         {
        //             isGrounded = true;
        //             animator.SetBool("isJumping", false);
        //         } else if(Mathf.Abs(contact.normal.x)>0.5f)
        //         {
        //             isTouchingWall = true;
        //         }
        //     } 
        //     foreach (ContactPoint2D contact in col.contacts)
        //     {
        //         Debug.DrawRay(contact.point, contact.normal, Color.red, 1f);
        //     }
        // }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
