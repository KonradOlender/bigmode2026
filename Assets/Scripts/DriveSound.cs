using UnityEngine;
using AK.Wwise; // Ensure you have the Wwise namespace imported

public class DriveSound : MonoBehaviour
{
    public AK.Wwise.Event groundCollisionEvent; // Assign the Wwise event in the Inspector
    public AK.Wwise.Event groundExitEvent; // Assign the second Wwise event in the Inspector
    public AK.Wwise.RTPC speedRTPC; // Assign the RTPC for speed in the Inspector
    public LayerMask groundLayer; // Assign the Ground layer in the Inspector
    public float distanceTolerance = 0.5f; // Tolerance for proximity to the ground

    private bool isTouchingGround = false;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("DriveSound script requires a Rigidbody component on the same GameObject.");
        }
    }

    private void Update()
    {
        // Update the RTPC value based on the object's in-game speed
        if (rb != null)
        {
            float speed = rb.linearVelocity.magnitude; // Use velocity magnitude for in-game speed
            speedRTPC.SetValue(gameObject, speed);
        }

        // Check if the object is near the ground
        Collider[] colliders = Physics.OverlapSphere(transform.position, distanceTolerance, groundLayer);
        if (colliders.Length > 0 && !isTouchingGround)
        {
            // Trigger the Wwise event
            groundCollisionEvent.Post(gameObject);
            isTouchingGround = true;
        }
        else if (colliders.Length == 0 && isTouchingGround)
        {
            // Stop the Wwise event
            groundCollisionEvent.Stop(gameObject);

            // Trigger the second Wwise event
            groundExitEvent.Post(gameObject);

            isTouchingGround = false;
        }
     
    }
}
