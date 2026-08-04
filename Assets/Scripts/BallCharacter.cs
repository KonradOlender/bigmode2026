using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


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
    [SerializeField] private bool FOVChangeEnable;
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

    [Header("Gravity")]
    [SerializeField] public float groundedGravity = 1f;
    [SerializeField] public float airGravity = 20f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded = false;
    private bool wasGrounded = false;
    private bool isBoosting;

    private string currentAnim = "";

    private Vector3 contactPoint;
    private Vector3 contactNormal;

    private SphereCollider sphere;
    private Vector3 sphereCenter;
    private Vector3 radiusDirection;

    private Vector3 down;
    private Vector3 up;
    private Vector3 right;
    private Vector3 forward;

    private Vector3 forwardRight;
    private Vector3 forwardLeft;
    private Vector3 backRight;
    private Vector3 backLeft;

    private Vector3 tangent;
    private Vector3 mytarget;


    private void OnCollisionStay(Collision collision)
    {
        ContactPoint contact = collision.GetContact(0);

        contactPoint = contact.point;

        //Debug.Log(contact.point);
        //Debug.Log(contact.normal);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckDistance);
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sphere = GetComponent<SphereCollider>();

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
    private void Update()
    {
        if (!gamemanager.isPreGame)
        {
            CheckGroundStatus();
        }
    }
    private void FixedUpdate()
    {
        Anims();
        ApplyMovement();
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        rb.useGravity = false;

        float gravity = isGrounded ? groundedGravity : airGravity;

        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    

    private void CheckGroundStatus()
    {        
        wasGrounded = isGrounded;

        isGrounded = Physics.CheckSphere(
            transform.position,
            groundCheckDistance,
            groundLayer
        );

        if (isGrounded != wasGrounded)
        {
            OnGroundedChanged(isGrounded);
        }

        sphereCenter = transform.TransformPoint(sphere.center);
        radiusDirection = (contactPoint - sphereCenter).normalized;
        down = radiusDirection;

        right = Vector3.Cross(down, cameraTransform.forward).normalized;

        // od œrodka kuli
        float length = 2f;
        Debug.DrawRay(sphereCenter, cameraTransform.forward * length, Color.blue);
        Debug.DrawRay(sphereCenter, radiusDirection * length, Color.green);
        Debug.DrawRay(sphereCenter, right * length, Color.red);

        DrawPoint(contactPoint, 0.1f, Color.red);
        DrawPoint(sphereCenter, 0.1f, Color.red);
    }

    public void OnGroundedChanged(bool grounded)
    {
        if (grounded)
        {
            Soundmanager.Instance.RideSoundPlay();
        }
        else if (!grounded)
        {
            Debug.Log("lose contact with ground");
            Soundmanager.Instance.RideEndPlay();
            Soundmanager.Instance.RideSoundStop();
        }
    }

    public static void DrawPoint(Vector3 position, float size, Color color)
    {
        Debug.DrawLine(position - Vector3.right * size, position + Vector3.right * size, color);
        Debug.DrawLine(position - Vector3.up * size, position + Vector3.up * size, color);
        Debug.DrawLine(position - Vector3.forward * size, position + Vector3.forward * size, color);
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

        //Debug.Log(rb.linearVelocity.magnitude);
        //Debug.Log(rb.linearVelocity);

        Vector3 moveDirection = moveInput.x * cameraRight;

        if(camera.fieldOfView >= minFOV && camera.fieldOfView <= maxFOV && FOVChangeEnable)
        {
            camera.fieldOfView = 60 + rb.linearVelocity.magnitude;
        }
        

        if (rb.linearVelocity.magnitude < maxSpeed && playerInControl && isGrounded)
        {
            rb.AddForce(moveDirection * moveForce, ForceMode.Force);
        }

        //GUI
        gamemanager.setSpeedText(rb.linearVelocity.magnitude);
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
