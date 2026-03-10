// SafeAnimationController.cs
// Handles smooth lid opening animation (rotation/elevation)
// Unity 6000.10f1 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Manages safe lid lifting animation with configurable easing and duration

using UnityEngine;

namespace Code.Lavos.Interactables
{
    public class SafeAnimationController : MonoBehaviour
    {
        [Header("Lid Configuration")]
        [SerializeField] private Transform lidTransform;
        [SerializeField] private Vector3 lidOpenRotation = new Vector3(90f, 0f, 0f);
        [SerializeField] private float animationDurationSeconds = 1.5f;
        [SerializeField] private AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool debugMode = false;

        private Vector3 lidClosedRotation;
        private float animationProgress = 0f;
        private bool isAnimating = false;

        private void Awake()
        {
            if (lidTransform == null)
                lidTransform = transform.Find("Lid");

            if (lidTransform == null && debugMode)
                Debug.LogWarning("[SafeAnimationController] Lid transform not found. Assign manually or create child object named 'Lid'");

            lidClosedRotation = lidTransform != null ? lidTransform.localEulerAngles : Vector3.zero;
        }

        private void Update()
        {
            if (isAnimating)
                UpdateAnimation();
        }

        public void PlayOpenAnimation()
        {
            if (lidTransform == null)
            {
                if (debugMode)
                    Debug.LogError("[SafeAnimationController] Cannot play animation: Lid transform is null");
                return;
            }

            isAnimating = true;
            animationProgress = 0f;

            if (debugMode)
                Debug.Log("[SafeAnimationController] Starting lid open animation");
        }

        private void UpdateAnimation()
        {
            animationProgress += Time.deltaTime / animationDurationSeconds;

            if (animationProgress >= 1f)
            {
                animationProgress = 1f;
                isAnimating = false;

                if (debugMode)
                    Debug.Log("[SafeAnimationController] Lid open animation completed");
            }

            float easedProgress = easingCurve.Evaluate(animationProgress);
            Vector3 currentRotation = Vector3.Lerp(lidClosedRotation, lidOpenRotation, easedProgress);
            lidTransform.localEulerAngles = currentRotation;
        }

        public bool IsAnimating()
        {
            return isAnimating;
        }

        public float GetAnimationProgress()
        {
            return animationProgress;
        }
    }
}
