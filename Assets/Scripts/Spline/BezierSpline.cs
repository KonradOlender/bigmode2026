using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spline
{
    /// <summary>
    /// Stores a sequence of cubic Bezier control points in local space and provides
    /// evaluation, editing, and constant-speed (arc-length) sampling utilities.
    /// Tangents are stored as local-space offsets relative to their owning point.
    /// </summary>
    public class BezierSpline : MonoBehaviour
    {
        private const int DefaultSamplesPerSegment = 20;
        private const float DefaultTangentOffset = 1f;

        [Serializable]
        public struct BezierPoint
        {
            public Vector3 position;
            public Vector3 inTangent;
            public Vector3 outTangent;
            public bool brokenTangents;
        }

        [SerializeField] private List<BezierPoint> points = new List<BezierPoint>();
        [SerializeField] private bool isClosed;

        private readonly List<float> lengthTableDistances = new List<float>();
        private readonly List<float> lengthTableParams = new List<float>();
        private float cachedLength;

        public bool IsClosed
        {
            get => isClosed;
            set
            {
                isClosed = value;
                RebuildLengthTable();
            }
        }

        public int PointCount => points.Count;

        private void OnValidate()
        {
            RebuildLengthTable();
        }

        /// <summary>Returns the number of segments, accounting for whether the spline is closed.</summary>
        private int GetSegmentCount()
        {
            if (points.Count < 2)
            {
                return 0;
            }

            return isClosed ? points.Count : points.Count - 1;
        }

        public Vector3 GetPointWorld(int index)
        {
            index = Mathf.Clamp(index, 0, points.Count - 1);
            return transform.TransformPoint(points[index].position);
        }

        public Vector3 GetInTangentWorld(int index)
        {
            index = Mathf.Clamp(index, 0, points.Count - 1);
            BezierPoint point = points[index];
            return transform.TransformPoint(point.position + point.inTangent);
        }

        public Vector3 GetOutTangentWorld(int index)
        {
            index = Mathf.Clamp(index, 0, points.Count - 1);
            BezierPoint point = points[index];
            return transform.TransformPoint(point.position + point.outTangent);
        }

        /// <summary>Appends a new point at the given world position with mirrored default tangents.</summary>
        public void AddPoint(Vector3 worldPosition)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            Vector3 tangentDirection = Vector3.forward * DefaultTangentOffset;

            if (points.Count > 0)
            {
                tangentDirection = (localPosition - points[points.Count - 1].position).normalized * DefaultTangentOffset;
                if (tangentDirection.sqrMagnitude < Mathf.Epsilon)
                {
                    tangentDirection = Vector3.forward * DefaultTangentOffset;
                }
            }

            points.Add(new BezierPoint
            {
                position = localPosition,
                inTangent = -tangentDirection,
                outTangent = tangentDirection,
                brokenTangents = false
            });

            RebuildLengthTable();
        }

        /// <summary>Splits the segment at normalized parameter t (0-1 within the segment) using De Casteljau subdivision.</summary>
        public void InsertPoint(int segmentIndex, float t)
        {
            int segmentCount = GetSegmentCount();
            if (segmentCount == 0)
            {
                return;
            }

            segmentIndex = Mathf.Clamp(segmentIndex, 0, segmentCount - 1);
            t = Mathf.Clamp01(t);

            int nextIndex = (segmentIndex + 1) % points.Count;

            BezierPoint startPoint = points[segmentIndex];
            BezierPoint endPoint = points[nextIndex];

            Vector3 p0 = startPoint.position;
            Vector3 p1 = startPoint.position + startPoint.outTangent;
            Vector3 p2 = endPoint.position + endPoint.inTangent;
            Vector3 p3 = endPoint.position;

            Vector3 a = Vector3.Lerp(p0, p1, t);
            Vector3 b = Vector3.Lerp(p1, p2, t);
            Vector3 c = Vector3.Lerp(p2, p3, t);
            Vector3 d = Vector3.Lerp(a, b, t);
            Vector3 e = Vector3.Lerp(b, c, t);
            Vector3 newPointPosition = Vector3.Lerp(d, e, t);

            startPoint.outTangent = a - p0;
            endPoint.inTangent = c - p3;

            BezierPoint newPoint = new BezierPoint
            {
                position = newPointPosition,
                inTangent = d - newPointPosition,
                outTangent = e - newPointPosition,
                brokenTangents = false
            };

            points[segmentIndex] = startPoint;
            points[nextIndex] = endPoint;
            points.Insert(segmentIndex + 1, newPoint);

            RebuildLengthTable();
        }

        public void RemovePoint(int index)
        {
            if (points.Count <= 2)
            {
                Debug.LogWarning("BezierSpline: cannot remove point, at least 2 points are required.");
                return;
            }

            index = Mathf.Clamp(index, 0, points.Count - 1);
            points.RemoveAt(index);
            RebuildLengthTable();
        }

        public void SetPointPosition(int index, Vector3 worldPosition)
        {
            index = Mathf.Clamp(index, 0, points.Count - 1);
            BezierPoint point = points[index];
            point.position = transform.InverseTransformPoint(worldPosition);
            points[index] = point;
            RebuildLengthTable();
        }

        public void SetInTangent(int index, Vector3 worldPosition)
        {
            index = Mathf.Clamp(index, 0, points.Count - 1);
            BezierPoint point = points[index];
            point.inTangent = transform.InverseTransformPoint(worldPosition) - point.position;

            if (!point.brokenTangents)
            {
                point.outTangent = -point.inTangent;
            }

            points[index] = point;
            RebuildLengthTable();
        }

        public void SetOutTangent(int index, Vector3 worldPosition)
        {
            index = Mathf.Clamp(index, 0, points.Count - 1);
            BezierPoint point = points[index];
            point.outTangent = transform.InverseTransformPoint(worldPosition) - point.position;

            if (!point.brokenTangents)
            {
                point.inTangent = -point.outTangent;
            }

            points[index] = point;
            RebuildLengthTable();
        }

        public void SetBrokenTangents(int index, bool broken)
        {
            index = Mathf.Clamp(index, 0, points.Count - 1);
            BezierPoint point = points[index];
            point.brokenTangents = broken;
            points[index] = point;
        }

        /// <summary>Evaluates the world-space position at normalized parameter t (0-1) across the whole spline.</summary>
        public Vector3 EvaluatePosition(float t)
        {
            int segmentCount = GetSegmentCount();
            if (segmentCount == 0)
            {
                return transform.position;
            }

            GetSegmentAndLocalT(t, segmentCount, out int segmentIndex, out float localT);
            return transform.TransformPoint(EvaluateSegmentPosition(segmentIndex, localT));
        }

        /// <summary>Evaluates the world-space tangent direction at normalized parameter t (0-1) across the whole spline.</summary>
        public Vector3 EvaluateTangent(float t)
        {
            int segmentCount = GetSegmentCount();
            if (segmentCount == 0)
            {
                return transform.forward;
            }

            GetSegmentAndLocalT(t, segmentCount, out int segmentIndex, out float localT);
            Vector3 localTangent = EvaluateSegmentTangent(segmentIndex, localT);
            return transform.TransformDirection(localTangent).normalized;
        }

        /// <summary>Evaluates the world-space position at the given arc-length distance using the cached length table.</summary>
        public Vector3 EvaluatePositionByDistance(float distance)
        {
            float t = DistanceToT(distance);
            return EvaluatePosition(t);
        }

        /// <summary>Evaluates the world-space tangent at the given arc-length distance using the cached length table.</summary>
        public Vector3 EvaluateTangentByDistance(float distance)
        {
            float t = DistanceToT(distance);
            return EvaluateTangent(t);
        }

        public float GetLength()
        {
            return cachedLength;
        }

        /// <summary>Rebuilds the arc-length lookup table by sampling every segment at fixed steps.</summary>
        public void RebuildLengthTable(int samplesPerSegment = DefaultSamplesPerSegment)
        {
            lengthTableDistances.Clear();
            lengthTableParams.Clear();
            cachedLength = 0f;

            int segmentCount = GetSegmentCount();
            if (segmentCount == 0)
            {
                return;
            }

            samplesPerSegment = Mathf.Max(1, samplesPerSegment);
            int totalSamples = segmentCount * samplesPerSegment;

            Vector3 previousPoint = EvaluatePosition(0f);
            lengthTableDistances.Add(0f);
            lengthTableParams.Add(0f);

            for (int i = 1; i <= totalSamples; i++)
            {
                float t = i / (float)totalSamples;
                Vector3 currentPoint = EvaluatePosition(t);
                cachedLength += Vector3.Distance(previousPoint, currentPoint);
                lengthTableDistances.Add(cachedLength);
                lengthTableParams.Add(t);
                previousPoint = currentPoint;
            }
        }

        private float DistanceToT(float distance)
        {
            if (lengthTableDistances.Count == 0)
            {
                RebuildLengthTable();
            }

            if (lengthTableDistances.Count == 0)
            {
                return 0f;
            }

            distance = Mathf.Clamp(distance, 0f, cachedLength);

            int low = 0;
            int high = lengthTableDistances.Count - 1;

            while (low < high)
            {
                int mid = (low + high) / 2;
                if (lengthTableDistances[mid] < distance)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }

            if (low == 0)
            {
                return lengthTableParams[0];
            }

            float distanceBefore = lengthTableDistances[low - 1];
            float distanceAfter = lengthTableDistances[low];
            float segmentSpan = distanceAfter - distanceBefore;
            float lerp = segmentSpan > Mathf.Epsilon ? (distance - distanceBefore) / segmentSpan : 0f;

            return Mathf.Lerp(lengthTableParams[low - 1], lengthTableParams[low], lerp);
        }

        private void GetSegmentAndLocalT(float t, int segmentCount, out int segmentIndex, out float localT)
        {
            t = Mathf.Clamp01(t);
            float scaledT = t * segmentCount;
            segmentIndex = Mathf.Clamp(Mathf.FloorToInt(scaledT), 0, segmentCount - 1);
            localT = scaledT - segmentIndex;

            if (segmentIndex == segmentCount - 1 && Mathf.Approximately(t, 1f))
            {
                localT = 1f;
            }
        }

        private Vector3 EvaluateSegmentPosition(int segmentIndex, float t)
        {
            GetSegmentControlPoints(segmentIndex, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);

            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * oneMinusT * p0
                   + 3f * oneMinusT * oneMinusT * t * p1
                   + 3f * oneMinusT * t * t * p2
                   + t * t * t * p3;
        }

        private Vector3 EvaluateSegmentTangent(int segmentIndex, float t)
        {
            GetSegmentControlPoints(segmentIndex, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);

            float oneMinusT = 1f - t;
            Vector3 derivative = 3f * oneMinusT * oneMinusT * (p1 - p0)
                                  + 6f * oneMinusT * t * (p2 - p1)
                                  + 3f * t * t * (p3 - p2);

            return derivative.sqrMagnitude > Mathf.Epsilon ? derivative.normalized : Vector3.forward;
        }

        private void GetSegmentControlPoints(int segmentIndex, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3)
        {
            int startIndex = segmentIndex;
            int endIndex = (segmentIndex + 1) % points.Count;

            BezierPoint startPoint = points[startIndex];
            BezierPoint endPoint = points[endIndex];

            p0 = startPoint.position;
            p1 = startPoint.position + startPoint.outTangent;
            p2 = endPoint.position + endPoint.inTangent;
            p3 = endPoint.position;
        }
    }
}
