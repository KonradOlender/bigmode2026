using UnityEngine;

public class Follow : MonoBehaviour
{
    public GameObject followTarget;
    public Vector3 offset;
    
    [Header("Rotation Settings")]
    [SerializeField] private bool rotateToMovementDirection = true;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float minimumVelocityThreshold = 0.1f;
    
    private Rigidbody targetRigidbody;
    
    void Start()
    {
        if (followTarget != null)
        {
            targetRigidbody = followTarget.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        if (followTarget == null) return;
        
        transform.position = followTarget.transform.position + offset;
        
        if (rotateToMovementDirection && targetRigidbody != null)
        {
            RotateToVelocity();
        }
    }
    
    private void RotateToVelocity()
    {
        Vector3 velocity = targetRigidbody.linearVelocity;
        velocity.y = 0f;
        
        if (velocity.magnitude > minimumVelocityThreshold)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
