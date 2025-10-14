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
    public float mouseSensitivity = 120f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;
    private float pitchRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Freeze X/Z rotation only, allow Yaw rotation
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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

        pitchRotation -= mouseY;
        pitchRotation = Mathf.Clamp(pitchRotation, -80f, 80f);

        //cameraHolder.localRotation = Quaternion.Euler(pitchRotation, 0f, 0f);
        cameraHolder.transform.localEulerAngles = new Vector3(pitchRotation,cameraHolder.transform.localEulerAngles.y+ mouseX,cameraHolder.transform.localEulerAngles.z);
        //transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovementInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Camera-relative movement
        Vector3 camForward = cameraHolder.forward;
        Vector3 camRight = cameraHolder.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * v + camRight * h).normalized;
        moveInput = moveDir * moveSpeed;
    }

    void MovePlayer()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        float control = isGrounded ? 1f : airControl;
        Vector3 targetVelocity = new Vector3(moveInput.x, rb.linearVelocity.y, moveInput.z);
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * acceleration * control);
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