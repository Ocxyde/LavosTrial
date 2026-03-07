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
//
// PlugInOutComplianceChecker.cs
// Scans C# files for plug-in-out compliance violations
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   Tools → Check Plug-in-Out Compliance
//   Reports violations in Console

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// PlugInOutComplianceChecker - Scans for architecture violations.
    /// Checks for forbidden patterns in Core scripts.
    /// </summary>
    public class PlugInOutComplianceChecker : EditorWindow
    {
        // Patterns that violate plug-in-out architecture
        private static readonly string[] forbiddenPatterns = new[]
        {
            @"\.AddComponent\s*<",                              // gameObject.AddComponent<T>()
            @"new\s+GameObject\s*\(",                           // new GameObject()
            @"Object\.Instantiate\s*<",                         // Object.Instantiate<T>() (in Core, not Editor)
            @"GameObject\.CreatePrimitive\s*\(",                // GameObject.CreatePrimitive()
        };

        // Patterns for hardcoded values (simplified check)
        private static readonly string[] hardcodedPatterns = new[]
        {
            @"=\s*\d+\s*f\s*;",                                 // Hardcoded floats: = 1.5f;
            @"=\s*\d+\s*;",                                     // Hardcoded ints: = 10;
        };

        // Allowed files (Editor scripts can create objects)
        private static readonly string[] allowedFolders = new[]
        {
            "/Editor/",
            "/Setup/",
            "/Build/",
            "/Cleanup/"
        };

        private Vector2 scrollPosition;
        private List<Violation> violations = new List<Violation>();
        private bool showWarnings = true;
        private bool scanComplete;

        [MenuItem("Tools/Compliance/Check Plug-in-Out Compliance")]
        public static void CheckComplianceMenuItem()
        {
            var window = GetWindow<PlugInOutComplianceChecker>("Compliance Check");
            window.minSize = new Vector2(600, 400);
            window.ScanCompliance();
        }

        private class Violation
        {
            public string filePath;
            public int lineNumber;
            public string line;
            public string violationType;
            public bool isWarning;
        }

        private void ScanCompliance()
        {
            violations.Clear();
            scanComplete = false;

            Debug.Log("========================================");
            Debug.Log("  PLUG-IN-OUT COMPLIANCE CHECK");
            Debug.Log("========================================");

            string scriptsPath = "Assets/Scripts/Core";

            if (!Directory.Exists(scriptsPath))
            {
                Debug.LogError($"   Core scripts folder not found: {scriptsPath}");
                return;
            }

            string[] csFiles = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);

            Debug.Log($"  Scanning {csFiles.Length} files...");
            Debug.Log("");

            foreach (string file in csFiles)
            {
                ScanFile(file);
            }

            scanComplete = true;

            // Summary
            int errors = 0;
            int warnings = 0;
            foreach (var v in violations)
            {
                if (v.isWarning) warnings++;
                else errors++;
            }

            Debug.Log("========================================");
            Debug.Log($"  SCAN COMPLETE");
            Debug.Log($"  Errors: {errors}");
            Debug.Log($"  Warnings: {warnings}");
            Debug.Log("========================================");

            if (errors == 0)
            {
                Debug.Log("   FULLY COMPLIANT!");
            }
            else
            {
                Debug.LogError("   VIOLATIONS FOUND - Fix before commit!");
            }
        }

        private void ScanFile(string filePath)
        {
            // Skip Editor scripts (they can create objects)
            foreach (string allowed in allowedFolders)
            {
                if (filePath.Contains(allowed))
                    return;
            }

            string[] lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int lineNumber = i + 1;

                // Skip comments and strings
                if (line.Trim().StartsWith("//") || line.Trim().StartsWith("*"))
                    continue;

                // Check forbidden patterns
                foreach (string pattern in forbiddenPatterns)
                {
                    if (Regex.IsMatch(line, pattern))
                    {
                        violations.Add(new Violation
                        {
                            filePath = filePath,
                            lineNumber = lineNumber,
                            line = line.Trim(),
                            violationType = GetViolationTypeName(pattern),
                            isWarning = false
                        });

                        Debug.LogError($"  {filePath}({lineNumber}): {GetViolationTypeName(pattern)}");
                        Debug.LogError($"    {line.Trim()}");
                    }
                }

                // Check hardcoded values (warnings only)
                if (showWarnings)
                {
                    foreach (string pattern in hardcodedPatterns)
                    {
                        if (Regex.IsMatch(line, pattern) && !line.Contains("SerializeField"))
                        {
                            violations.Add(new Violation
                            {
                                filePath = filePath,
                                lineNumber = lineNumber,
                                line = line.Trim(),
                                violationType = "Hardcoded value",
                                isWarning = true
                            });
                        }
                    }
                }
            }
        }

        private string GetViolationTypeName(string pattern)
        {
            if (pattern.Contains("AddComponent"))
                return "VIOLATION: AddComponent() - Use FindFirstObjectByType()";
            if (pattern.Contains("new GameObject"))
                return "VIOLATION: new GameObject() - Find existing or use Editor tool";
            if (pattern.Contains("Instantiate"))
                return "VIOLATION: Object.Instantiate() - Use prefab reference";
            if (pattern.Contains("CreatePrimitive"))
                return "VIOLATION: GameObject.CreatePrimitive() - Use prefabs";
            return "VIOLATION";
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Plug-in-Out Compliance Checker", EditorStyles.boldLabel);

            GUILayout.Space(10);

            if (GUILayout.Button("Scan Now", GUILayout.Height(30)))
            {
                ScanCompliance();
            }

            GUILayout.Space(10);

            showWarnings = EditorGUILayout.Toggle("Show Hardcoded Value Warnings", showWarnings);

            GUILayout.Space(15);

            if (violations.Count > 0)
            {
                GUILayout.Label($"Found {violations.Count} violation(s):", EditorStyles.boldLabel);

                foreach (var v in violations)
                {
                    if (!showWarnings && v.isWarning)
                        continue;

                    string relativePath = v.filePath.Replace("Assets/", "");
                    string styleLabel = v.isWarning ? "Box" : "RedLabel";

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField(
                        $"{(v.isWarning ? "️" : "")} {v.violationType}",
                        EditorStyles.boldLabel
                    );
                    EditorGUILayout.LabelField($"File: {relativePath}({v.lineNumber})");
                    EditorGUILayout.TextField(v.line);
                    EditorGUILayout.EndVertical();
                }
            }
            else if (scanComplete)
            {
                GUILayout.Label(" No violations found!", EditorStyles.boldLabel);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
