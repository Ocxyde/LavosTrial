// DoorAnimation.cs
// Animation system for door opening/closing
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Plug-in-out: Works with DoorsEngine for smooth door animations

using UnityEngine;

#pragma warning disable CS0414 // Disable warnings for unused serialized fields (reserved for future features)

namespace Code.Lavos.Core
{
    /// <summary>
    /// Handles smooth door animations for opening/closing.
    /// Plug-in-out: Attach to door GameObject with DoorsEngine.
    /// </summary>
    public class DoorAnimation : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Animation Settings")]
        [Range(0.1f, 5f)]
        [SerializeField] private float openSpeed = 2f;
        [Range(0.1f, 5f)]
        [SerializeField] private float closeSpeed = 2f;
        
        [Header("Rotation Angles")]
        [SerializeField] private float leftDoorOpenAngle = -90f;
        [SerializeField] private float rightDoorOpenAngle = 90f;
        
        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] [Range(0f, 1f)] private float volume = 0.8f;

        #endregion

        #region Private Fields

        private Transform _leftPanel;
        private Transform _rightPanel;
        private AudioSource _audioSource;
        private bool _isAnimating = false;
        private float _currentRotation = 0f;
        private bool _isOpen = false;

        #endregion

        #region Properties

        public bool IsOpen => _isOpen;
        public bool IsAnimating => _isAnimating;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            FindDoorPanels();
            SetupAudio();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Set panel references from DoorFactory.
        /// </summary>
        public void SetPanelReferences(Transform leftPanel, Transform rightPanel)
        {
            _leftPanel = leftPanel;
            _rightPanel = rightPanel;
        }

        private void FindDoorPanels()
        {
            var doorRoot = transform.Find("DoorGeometry");
            if (doorRoot != null)
            {
                _leftPanel = doorRoot.Find("LeftPanel");
                _rightPanel = doorRoot.Find("RightPanel");
            }

            if (_leftPanel == null || _rightPanel == null)
            {
                // Panels will be set via SetPanelReferences from DoorFactory
                Debug.Log("[DoorAnimation] Waiting for panel references from DoorFactory");
            }
        }

        private void SetupAudio()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.playOnAwake = false;
            _audioSource.volume = volume;
        }

        #endregion

        #region Public Animation Methods

        /// <summary>
        /// Open the door with animation.
        /// Plug-in-out: Call from DoorsEngine.OpenDoor().
        /// </summary>
        public void Open()
        {
            if (_isOpen || _isAnimating) return;
            
            _isAnimating = true;
            _isOpen = true;
            
            if (openSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(openSound);
            }
            
            StartCoroutine(AnimateOpen());
        }

        /// <summary>
        /// Close the door with animation.
        /// Plug-in-out: Call from DoorsEngine.CloseDoor().
        /// </summary>
        public void Close()
        {
            if (!_isOpen || _isAnimating) return;
            
            _isAnimating = true;
            _isOpen = false;
            
            if (closeSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(closeSound);
            }
            
            StartCoroutine(AnimateClose());
        }

        /// <summary>
        /// Instantly set door state (no animation).
        /// For initialization or debugging.
        /// </summary>
        public void SetOpen(bool open)
        {
            _isOpen = open;
            _isAnimating = false;
            
            if (_leftPanel != null && _rightPanel != null)
            {
                if (open)
                {
                    _leftPanel.localRotation = Quaternion.Euler(0, leftDoorOpenAngle, 0);
                    _rightPanel.localRotation = Quaternion.Euler(0, rightDoorOpenAngle, 0);
                }
                else
                {
                    _leftPanel.localRotation = Quaternion.identity;
                    _rightPanel.localRotation = Quaternion.identity;
                }
            }
        }

        #endregion

        #region Animation Coroutines

        private System.Collections.IEnumerator AnimateOpen()
        {
            float elapsed = 0f;
            Quaternion leftStart = _leftPanel != null ? _leftPanel.localRotation : Quaternion.identity;
            Quaternion rightStart = _rightPanel != null ? _rightPanel.localRotation : Quaternion.identity;
            
            Quaternion leftEnd = Quaternion.Euler(0, leftDoorOpenAngle, 0);
            Quaternion rightEnd = Quaternion.Euler(0, rightDoorOpenAngle, 0);

            while (elapsed < openSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / openSpeed);
                t = SmoothStep(t); // Smooth easing

                if (_leftPanel != null)
                {
                    _leftPanel.localRotation = Quaternion.Slerp(leftStart, leftEnd, t);
                }
                if (_rightPanel != null)
                {
                    _rightPanel.localRotation = Quaternion.Slerp(rightStart, rightEnd, t);
                }

                yield return null;
            }

            // Ensure final position
            SetOpen(true);
            _isAnimating = false;
        }

        private System.Collections.IEnumerator AnimateClose()
        {
            float elapsed = 0f;
            Quaternion leftStart = _leftPanel != null ? _leftPanel.localRotation : Quaternion.identity;
            Quaternion rightStart = _rightPanel != null ? _rightPanel.localRotation : Quaternion.identity;
            
            Quaternion leftEnd = Quaternion.identity;
            Quaternion rightEnd = Quaternion.identity;

            while (elapsed < closeSpeed)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / closeSpeed);
                t = SmoothStep(t);

                if (_leftPanel != null)
                {
                    _leftPanel.localRotation = Quaternion.Slerp(leftStart, leftEnd, t);
                }
                if (_rightPanel != null)
                {
                    _rightPanel.localRotation = Quaternion.Slerp(rightStart, rightEnd, t);
                }

                yield return null;
            }

            // Ensure final position
            SetOpen(false);
            _isAnimating = false;
        }

        #endregion

        #region Utilities

        private float SmoothStep(float t)
        {
            // Smoothstep easing for natural motion
            return t * t * (3f - 2f * t);
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 380, 250, 120));
            GUILayout.Label($"[DoorAnimation DEBUG]");
            GUILayout.Label($"Open: {_isOpen}");
            GUILayout.Label($"Animating: {_isAnimating}");

            if (GUILayout.Button("Test Open"))
            {
                Open();
            }

            if (GUILayout.Button("Test Close"))
            {
                Close();
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
