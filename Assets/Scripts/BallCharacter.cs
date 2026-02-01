using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BallCharacter : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private bool playerInControl = true;
    
    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Camera camera;
    [SerializeField] float minFOV;
    [SerializeField] float maxFOV;
    
    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Reset")]
    [SerializeField] private Transform resetPosition;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        playerInControl = true;


        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (camera == null)
        {
            camera = Camera.main;
        }
    }
    
    private void FixedUpdate()
    {
        CheckGroundStatus();
        ApplyMovement();
    }
    
    private void CheckGroundStatus()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }
    
    private void ApplyMovement()
    {
        if (cameraTransform == null) return;
        
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Debug.Log(rb.linearVelocity.magnitude);
        //Debug.Log(rb.linearVelocity);

        Vector3 moveDirection = cameraRight * moveInput.x;
        //cameraForward * moveInput.y +

        if(camera.fieldOfView >= minFOV && camera.fieldOfView < maxFOV)
        {
            camera.fieldOfView = 60 + rb.linearVelocity.magnitude;
        }
        

        if (rb.linearVelocity.magnitude < maxSpeed && playerInControl)
        {
            rb.AddForce(moveDirection * moveForce, ForceMode.Force);
        }
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        transform.position = resetPosition.position;
        rb.linearVelocity = new Vector3(0, 0, 0);
    }

    public void setPlayerInControl(bool value)
    {
        playerInControl = value;
    }
}
