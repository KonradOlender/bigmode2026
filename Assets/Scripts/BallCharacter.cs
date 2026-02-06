using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BallCharacter : MonoBehaviour
{
    public Gamemanager gamemanager;
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

    [Header("Reset")]
    [SerializeField] private Animator animator;
    [SerializeField] public float isBoostingCooldown;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isBoosting;

    private string currentAnim = "";

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
        Anims();
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

        gamemanager.setSpeedText(rb.linearVelocity.magnitude);
        //Debug.Log(rb.linearVelocity.magnitude);
        //Debug.Log(rb.linearVelocity);

        Vector3 moveDirection = cameraRight * moveInput.x;
        //cameraForward * moveInput.y +

        if(camera.fieldOfView >= minFOV && camera.fieldOfView <= maxFOV)
        {
            camera.fieldOfView = 60 + rb.linearVelocity.magnitude;
        }
        

        if (rb.linearVelocity.magnitude < maxSpeed && playerInControl && isGrounded)
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

    public void AddForcePreGame(float force)
    {
        isBoosting = true;
        rb.AddForce(Vector3.back * force, ForceMode.Impulse);
        ChangeAnimation("Boots");
        StartCoroutine(BoostingCooldown());
    }

    public void Anims()
    {
        if (!isBoosting)
        {
            if (moveInput.x > 0)
            {
                //animator.SetBool("Reset", false);
                //animator.SetBool("Left", false);
                //animator.SetBool("Right", true);
                ChangeAnimation("TurnRight");
            }
            else if (moveInput.x < 0)
            {
                //animator.SetBool("Reset", false);
                //animator.SetBool("Right", false);
                //animator.SetBool("Left", true);
                ChangeAnimation("TurnLeft");
            }
            else
            {
                //animator.SetBool("Left", false);
                //animator.SetBool("Right", false);
                //animator.SetBool("Reset", true);
                ChangeAnimation("Idle");
            }
        }
    }

    private void ChangeAnimation(string name)
    {
        if(currentAnim != name)
        {
            currentAnim = name;
            animator.CrossFade(name, 0.2f);
        }
    }

    public void AddBoost(float boostForce)
    {
        isBoosting = true;
        rb.AddForce(rb.linearVelocity.normalized * boostForce, ForceMode.Impulse);
        ChangeAnimation("Boots");
        StartCoroutine(BoostingCooldown());
    }

    IEnumerator BoostingCooldown()
    {
        yield return new WaitForSeconds(isBoostingCooldown);
        isBoosting = false;
    }
}
