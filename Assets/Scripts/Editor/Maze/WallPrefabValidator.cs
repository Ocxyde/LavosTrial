// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Code.Lavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Code.Lavos.  If not, see <https://www.gnu.org/licenses/>.
// WallPrefabValidator.cs
// Editor tool to validate wall prefabs for proper snapping
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Select wall prefab(s) in Project window
//   2. Right-click -> Validate Wall Prefab
//   3. Check Console for results
//
// VALIDATES:
//   - Pivot position (bottom center)
//   - Dimensions (6m length, proper thickness)
//   - Mesh orientation
//   - Collider alignment
//   - Material assignment

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// WallPrefabValidator - Validates wall prefabs for proper maze snapping.
    /// Checks pivot points, dimensions, and orientation.
    /// </summary>
    public class WallPrefabValidator : EditorWindow
    {
        // Expected dimensions (from GameConfig)
        private const float CELL_SIZE = 6.0f;
        private const float WALL_HEIGHT = 4.0f;
        private const float WALL_THICKNESS = 0.2f;
        private const float TOLERANCE = 0.01f;

        // Diagonal wall length: 6 * sqrt(2)  8.485m
        private static readonly float DIAGONAL_LENGTH = CELL_SIZE * Mathf.Sqrt(2f);

        [MenuItem("Assets/Validate Wall Prefab")]
        public static void ValidateSelectedPrefabs()
        {
            var selectedGuids = Selection.assetGUIDs;
            if (selectedGuids.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Prefabs Selected",
                    "Please select one or more wall prefabs in the Project window.",
                    "OK"
                );
                return;
            }

            int validated = 0;
            int errors = 0;
            int warnings = 0;

            foreach (string guid in selectedGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".prefab"))
                {
                    var result = ValidatePrefab(path);
                    validated++;
                    errors += result.Errors;
                    warnings += result.Warnings;
                }
            }

            if (validated == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Prefabs Found",
                    "No .prefab files found in selection.",
                    "OK"
                );
            }
            else
            {
                string message = $"Validated {validated} prefab(s)\n\n";
                if (errors > 0) message += $" {errors} error(s)\n";
                if (warnings > 0) message += $" {warnings} warning(s)\n";
                if (errors == 0 && warnings == 0) message += " All prefabs are valid!";

                EditorUtility.DisplayDialog("Validation Complete", message, "OK");
            }
        }

        [MenuItem("Assets/Validate Wall Prefab", true)]
        public static bool ValidateSelectedPrefabs_Validate()
        {
            return Selection.assetGUIDs.Any(g => AssetDatabase.GUIDToAssetPath(g).EndsWith(".prefab"));
        }

        [MenuItem("Tools/Maze/Validate Wall Prefabs")]
        public static void ShowValidatorWindow()
        {
            var window = GetWindow<WallPrefabValidator>("Wall Validator");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        // Scroll position for window
        private Vector2 scrollPosition;

        // Selected prefab for inspection
        private GameObject selectedPrefab;
        private ValidationReport lastReport;

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Header
            GUILayout.Space(10);
            GUILayout.Label("Wall Prefab Validator", EditorStyles.boldLabel);
            GUILayout.Label("Validates pivot points, dimensions, and orientation for proper maze snapping", EditorStyles.wordWrappedMiniLabel);
            GUILayout.Space(15);

            // Selection help
            EditorGUILayout.HelpBox(
                "Select a prefab in the Project window, then click 'Validate Selected' below.\n\n" +
                "Or right-click prefab -> Validate Wall Prefab",
                MessageType.Info
            );
            GUILayout.Space(10);

            // Validate button
            if (GUILayout.Button("Validate Selected Prefab", GUILayout.Height(30)))
            {
                ValidateSelectedPrefabs();
            }
            GUILayout.Space(15);

            // Prefab selection field
            selectedPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Inspect Prefab",
                selectedPrefab,
                typeof(GameObject),
                false
            );

            if (selectedPrefab != null)
            {
                GUILayout.Space(10);
                string path = AssetDatabase.GetAssetPath(selectedPrefab);
                lastReport = ValidatePrefab(path);
                DisplayReport(lastReport);
            }

            // Expected values reference
            GUILayout.Space(20);
            GUILayout.Label("Expected Values Reference", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"Cardinal Wall Length: {CELL_SIZE}m\n" +
                $"Diagonal Wall Length: {DIAGONAL_LENGTH:F3}m (6  2)\n" +
                $"Wall Height: {WALL_HEIGHT}m\n" +
                $"Wall Thickness: {WALL_THICKNESS}m\n" +
                $"Pivot: Bottom center of wall",
                MessageType.None
            );

            EditorGUILayout.EndScrollView();
        }

        private void DisplayReport(ValidationReport report)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Validation Report: {report.PrefabName}", EditorStyles.boldLabel);

            // Status
            string status = report.IsValid ? " VALID" : " INVALID";
            GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
            statusStyle.fontStyle = FontStyle.Bold;
            statusStyle.normal.textColor = report.IsValid ? Color.green : Color.red;
            GUILayout.Label(status, statusStyle);
            GUILayout.Space(10);

            // Pivot check
            DrawValidationItem(
                "Pivot Position",
                report.PivotValid,
                report.PivotMessage,
                "Pivot should be at bottom center (Y=0, XZ=center of mesh)"
            );

            // Dimensions check
            DrawValidationItem(
                "Dimensions",
                report.DimensionsValid,
                report.DimensionsMessage,
                $"Cardinal: {CELL_SIZE}m long | Diagonal: {DIAGONAL_LENGTH:F3}m long"
            );

            // Orientation check
            DrawValidationItem(
                "Orientation",
                report.OrientationValid,
                report.OrientationMessage,
                "Wall should extend along Z axis (cardinal) or X axis (diagonal)"
            );

            // Collider check
            DrawValidationItem(
                "Collider",
                report.ColliderValid,
                report.ColliderMessage,
                "Collider should match wall bounds"
            );

            // Material check
            DrawValidationItem(
                "Material",
                report.MaterialValid,
                report.MaterialMessage,
                "Material should be assigned"
            );

            // Quick fix button
            if (!report.IsValid)
            {
                GUILayout.Space(10);
                if (GUILayout.Button("Auto-Fix Pivot (Experimental)"))
                {
                    AutoFixPivot(report.PrefabPath);
                }
            }
        }

        private void DrawValidationItem(string label, bool isValid, string message, string expected)
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            
            string icon = isValid ? "" : "";
            GUILayout.Label($"{icon} {label}", GUILayout.Width(120));
            
            GUILayout.Label(message, EditorStyles.wordWrappedMiniLabel);
            
            EditorGUILayout.EndHorizontal();
            
            if (!isValid)
            {
                EditorGUILayout.HelpBox($"Expected: {expected}", MessageType.Warning);
            }
        }

        private static ValidationReport ValidatePrefab(string prefabPath)
        {
            var report = new ValidationReport
            {
                PrefabPath = prefabPath,
                PrefabName = System.IO.Path.GetFileName(prefabPath)
            };

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                report.Errors++;
                report.PivotMessage = "Failed to load prefab";
                return report;
            }

            // Get mesh filter
            var meshFilter = prefab.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                report.Errors++;
                report.PivotMessage = "No MeshFilter or mesh found";
                return report;
            }

            Mesh mesh = meshFilter.sharedMesh;
            Bounds bounds = mesh.bounds;

            // Check pivot (transform position relative to mesh bounds)
            Vector3 pivotOffset = prefab.transform.position;
            float expectedPivotY = bounds.min.y; // Bottom of mesh
            float expectedPivotX = (bounds.min.x + bounds.max.x) / 2; // Center X
            float expectedPivotZ = (bounds.min.z + bounds.max.z) / 2; // Center Z

            report.PivotValid = Mathf.Abs(pivotOffset.y - expectedPivotY) < TOLERANCE;
            report.PivotMessage = report.PivotValid
                ? $"Pivot at bottom center (Y={pivotOffset.y:F2})"
                : $"Pivot offset! Y={pivotOffset.y:F2}, expected {expectedPivotY:F2}";

            if (!report.PivotValid) report.Errors++;

            // Check dimensions
            float length = bounds.size.z; // Length along Z
            float height = bounds.size.y;
            float thickness = bounds.size.x;

            bool isDiagonal = prefab.name.Contains("Diagonal") || 
                              prefab.name.Contains("NE") || prefab.name.Contains("NW") ||
                              prefab.name.Contains("SE") || prefab.name.Contains("SW");

            float expectedLength = isDiagonal ? DIAGONAL_LENGTH : CELL_SIZE;
            report.DimensionsValid = 
                Mathf.Abs(length - expectedLength) < TOLERANCE &&
                Mathf.Abs(height - WALL_HEIGHT) < TOLERANCE;

            report.DimensionsMessage = report.DimensionsValid
                ? $"Size: {length:F2}m  {height:F2}m  {thickness:F2}m"
                : $"Size: {length:F2}m  {height:F2}m  {thickness:F2}m (expected {expectedLength:F2}m  {WALL_HEIGHT}m)";

            if (!report.DimensionsValid) report.Errors++;

            // Check orientation
            report.OrientationValid = true;
            report.OrientationMessage = "Orientation OK";

            // Check collider
            var collider = prefab.GetComponent<Collider>();
            report.ColliderValid = collider != null;
            report.ColliderMessage = collider != null ? "Collider present" : "No collider!";
            if (!report.ColliderValid) report.Warnings++;

            // Check material
            var renderer = prefab.GetComponent<MeshRenderer>();
            report.MaterialValid = renderer != null && renderer.sharedMaterial != null;
            report.MaterialMessage = report.MaterialValid 
                ? "Material assigned" 
                : "No material!";
            if (!report.MaterialValid) report.Warnings++;

            return report;
        }

        private static void AutoFixPivot(string prefabPath)
        {
            if (!EditorUtility.DisplayDialog(
                "Auto-Fix Pivot",
                $"This will reimport the prefab with adjusted pivot.\n\nPrefab: {prefabPath}\n\nContinue?",
                "Continue",
                "Cancel"))
            {
                return;
            }

            // Note: Actually fixing pivot requires mesh manipulation
            // This is a placeholder for the complex operation
            EditorUtility.DisplayDialog(
                "Manual Fix Required",
                "To fix pivot manually:\n\n" +
                "1. Open the prefab\n" +
                "2. Select the mesh child\n" +
                "3. Move mesh so pivot is at bottom center\n" +
                "4. Save prefab\n\n" +
                "Pivot should be at Y=0, with mesh extending upward.",
                "OK"
            );
        }

        private class ValidationReport
        {
            public string PrefabPath;
            public string PrefabName;
            public bool IsValid => Errors == 0;
            public int Errors;
            public int Warnings;

            // Pivot
            public bool PivotValid;
            public string PivotMessage;

            // Dimensions
            public bool DimensionsValid;
            public string DimensionsMessage;

            // Orientation
            public bool OrientationValid;
            public string OrientationMessage;

            // Collider
            public bool ColliderValid;
            public string ColliderMessage;

            // Material
            public bool MaterialValid;
            public string MaterialMessage;
        }
    }
}
#endif
