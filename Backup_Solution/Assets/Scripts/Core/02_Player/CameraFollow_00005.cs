// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
// CameraFollow.cs
// Third-person camera follow for maze exploration
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Smooth camera follow with adjustable distance
//
// SETUP:
//   1. Create empty GameObject as camera pivot
//   2. Attach this script
//   3. Assign target (player)
//   4. MainCamera will follow at specified distance

using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Lavos.Core
{
    /// <summary>
    /// CameraFollow - Smooth third-person camera follow.
    /// Perfect for maze exploration with adjustable view distance.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        #region Inspector Settings

        [Header("Target")]
        [Tooltip("Target to follow (player)")]
        [SerializeField] private Transform target;

        [Tooltip("Auto-find player on start")]
        [SerializeField] private bool autoFindTarget = true;

        [Header("Camera Settings")]
        [Tooltip("Distance from target (eye level)")]
        [SerializeField] private float distance = 3.5f;

        [Tooltip("Height offset from target position")]
        [SerializeField] private float heightOffset = 1.7f;

        [Tooltip("Camera smoothness (higher = faster)")]
        [SerializeField] private float smoothSpeed = 10f;

        [Tooltip("Look at target smoothing")]
        [SerializeField] private float lookSmoothness = 5f;

        [Header("Limits")]
        [Tooltip("Minimum distance clamp")]
        [SerializeField] private float minDistance = 1.5f;

        [Tooltip("Maximum distance clamp")]
        [SerializeField] private float maxDistance = 10f;

        [Tooltip("Minimum vertical angle")]
        [SerializeField] private float minVerticalAngle = -80f;

        [Tooltip("Maximum vertical angle")]
        [SerializeField] private float maxVerticalAngle = 80f;

        [Header("Mouse Control")]
        [Tooltip("Enable mouse look")]
        [SerializeField] private bool enableMouseLook = true;

        [Tooltip("Mouse sensitivity")]
        [SerializeField] private float mouseSensitivity = 2f;

        [Tooltip("Invert Y axis")]
        [SerializeField] private bool invertY = false;

        [Header("Debug")]
        [SerializeField] private bool showDebugUI = false;

        #endregion

        #region Private State

        private Camera _camera;
        private Vector3 _currentPosition;
        private Quaternion _currentRotation;
        private float _horizontalAngle = 0f;
        private float _verticalAngle = 10f;
        private Vector3 _targetPosition;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                _camera = GetComponentInChildren<Camera>();
                if (_camera == null)
                {
                    var cameraGO = new GameObject("Main Camera");
                    cameraGO.transform.SetParent(transform);
                    cameraGO.transform.localPosition = Vector3.zero;
                    _camera = cameraGO.AddComponent<Camera>();
                    _camera.tag = "MainCamera";
                }
            }
        }

        void Start()
        {
            if (autoFindTarget && target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                    Debug.Log("[CameraFollow] Auto-found player target");
                }
                else
                {
                    Debug.LogWarning("[CameraFollow] No player found! Assign target manually.");
                }
            }

            // Initialize camera position
            if (target != null)
            {
                transform.position = target.position;
                UpdateCameraPosition();
            }
        }

        void LateUpdate()
        {
            if (target == null) return;

            HandleMouseInput();
            UpdateCameraPosition();
            LookAtTarget();
        }

        #endregion

        #region Camera Control

        private void HandleMouseInput()
        {
            if (!enableMouseLook) return;

            // Use New Input System - Mouse delta
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse == null) return;

            float mouseX = mouse.delta.ReadValue().x * mouseSensitivity;
            float mouseY = mouse.delta.ReadValue().y * mouseSensitivity;

            if (invertY) mouseY = -mouseY;

            _horizontalAngle += mouseX;
            _verticalAngle -= mouseY;
            _verticalAngle = Mathf.Clamp(_verticalAngle, minVerticalAngle, maxVerticalAngle);
        }

        private void UpdateCameraPosition()
        {
            // Calculate desired camera position
            float currentDistance = Mathf.Clamp(distance, minDistance, maxDistance);

            Vector3 offset = new Vector3(
                -Mathf.Sin(_horizontalAngle * Mathf.Deg2Rad) * currentDistance,
                heightOffset,
                -Mathf.Cos(_horizontalAngle * Mathf.Deg2Rad) * currentDistance
            );

            // Apply vertical rotation
            Quaternion verticalRotation = Quaternion.Euler(_verticalAngle, 0f, 0f);
            offset = verticalRotation * offset;

            _targetPosition = target.position + offset;

            // Smooth camera movement
            _currentPosition = Vector3.Lerp(
                _currentPosition != Vector3.zero ? _currentPosition : _targetPosition,
                _targetPosition,
                Time.deltaTime * smoothSpeed
            );

            transform.position = _currentPosition;
        }

        private void LookAtTarget()
        {
            Vector3 lookTarget = target.position + Vector3.up * 0.5f; // Look at upper body
            Quaternion targetRotation = Quaternion.LookRotation(
                (lookTarget - transform.position).normalized,
                Vector3.up
            );

            _currentRotation = Quaternion.Slerp(
                _currentRotation != Quaternion.identity ? _currentRotation : targetRotation,
                targetRotation,
                Time.deltaTime * lookSmoothness
            );

            transform.rotation = _currentRotation;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set camera distance (clamped to min/max)
        /// </summary>
        public void SetDistance(float newDistance)
        {
            distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        }

        /// <summary>
        /// Zoom in/out
        /// </summary>
        public void Zoom(float delta)
        {
            distance = Mathf.Clamp(distance - delta, minDistance, maxDistance);
        }

        /// <summary>
        /// Reset camera to default position
        /// </summary>
        public void ResetCamera()
        {
            _horizontalAngle = 0f;
            _verticalAngle = 10f;
            distance = 3.5f;
        }

        /// <summary>
        /// Set target to follow
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        #endregion

        #region Debug UI

        void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 500, 300, 200));
            GUILayout.BeginVertical("box");

            GUILayout.Label("═══ Camera Follow ═══");
            GUILayout.Label($"Distance: {distance:F2}");
            GUILayout.Label($"Height: {heightOffset:F2}");
            GUILayout.Label($"V-Angle: {_verticalAngle:F1}");
            GUILayout.Label($"H-Angle: {_horizontalAngle:F1}");

            GUILayout.Space(10);

            if (GUILayout.Button("Reset Camera"))
                ResetCamera();

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        #region Inspector Helpers

        private void OnDrawGizmosSelected()
        {
            if (target == null) return;

            // Draw target
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position, 0.3f);

            // Draw camera position
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.3f);

            // Draw line of sight
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
        }

        #endregion
    }
}
