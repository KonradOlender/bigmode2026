using System.Collections.Generic;
using UnityEngine;

namespace Spline
{
    /// <summary>
    /// Sweeps a cross-section (a generated tube ring or a custom mesh profile) along a
    /// BezierSpline to build a track mesh, assigns it to a MeshFilter/MeshCollider, and
    /// applies the configured physics layer to the GameObject.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SplineMeshExtruder : MonoBehaviour
    {
        public enum CrossSectionMode
        {
            Tube,
            CustomMesh
        }

        private const int MinTubeSides = 3;
        private const int MinSegmentsPerUnit = 1;

        [Header("Spline Source")]
        [SerializeField] private BezierSpline spline;

        [Header("Cross Section")]
        [SerializeField] private CrossSectionMode crossSectionMode = CrossSectionMode.Tube;
        [SerializeField] private int tubeSides = 8;
        [SerializeField] private float tubeRadius = 1f;
        [SerializeField] private Mesh customCrossSectionMesh;
        [SerializeField] private Vector3 crossSectionRotationEuler;

        [Header("Extrusion")]
        [SerializeField] private int segmentsPerUnit = 2;

        [Header("Collider")]
        [SerializeField] private bool generateCollider = true;
        [SerializeField] private bool convexCollider = false;

        [Header("Layer")]
        [SerializeField] private LayerMask trackLayer;

        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private Mesh generatedMesh;
        private int lastRebuildHash;
        private bool hasWarnedMissingSpline;
        private bool hasWarnedMissingCrossSectionMesh;

        private void OnEnable()
        {
            CacheComponents();
            RebuildMesh();
        }

        private void OnValidate()
        {
            CacheComponents();

            int currentHash = ComputeConfigHash();
            if (currentHash == lastRebuildHash)
            {
                return;
            }

            RebuildMesh();
        }

        private void CacheComponents()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (generateCollider && meshCollider == null)
            {
                meshCollider = GetComponent<MeshCollider>();
            }
        }

        /// <summary>Rebuilds the extruded mesh, collider, and GameObject layer from the current spline and settings.</summary>
        public void RebuildMesh()
        {
            CacheComponents();

            if (spline == null || spline.PointCount < 2)
            {
                if (!hasWarnedMissingSpline)
                {
                    Debug.LogWarning("SplineMeshExtruder: no valid spline assigned, clearing mesh.", this);
                    hasWarnedMissingSpline = true;
                }

                ClearMesh();
                return;
            }

            hasWarnedMissingSpline = false;

            List<Vector3> profilePoints = BuildCrossSectionProfile();
            if (profilePoints.Count < 3)
            {
                ClearMesh();
                return;
            }

            Mesh mesh = BuildExtrudedMesh(profilePoints);

            if (generatedMesh != null)
            {
                DestroyImmediateIfEditorAsset(generatedMesh);
            }

            generatedMesh = mesh;
            meshFilter.sharedMesh = generatedMesh;

            ApplyCollider();
            ApplyTrackLayer();

            lastRebuildHash = ComputeConfigHash();
        }

        private List<Vector3> BuildCrossSectionProfile()
        {
            List<Vector3> profile;

            if (crossSectionMode == CrossSectionMode.CustomMesh)
            {
                if (customCrossSectionMesh != null)
                {
                    hasWarnedMissingCrossSectionMesh = false;
                    profile = new List<Vector3>(customCrossSectionMesh.vertices);
                }
                else
                {
                    if (!hasWarnedMissingCrossSectionMesh)
                    {
                        Debug.LogWarning("SplineMeshExtruder: CustomMesh mode selected but no cross section mesh assigned, falling back to generated tube.", this);
                        hasWarnedMissingCrossSectionMesh = true;
                    }

                    profile = BuildTubeProfile();
                }
            }
            else
            {
                profile = BuildTubeProfile();
            }

            ApplyCrossSectionRotation(profile);
            return profile;
        }

        /// <summary>Rotates every cross-section profile vertex by the configured Euler angles, in the profile's local space.</summary>
        private void ApplyCrossSectionRotation(List<Vector3> profile)
        {
            if (crossSectionRotationEuler == Vector3.zero)
            {
                return;
            }

            Quaternion rotation = Quaternion.Euler(crossSectionRotationEuler);
            for (int i = 0; i < profile.Count; i++)
            {
                profile[i] = rotation * profile[i];
            }
        }

        private List<Vector3> BuildTubeProfile()
        {
            int sides = Mathf.Max(MinTubeSides, tubeSides);
            List<Vector3> ring = new List<Vector3>(sides);

            for (int i = 0; i < sides; i++)
            {
                float angle = i / (float)sides * Mathf.PI * 2f;
                ring.Add(new Vector3(Mathf.Cos(angle) * tubeRadius, Mathf.Sin(angle) * tubeRadius, 0f));
            }

            return ring;
        }

        private Mesh BuildExtrudedMesh(List<Vector3> profilePoints)
        {
            float splineLength = spline.GetLength();
            int rings = Mathf.Max(2, Mathf.CeilToInt(splineLength * Mathf.Max(MinSegmentsPerUnit, segmentsPerUnit)) + 1);
            int profileCount = profilePoints.Count;

            List<Vector3> vertices = new List<Vector3>(rings * profileCount);
            List<Vector2> uvs = new List<Vector2>(rings * profileCount);
            List<int> triangles = new List<int>((rings - 1) * profileCount * 6);

            Vector3 previousUp = Vector3.up;

            for (int ring = 0; ring < rings; ring++)
            {
                float t = ring / (float)(rings - 1);
                float distance = t * splineLength;

                Vector3 worldPosition = spline.EvaluatePositionByDistance(distance);
                Vector3 worldTangent = spline.EvaluateTangentByDistance(distance);
                Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
                Vector3 forward = transform.InverseTransformDirection(worldTangent).normalized;

                Vector3 right = Vector3.Cross(previousUp, forward);
                if (right.sqrMagnitude < Mathf.Epsilon)
                {
                    right = Vector3.Cross(Vector3.up, forward);
                    if (right.sqrMagnitude < Mathf.Epsilon)
                    {
                        right = Vector3.right;
                    }
                }

                right.Normalize();
                Vector3 up = Vector3.Cross(forward, right).normalized;
                previousUp = up;

                for (int p = 0; p < profileCount; p++)
                {
                    Vector3 profilePoint = profilePoints[p];
                    Vector3 offset = right * profilePoint.x + up * profilePoint.y;
                    vertices.Add(localPosition + offset);
                    uvs.Add(new Vector2(p / (float)profileCount, t * splineLength));
                }

                if (ring > 0)
                {
                    int ringStart = ring * profileCount;
                    int previousRingStart = (ring - 1) * profileCount;

                    for (int p = 0; p < profileCount; p++)
                    {
                        int next = (p + 1) % profileCount;

                        int a = previousRingStart + p;
                        int b = previousRingStart + next;
                        int c = ringStart + p;
                        int d = ringStart + next;

                        triangles.Add(a);
                        triangles.Add(c);
                        triangles.Add(b);

                        triangles.Add(b);
                        triangles.Add(c);
                        triangles.Add(d);
                    }
                }
            }

            Mesh mesh = new Mesh
            {
                name = "SplineTrackMesh",
                indexFormat = vertices.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16
            };

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }

        private void ApplyCollider()
        {
            if (!generateCollider)
            {
                if (meshCollider != null)
                {
                    meshCollider.sharedMesh = null;
                }

                return;
            }

            if (meshCollider == null)
            {
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }

            meshCollider.convex = convexCollider;
            meshCollider.sharedMesh = generatedMesh;
        }

        private void ApplyTrackLayer()
        {
            int layerValue = trackLayer.value;
            if (layerValue == 0)
            {
                return;
            }

            int resolvedLayer = Mathf.RoundToInt(Mathf.Log(layerValue, 2));
            gameObject.layer = resolvedLayer;
        }

        private void ClearMesh()
        {
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = null;
            }

            if (meshCollider != null)
            {
                meshCollider.sharedMesh = null;
            }
        }

        private int ComputeConfigHash()
        {
            int hash = 17;
            hash = hash * 31 + (spline != null ? spline.GetInstanceID() : 0);
            hash = hash * 31 + crossSectionMode.GetHashCode();
            hash = hash * 31 + tubeSides;
            hash = hash * 31 + tubeRadius.GetHashCode();
            hash = hash * 31 + (customCrossSectionMesh != null ? customCrossSectionMesh.GetInstanceID() : 0);
            hash = hash * 31 + crossSectionRotationEuler.GetHashCode();
            hash = hash * 31 + segmentsPerUnit;
            hash = hash * 31 + generateCollider.GetHashCode();
            hash = hash * 31 + convexCollider.GetHashCode();
            hash = hash * 31 + trackLayer.value;
            hash = hash * 31 + (spline != null ? spline.GetLength().GetHashCode() : 0);
            return hash;
        }

        private static void DestroyImmediateIfEditorAsset(Mesh mesh)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorUtility.IsPersistent(mesh))
            {
                DestroyImmediate(mesh);
            }
#else
            Destroy(mesh);
#endif
        }
    }
}
