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
// CastleDoubleDoor.cs
// Manor / castle style double door — scene MazeLav8s_v1-0_0_0
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Locale: en_US
//
// BEHAVIOR:
//   - Two door panels (left + right) that swing open on both sides.
//   - Optional OneWay mode: door only opens when player approaches from
//     the EXIT side (inside the maze).  Blocked from the outside.
//   - Pixel-art 8-bit iron texture via Lav8s_PixelArt8Bit.
//   - Visual arch / trim added procedurally above the door.
//   - Interactable: press [E] to open / close.
//
// SETUP:
//   Attach to an empty GameObject at the door center.
//   Set wallFacing to the direction the door faces (world Y-angle).
//   Toggle isOneWayExit for the final exit door.
//
// PLUG-IN-OUT: no DontDestroyOnLoad — local to maze scene.
// LOCATION: Assets/Scripts/Core/07_Doors/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// CastleDoubleDoor — procedural manor double door.
    /// Builds geometry at runtime; no prefab required beyond this MonoBehaviour.
    /// Interactable via DoorsEngine or direct IInteractable call.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class CastleDoubleDoor : MonoBehaviour, IInteractable
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Geometry")]
        [SerializeField] private float doorWidth    = 2.8f;   // each panel width (m)
        [SerializeField] private float doorHeight   = 3.6f;   // panel height (m)
        [SerializeField] private float doorThick    = 0.18f;  // panel thickness (m)
        [SerializeField] private float archHeight   = 0.8f;   // stone arch above

        [Header("Behavior")]
        [Tooltip("Exit only: cannot be opened from the outside.")]
        [SerializeField] public  bool  isOneWayExit = false;
        [Tooltip("Forward = direction the door faces (toward exit side).")]
        [SerializeField] private float wallFacingY  = 0f;     // world Y rotation
        [SerializeField] private float openAngle    = 85f;    // each panel swing
        [SerializeField] private float openSpeed    = 2.2f;
        [SerializeField] private float closeSpeed   = 1.6f;
        [SerializeField] private float autoCloseDelay = 5f;   // 0 = no auto-close

        [Header("Audio")]
        [SerializeField] private AudioClip openSfx;
        [SerializeField] private AudioClip closeSfx;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume   = 0.75f;

        // ── State ────────────────────────────────────────────────────────────
        private bool   _isOpen        = false;
        private bool   _isAnimating   = false;
        private float  _targetAngle   = 0f;
        private float  _currentAngle  = 0f;
        private float  _autoCloseTimer= 0f;

        // ── Scene objects ─────────────────────────────────────────────────
        private Transform _leftPivot;
        private Transform _rightPivot;
        private AudioSource _audio;

        // ── IInteractable Implementation ─────────────────────────────────
        public string InteractionPrompt => _isOpen ? "Fermer" : "Ouvrir";

        public bool CanInteract(PlayerController player) => true;

        public void OnInteract(PlayerController player)
        {
            if (_isOpen)
            {
                Close();
                return;
            }

            // One-way check: player must be on the EXIT side (inside maze)
            if (isOneWayExit)
            {
                Vector3 toDoor  = transform.position - player.transform.position;
                Vector3 doorFwd = Quaternion.Euler(0, wallFacingY, 0) * Vector3.forward;
                // dot > 0 means player is on the "inside" (exit side)
                if (Vector3.Dot(toDoor.normalized, doorFwd) < 0f)
                {
                    Debug.Log("[CastleDoubleDoor] Porte à sens unique — côté extérieur bloqué.");
                    EventHandler.Instance?.InvokeShowHint("Sortie à sens unique — impossible d'entrer.");
                    return;
                }
            }

            Open();
        }

        public void OnHighlightEnter(PlayerController player)
        {
            // Optional: Highlight effect when player looks at door
        }

        public void OnHighlightExit(PlayerController player)
        {
            // Optional: Remove highlight effect
        }

        // ── Unity lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _audio.spatialBlend = 1f;
            _audio.rolloffMode  = AudioRolloffMode.Linear;
            _audio.maxDistance  = 20f;

            BuildDoor();
        }

        private void Update()
        {
            if (!_isAnimating) return;

            float speed = _isOpen ? openSpeed : closeSpeed;
            _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle,
                                               speed * 90f * Time.deltaTime);

            ApplyRotation();

            if (Mathf.Approximately(_currentAngle, _targetAngle))
                _isAnimating = false;

            // Auto-close countdown
            if (_isOpen && autoCloseDelay > 0f)
            {
                _autoCloseTimer += Time.deltaTime;
                if (_autoCloseTimer >= autoCloseDelay)
                    Close();
            }
        }

        // ── Public API ────────────────────────────────────────────────────

        public void Open()
        {
            if (_isOpen) return;
            _isOpen      = true;
            _targetAngle = openAngle;
            _isAnimating = true;
            _autoCloseTimer = 0f;
            PlaySfx(openSfx);
            EventHandler.Instance?.InvokeDoorOpened(transform.position, DoorVariant.Normal);
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen      = false;
            _targetAngle = 0f;
            _isAnimating = true;
            PlaySfx(closeSfx);
        }

        // ── Build geometry ─────────────────────────────────────────────────

        private void BuildDoor()
        {
            // ── Left panel ─────────────────────────────────────────────
            _leftPivot  = new GameObject("LeftPivot").transform;
            _leftPivot.SetParent(transform, false);
            _leftPivot.localPosition = new Vector3(-doorWidth * 0.5f, 0f, 0f);
            _leftPivot.localRotation = Quaternion.identity;

            GameObject leftPanel = MakePanel("LeftPanel");
            leftPanel.transform.SetParent(_leftPivot, false);
            // Panel center offset from pivot (pivot at inner edge)
            leftPanel.transform.localPosition = new Vector3(-doorWidth * 0.5f, doorHeight * 0.5f, 0f);
            ApplyMaterial(leftPanel, CreateDoubleDoorMaterial());
            AddDoorCollider(leftPanel);

            // ── Right panel ────────────────────────────────────────────
            _rightPivot = new GameObject("RightPivot").transform;
            _rightPivot.SetParent(transform, false);
            _rightPivot.localPosition = new Vector3(doorWidth * 0.5f, 0f, 0f);
            _rightPivot.localRotation = Quaternion.identity;

            GameObject rightPanel = MakePanel("RightPanel");
            rightPanel.transform.SetParent(_rightPivot, false);
            rightPanel.transform.localPosition = new Vector3(doorWidth * 0.5f, doorHeight * 0.5f, 0f);
            ApplyMaterial(rightPanel, CreateDoubleDoorMaterial());
            AddDoorCollider(rightPanel);

            // ── Stone arch (trim above door) ───────────────────────────
            GameObject arch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arch.name = "Arch";
            arch.transform.SetParent(transform, false);
            arch.transform.localPosition = new Vector3(0f, doorHeight + archHeight * 0.5f, 0f);
            arch.transform.localScale    = new Vector3(doorWidth * 2f + doorThick,
                                                        archHeight, doorThick * 1.5f);
            ApplyMaterial(arch, CreateCastleWallMaterial());
            Destroy(arch.GetComponent<Collider>());

            // ── Left / right stone pillars ─────────────────────────────
            BuildPillar(-doorWidth - doorThick * 0.5f);
            BuildPillar( doorWidth + doorThick * 0.5f);

            // Apply world-facing rotation
            transform.localRotation = Quaternion.Euler(0f, wallFacingY, 0f);
        }

        private GameObject MakePanel(string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.localScale = new Vector3(doorWidth, doorHeight, doorThick);
            Destroy(go.GetComponent<BoxCollider>()); // collider added manually later
            return go;
        }

        private void BuildPillar(float xOffset)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "Pillar";
            pillar.transform.SetParent(transform, false);
            pillar.transform.localPosition = new Vector3(xOffset, doorHeight * 0.5f, 0f);
            pillar.transform.localScale    = new Vector3(doorThick * 2f, doorHeight, doorThick * 2f);
            ApplyMaterial(pillar, CreateCastleWallMaterial());
            Destroy(pillar.GetComponent<Collider>());
        }

        private void AddDoorCollider(GameObject panel)
        {
            var col = panel.AddComponent<BoxCollider>();
            col.size = Vector3.one; // matches the cube scale
        }

        private void ApplyMaterial(GameObject go, Material mat)
        {
            var r = go.GetComponent<Renderer>();
            if (r != null) r.material = mat;
        }

        private void ApplyRotation()
        {
            if (_leftPivot  != null)
                _leftPivot.localRotation  = Quaternion.Euler(0f, -_currentAngle, 0f);
            if (_rightPivot != null)
                _rightPivot.localRotation = Quaternion.Euler(0f,  _currentAngle, 0f);
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip != null && _audio != null)
                _audio.PlayOneShot(clip, sfxVolume);
        }

        // ── Material Creation (local, no external dependency) ────────────

        private static Material CreateDoubleDoorMaterial()
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color32(139, 90, 43, 255); // Brown wood
            mat.SetFloat("_Glossiness", 0.3f);
            mat.SetFloat("_Metallic", 0.1f);
            return mat;
        }

        private static Material CreateCastleWallMaterial()
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color32(80, 80, 90, 255); // Gray stone
            mat.SetFloat("_Glossiness", 0.2f);
            mat.SetFloat("_Metallic", 0.0f);
            return mat;
        }
    }
}
