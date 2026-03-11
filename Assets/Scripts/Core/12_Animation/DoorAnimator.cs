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
// DoorAnimator.cs
// Door animation component - rotates door around Y axis (hinge)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Animation system - used by all door types

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// DoorAnimator - Handles smooth door rotation animation.
    /// Rotates door around Y axis (hinge) with configurable duration.
    /// </summary>
    public class DoorAnimator : MonoBehaviour
    {
        private float startAngle = 0f;
        private float targetAngle = 90f;
        private float duration = 1f;
        private float elapsed = 0f;
        private bool isAnimating = false;

        /// <summary>
        /// Animate to an absolute target angle.
        /// Handles angle wrapping correctly for doors that start at 180.
        /// </summary>
        /// <param name="angle">Target angle in degrees</param>
        /// <param name="dur">Animation duration in seconds</param>
        public void AnimateToAbsoluteAngle(float angle, float dur)
        {
            startAngle = transform.localEulerAngles.y;
            targetAngle = angle;

            // Normalize to -180 to 180 for consistent interpolation
            startAngle = NormalizeAngle(startAngle);
            targetAngle = NormalizeAngle(targetAngle);

            // Find shortest path
            float diff = targetAngle - startAngle;
            if (diff > 180f) diff -= 360f;
            if (diff < -180f) diff += 360f;

            // Store the actual target (may be adjusted for shortest path)
            targetAngle = startAngle + diff;

            duration = dur;
            elapsed = 0f;
            isAnimating = true;

            Debug.Log($"[DoorAnimator] {name}: {startAngle:F1}  {targetAngle:F1} (diff={diff:F1}) over {dur}s", this);
        }

        /// <summary>
        /// Animate by a relative angle delta.
        /// </summary>
        /// <param name="angle">Angle delta in degrees</param>
        /// <param name="dur">Animation duration in seconds</param>
        public void AnimateToAngle(float angle, float dur)
        {
            startAngle = transform.localEulerAngles.y;
            targetAngle = startAngle + angle;

            duration = dur;
            elapsed = 0f;
            isAnimating = true;

            Debug.Log($"[DoorAnimator] Relative: {startAngle:F1}  {targetAngle:F1} (delta={angle:F1}) over {dur}s", this);
        }

        /// <summary>
        /// Normalize angle to -180 to 180 range.
        /// </summary>
        float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        void Update()
        {
            if (!isAnimating) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth step for natural motion
            t = Mathf.SmoothStep(0, 1, t);

            float currentY = Mathf.Lerp(startAngle, targetAngle, t);
            transform.localEulerAngles = new Vector3(0, currentY, 0);

            if (t >= 1f)
            {
                isAnimating = false;
                Debug.Log($"[DoorAnimator] Complete at {currentY:F1}", this);
            }
        }
    }
}
