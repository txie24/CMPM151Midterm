using UnityEngine;
using UnityEngine.InputSystem; // 1. Required for the New Input System

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airMultiplier = 0.4f;
    private float currentSpeed;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpCooldown = 0.25f;
    private bool readyToJump = true;

    [Header("Ground Check")]
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;
    private float playerHeight = 2f;

    [Header("Camera Look")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 0.1f; // New input system values are larger, lower this slightly
    [SerializeField] private float maxLookAngle = 85f;
    private float xRotation = 0f;

    // References & Inputs
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintPressed;
    private Vector3 moveDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        // Ground Check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, (playerHeight * 0.5f) + 0.2f, whatIsGround);

        // Sprint toggle
        currentSpeed = sprintPressed && isGrounded ? sprintSpeed : moveSpeed;

        // Drag Control
        rb.linearDamping = isGrounded ? groundDrag : 0f;

        // Camera Look
        HandleLook();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        SpeedControl();
    }

    // 2. NEW INPUT SYSTEM METHODS (Hook these up via Player Input Component events or automatically)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    public void OnSprint(InputValue value)
    {
        sprintPressed = value.isPressed;
    }

    private void HandleLook()
    {
        if (playerCamera == null) return;

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void MovePlayer()
    {
        // Use moveInput.y for forward/backward and moveInput.x for strafing
        moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

        if (isGrounded)
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > currentSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}