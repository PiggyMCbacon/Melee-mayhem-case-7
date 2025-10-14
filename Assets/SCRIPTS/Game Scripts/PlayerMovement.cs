using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float acceleration = 10f;
    public float airControl = 0.5f;
    public float jumpForce = 6f;
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;

    [Header("Mouse Look")]
    public Transform cameraHolder;
    public float mouseSensitivity = 100f;
    public float lookLimit = 80f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 moveVelocity;
    private bool isGrounded;
    private float rotationX = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovementInput();
        HandleJump();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -lookLimit, lookLimit);

        // Rotate camera pitch
        cameraHolder.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        // Rotate player yaw
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovementInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.forward * v + transform.right * h).normalized;
        moveInput = moveDir * moveSpeed;
    }

    void MovePlayer()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        float control = isGrounded ? 1f : airControl;
        Vector3 targetVelocity = new Vector3(moveInput.x, rb.linearVelocity.y, moveInput.z);
        Vector3 velocity = Vector3.Lerp(rb.linearVelocity, transform.TransformDirection(targetVelocity), Time.fixedDeltaTime * acceleration * control);
        rb.linearVelocity = velocity;
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}