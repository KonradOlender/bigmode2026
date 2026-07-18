using UnityEngine;

namespace Spline
{
    /// <summary>
    /// Moves this transform along a BezierSpline at a constant world-space speed using
    /// arc-length sampling, optionally orienting to the spline's tangent direction.
    /// </summary>
    public class SplineFollower : MonoBehaviour
    {
        [Header("Spline Source")]
        [SerializeField] private BezierSpline spline;

        [Header("Movement")]
        [SerializeField] private float speed = 5f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool alignToTangent = true;

        public float DistanceTraveled { get; private set; }

        private void Update()
        {
            if (spline == null || spline.PointCount < 2)
            {
                return;
            }

            float splineLength = spline.GetLength();
            if (splineLength <= Mathf.Epsilon)
            {
                return;
            }

            DistanceTraveled += speed * Time.deltaTime;

            if (loop)
            {
                DistanceTraveled = Mathf.Repeat(DistanceTraveled, splineLength);
            }
            else
            {
                DistanceTraveled = Mathf.Clamp(DistanceTraveled, 0f, splineLength);
            }

            transform.position = spline.EvaluatePositionByDistance(DistanceTraveled);

            if (alignToTangent)
            {
                Vector3 tangent = spline.EvaluateTangentByDistance(DistanceTraveled);
                if (tangent.sqrMagnitude > Mathf.Epsilon)
                {
                    transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
                }
            }
        }

        /// <summary>Resets the follower back to the start of the spline (distance 0).</summary>
        public void ResetToStart()
        {
            DistanceTraveled = 0f;

            if (spline != null && spline.PointCount >= 2)
            {
                transform.position = spline.EvaluatePositionByDistance(0f);
            }
        }
    }
}
