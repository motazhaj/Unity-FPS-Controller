using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    private float currentSpeed;
    private float moveSpeed = 8f;
    public float walkSpeed = 3f;
    public float sprintSpeed = 15f;
    public float groundDrag = 1f;
    public float jumpForce = 35f;
    public float jumpCooldown = 0.2f;
    public float airControl = 0.5f;
    bool readyToJump;
    public float extraGravity = 40f;

    [Header("Crouch")]
    public float crouchYScale = 0.5f;
    float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode walkKey = KeyCode.LeftAlt;

    [Header("Grounded")]
    public float playerHeight = 2f;
    public LayerMask ground;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    bool exitSlope;
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        move,
        crouch,
        walking,
        sprinting,
        air
    }

    private void Awake()
    {
        moveSpeed = 8f;

        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = rb.transform.localScale.y;
    }

    private void Update()
    {
        // Ground Check
        grounded = Physics.Raycast(transform.position, Vector3.down, 0.5f * playerHeight + 0.2f, ground);

        PlayerInput();
        SpeedControl();
        StateHandler();

        // Drag
        if (OnSlope())
        {
            rb.drag = 5f;
        }
        else if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
        print(rb.velocity.magnitude);
    }

    private void FixedUpdate()
    {
        PlayerMove();
        
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && (grounded || OnSlope()))
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            Crouch();
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            ResetCrouch();
        }

    }

    private void StateHandler()
    {
        // Crouch
        if (grounded && Input.GetKey(crouchKey))
        {
            state = MovementState.crouch;
            currentSpeed = walkSpeed;
        }
        // Sprint
        else if (grounded && Input.GetKey(sprintKey)){
            state = MovementState.sprinting;
            currentSpeed = sprintSpeed;
        }
        // Walk
        else if (grounded && Input.GetKey(walkKey))
        {
            state = MovementState.walking;
            currentSpeed = walkSpeed;

        }
        // Move
        else if (grounded){
            state = MovementState.move;
            currentSpeed = moveSpeed;
        }
        // Air
        else
        {
            state = MovementState.air;
        }
        print(state);

    }

    private void PlayerMove()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (OnSlope())
        {

            rb.AddForce(GetSlopeMoveDirection() * currentSpeed * 10f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f * airControl, ForceMode.Force);
        }

        // Turn off gravity when on a slope
        rb.useGravity = !OnSlope();
        // Apply extra gravity to simualte weight
        if (!OnSlope() || !grounded)
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Force);
        }

        

    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (OnSlope() && !exitSlope)
        {
            if(rb.velocity.magnitude > currentSpeed)
            {
                rb.velocity = rb.velocity.normalized * currentSpeed;
            }
        }
        else if (flatVel.magnitude > currentSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }

    }

    private void Jump()
    {
        exitSlope = true;
        // Reset y vel
        rb.velocity = new Vector3 (rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        exitSlope = false;
        readyToJump = true;
    }

    private void Crouch()
    {
        transform.localScale = new Vector3(rb.transform.localScale.x, crouchYScale, rb.transform.localScale.z);
        rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
    }

    private void ResetCrouch()
    {
        transform.localScale = new Vector3(rb.transform.localScale.x, startYScale, rb.transform.localScale.z);

    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.3f))
        {
            float angle = Vector3.Angle(transform.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;

    }


}
