using UnityEditor;
using UnityEngine;

namespace Spline.Editor
{
    /// <summary>
    /// Scene View authoring tool for BezierSpline. Draws draggable handles for points and
    /// tangents, supports inserting a point by Ctrl/Cmd-clicking near the curve, and exposes
    /// inspector buttons for common editing operations.
    /// </summary>
    [CustomEditor(typeof(BezierSpline))]
    public class BezierSplineEditor : UnityEditor.Editor
    {
        private const float PointHandleSize = 0.12f;
        private const float TangentHandleSize = 0.08f;
        private const float InsertClickDistanceThreshold = 0.5f;
        private const int CurveSampleResolution = 30;

        private static readonly Color PointColor = Color.white;
        private static readonly Color TangentColor = Color.yellow;
        private static readonly Color TangentLineColor = new Color(1f, 1f, 0f, 0.6f);
        private static readonly Color CurveColor = Color.green;
        private static readonly Color SelectedPointColor = Color.red;

        private int selectedPointIndex = -1;

        private void OnEnable()
        {
            selectedPointIndex = -1;
        }

        public override void OnInspectorGUI()
        {
            BezierSpline spline = (BezierSpline)target;

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spline Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Add Point at End"))
            {
                Undo.RecordObject(spline, "Add Spline Point");
                Vector3 newPointWorldPosition = spline.PointCount > 0
                    ? spline.GetPointWorld(spline.PointCount - 1) + spline.transform.forward
                    : spline.transform.position;
                spline.AddPoint(newPointWorldPosition);
                selectedPointIndex = spline.PointCount - 1;
                EditorUtility.SetDirty(spline);
            }

            using (new EditorGUI.DisabledScope(selectedPointIndex < 0 || selectedPointIndex >= spline.PointCount || spline.PointCount <= 2))
            {
                if (GUILayout.Button("Remove Selected Point"))
                {
                    Undo.RecordObject(spline, "Remove Spline Point");
                    spline.RemovePoint(selectedPointIndex);
                    selectedPointIndex = -1;
                    EditorUtility.SetDirty(spline);
                }
            }

            if (GUILayout.Button(spline.IsClosed ? "Open Loop" : "Close Loop"))
            {
                Undo.RecordObject(spline, "Toggle Spline Closed Loop");
                spline.IsClosed = !spline.IsClosed;
                EditorUtility.SetDirty(spline);
            }

            if (GUILayout.Button("Rebuild Length Table"))
            {
                spline.RebuildLengthTable();
            }

            EditorGUILayout.HelpBox("Ctrl/Cmd-click near the curve in the Scene View to insert a point. Drag white handles to move points, yellow handles to shape tangents.", MessageType.Info);
        }

        private void OnSceneGUI()
        {
            BezierSpline spline = (BezierSpline)target;

            if (spline.PointCount == 0)
            {
                return;
            }

            DrawCurve(spline);
            DrawPointHandles(spline);
            HandleInsertClick(spline);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(spline);
            }
        }

        private void DrawCurve(BezierSpline spline)
        {
            Handles.color = CurveColor;

            for (int i = 0; i < spline.PointCount; i++)
            {
                int nextIndex = i + 1;
                if (nextIndex >= spline.PointCount)
                {
                    if (!spline.IsClosed)
                    {
                        continue;
                    }

                    nextIndex = 0;
                }

                Handles.DrawBezier(
                    spline.GetPointWorld(i),
                    spline.GetPointWorld(nextIndex),
                    spline.GetOutTangentWorld(i),
                    spline.GetInTangentWorld(nextIndex),
                    CurveColor,
                    null,
                    2f);
            }
        }

        private void DrawPointHandles(BezierSpline spline)
        {
            for (int i = 0; i < spline.PointCount; i++)
            {
                Vector3 pointPosition = spline.GetPointWorld(i);
                Vector3 inTangentPosition = spline.GetInTangentWorld(i);
                Vector3 outTangentPosition = spline.GetOutTangentWorld(i);

                Handles.color = TangentLineColor;
                Handles.DrawLine(pointPosition, inTangentPosition);
                Handles.DrawLine(pointPosition, outTangentPosition);

                Handles.color = i == selectedPointIndex ? SelectedPointColor : PointColor;
                float pointSize = HandleUtility.GetHandleSize(pointPosition) * PointHandleSize;

                EditorGUI.BeginChangeCheck();
                Vector3 newPointPosition = Handles.FreeMoveHandle(pointPosition, pointSize, Vector3.zero, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spline, "Move Spline Point");
                    spline.SetPointPosition(i, newPointPosition);
                    selectedPointIndex = i;
                }

                if (Handles.Button(pointPosition, Quaternion.identity, 0f, pointSize * 1.5f, Handles.RectangleHandleCap))
                {
                    selectedPointIndex = i;
                }

                Handles.color = TangentColor;
                float tangentSize = HandleUtility.GetHandleSize(inTangentPosition) * TangentHandleSize;

                EditorGUI.BeginChangeCheck();
                Vector3 newInTangentPosition = Handles.FreeMoveHandle(inTangentPosition, tangentSize, Vector3.zero, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spline, "Move Spline In Tangent");
                    spline.SetInTangent(i, newInTangentPosition);
                }

                float outTangentSize = HandleUtility.GetHandleSize(outTangentPosition) * TangentHandleSize;

                EditorGUI.BeginChangeCheck();
                Vector3 newOutTangentPosition = Handles.FreeMoveHandle(outTangentPosition, outTangentSize, Vector3.zero, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spline, "Move Spline Out Tangent");
                    spline.SetOutTangent(i, newOutTangentPosition);
                }
            }
        }

        private void HandleInsertClick(BezierSpline spline)
        {
            Event currentEvent = Event.current;

            if (currentEvent == null || currentEvent.type != EventType.MouseDown || currentEvent.button != 0 || !currentEvent.control)
            {
                return;
            }

            int segmentCount = spline.IsClosed ? spline.PointCount : spline.PointCount - 1;
            if (segmentCount <= 0)
            {
                return;
            }

            float bestDistance = float.MaxValue;
            int bestSegmentIndex = -1;
            float bestT = 0f;

            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                for (int sample = 0; sample <= CurveSampleResolution; sample++)
                {
                    float localT = sample / (float)CurveSampleResolution;
                    float globalT = (segmentIndex + localT) / segmentCount;
                    Vector3 worldPoint = spline.EvaluatePosition(globalT);
                    Vector2 screenPoint = HandleUtility.WorldToGUIPoint(worldPoint);
                    float distance = Vector2.Distance(screenPoint, currentEvent.mousePosition);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestSegmentIndex = segmentIndex;
                        bestT = localT;
                    }
                }
            }

            if (bestSegmentIndex >= 0 && bestDistance <= HandleUtility.GetHandleSize(spline.transform.position) * InsertClickDistanceThreshold * 50f)
            {
                Undo.RecordObject(spline, "Insert Spline Point");
                spline.InsertPoint(bestSegmentIndex, bestT);
                currentEvent.Use();
            }
        }
    }
}
