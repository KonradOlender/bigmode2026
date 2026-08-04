using UnityEngine;

public class Follow : MonoBehaviour
{
    public GameObject followTarget;
    public Vector3 offset;

    public bool isOnCamera;

    [Header("Rotation Settings")]
    [SerializeField] private bool rotateToMovementDirection = true;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float minimumVelocityThreshold = 0.1f;
    [SerializeField] float directionSmooth = 5f;

    [Header("TreePointPlane Settings")]
    [SerializeField] private bool TreePointPlaneEnabled = false;
    [SerializeField] private Vector3 point1Offset;
    [SerializeField] private Vector3 point2Offset;
    [SerializeField] private Vector3 point3Offset;
    [SerializeField] private float rayLength;
    [SerializeField] private float treePointRotationSpeed = 5f;
    [SerializeField] private LayerMask groundMask;

    private Vector3 point1;
    private Vector3 point2;
    private Vector3 point3;

    private Rigidbody targetRigidbody;

    private Vector3 smoothedDirection;
    

    

    private void OnDrawGizmos()
    {
        if (followTarget == null)
            return;

        Vector3 center = followTarget.transform.position + offset;

        Vector3 p1 = center + point1Offset;
        Vector3 p2 = center + point2Offset;
        Vector3 p3 = center + point3Offset;

        // Punkty kontrolne
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(p1, 0.08f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(p2, 0.08f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(p3, 0.08f);

        // Raycasty
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p1, p1 + Vector3.down * rayLength);
        Gizmos.DrawLine(p2, p2 + Vector3.down * rayLength);
        Gizmos.DrawLine(p3, p3 + Vector3.down * rayLength);

#if UNITY_EDITOR
        // Jeœli raycast trafia, poka¿ punkt trafienia i normaln¹.
        DrawHit(p1);
        DrawHit(p2);
        DrawHit(p3);
#endif
    }

#if UNITY_EDITOR
    private void DrawHit(Vector3 origin)
    {
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength, groundMask))
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(hit.point, 0.06f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.4f);
        }
    }
#endif

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

        if (isOnCamera)
        {
            transform.position = followTarget.transform.position + offset;

            if (rotateToMovementDirection && targetRigidbody != null)
            {
                RotateToVelocity();
            }
        }
        else
        {
            if (TreePointPlaneEnabled)
            {
                TreePointPlane();
            }
        }
    }
    
    private void RotateToVelocity()
    {
        Vector3 velocity = targetRigidbody.linearVelocity;
        velocity.y = 0f;

        if (velocity.sqrMagnitude > minimumVelocityThreshold * minimumVelocityThreshold)
        {
            Vector3 targetDirection = velocity.normalized;

            smoothedDirection = Vector3.Lerp(
                smoothedDirection,
                targetDirection,
                directionSmooth * Time.deltaTime
            );
        }

        if (smoothedDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(
            smoothedDirection,
            Vector3.up
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void TreePointPlane()
    {
        transform.position = followTarget.transform.position + offset;

        if (!Physics.Raycast(transform.position + point1Offset, Vector3.down, out RaycastHit hit1, rayLength, groundMask) ||
            !Physics.Raycast(transform.position + point2Offset, Vector3.down, out RaycastHit hit2, rayLength, groundMask) ||
            !Physics.Raycast(transform.position + point3Offset, Vector3.down, out RaycastHit hit3, rayLength, groundMask))
        {
            return;
        }

        Vector3 normal = Vector3.Cross(
            hit2.point - hit1.point,
            hit3.point - hit1.point
        ).normalized;

        if (Vector3.Dot(normal, Vector3.up) < 0)
            normal = -normal;

        Vector3 forward;

        if (targetRigidbody.linearVelocity.sqrMagnitude > 0.05f)
        {
            forward = Vector3.ProjectOnPlane(
                targetRigidbody.linearVelocity,
                normal
            ).normalized;
        }
        else
        {
            forward = Vector3.ProjectOnPlane(
                transform.forward,
                normal
            ).normalized;
        }

        Quaternion targetRotation = Quaternion.LookRotation(forward, normal);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            treePointRotationSpeed * Time.deltaTime
        );
    }


    private void OldRotateToVelocity()
    {
        Vector3 velocity = targetRigidbody.linearVelocity;
        //velocity.y = 0f;

        if (velocity.magnitude > minimumVelocityThreshold)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
