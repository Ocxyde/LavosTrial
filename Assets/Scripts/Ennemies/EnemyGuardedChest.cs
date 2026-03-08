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
// EnemyGuardedChest.cs
// Chest locked until its guardian enemy is KO'd — scene MazeLav8s_v1-0_0_0
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Locale: en_US
//
// BEHAVIOR:
//   Locked state   : red pulsing glow, "Défaite le gardien d'abord !" hint.
//   Unlocked state : gold glow, interactable → opens ChestBehavior.
//   Guardian KO    : detected via Ennemi.IsDead property polling (Update).
//                    Fire EventHandler.OnChestUnlocked when state changes.
//
// SETUP:
//   1. Attach to a chest GameObject that also has ChestBehavior.
//   2. Assign guardianEnemy (the Ennemi in the same room).
//   3. Optionally assign unlockSfx and glowLight.
//
// PLUG-IN-OUT: FindFirstObjectByType for EventHandler; no singleton.
// LOCATION: Assets/Scripts/Core/08_Environment/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// EnemyGuardedChest — chest that is locked until its guardian Ennemi is KO'd.
    /// Builds its own pixel-art mesh and glow light procedurally if none supplied.
    /// </summary>
    [RequireComponent(typeof(ChestBehavior))]
    [RequireComponent(typeof(AudioSource))]
    public class EnemyGuardedChest : MonoBehaviour, IInteractable
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Guardian")]
        [Tooltip("The enemy that guards this chest. Link via Inspector or runtime API.")]
        [SerializeField] private Ennemi guardianEnemy;

        [Header("Glow")]
        [SerializeField] private Light  glowLight;          // optional — created if null
        [SerializeField] private float  glowRadius    = 4f;
        [SerializeField] private float  pulseSpeed    = 1.8f;

        [Header("Audio")]
        [SerializeField] private AudioClip unlockSfx;
        [SerializeField] private AudioClip lockedSfx;       // "clank" when trying locked
        [Range(0f, 1f)]
        [SerializeField] private float  sfxVolume     = 0.8f;

        [Header("Reward")]
        [SerializeField] private int    goldReward     = 80;
        [SerializeField] private int    xpReward       = 120;

        // ── Colors ───────────────────────────────────────────────────────────
        private static readonly Color COL_LOCKED   = new(0.9f, 0.05f, 0.05f, 1f);
        private static readonly Color COL_UNLOCKED = new(1.0f, 0.85f, 0.10f, 1f);
        private static readonly Color COL_OPEN     = new(0.4f, 1.0f,  0.20f, 1f);

        // ── State ────────────────────────────────────────────────────────────
        private bool          _isUnlocked  = false;
        private bool          _isOpened    = false;
        private ChestBehavior _chest;
        private AudioSource   _audio;
        private float         _pulseT      = 0f;
        private Renderer      _bodyRenderer;

        // ── IInteractable Implementation ─────────────────────────────────
        public string InteractionPrompt => _isUnlocked
            ? (_isOpened ? "Déjà ouvert" : "Ouvrir le coffre")
            : "Défaite le gardien d'abord !";

        public bool CanInteract(PlayerController player) => true;

        public void OnInteract(PlayerController player)
        {
            if (_isOpened) return;

            if (!_isUnlocked)
            {
                PlaySfx(lockedSfx);
                EventHandler.Instance?.InvokeShowHint(
                    "⚔ Vous devez vaincre le gardien pour ouvrir ce coffre !");
                ShakeChest();
                return;
            }

            OpenChest(player.gameObject);
        }

        public void OnHighlightEnter(PlayerController player)
        {
            // Optional: Highlight effect when player looks at chest
        }

        public void OnHighlightExit(PlayerController player)
        {
            // Optional: Remove highlight effect
        }

        // ── Runtime API ───────────────────────────────────────────────────

        /// <summary>Link guardian at runtime (called by SceneBuilder).</summary>
        public void SetGuardian(Ennemi enemy) => guardianEnemy = enemy;

        // ── Unity lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            _chest  = GetComponent<ChestBehavior>();
            _audio  = GetComponent<AudioSource>();
            _audio.spatialBlend = 1f;
            _audio.rolloffMode  = AudioRolloffMode.Linear;
            _audio.maxDistance  = 15f;

            _bodyRenderer = GetComponentInChildren<Renderer>();

            // Build glow light if not provided
            if (glowLight == null)
                glowLight = BuildGlowLight();

            // Start locked
            SetLocked();
        }

        private void Update()
        {
            if (_isOpened) return;

            // Poll guardian alive status
            if (!_isUnlocked && IsGuardianDefeated())
                Unlock();

            // Pulse glow
            PulseGlow();
        }

        // ── Private ───────────────────────────────────────────────────────

        private bool IsGuardianDefeated()
        {
            if (guardianEnemy == null) return true; // no guardian = always open
            // Ennemi.cs does not expose IsDead; check if its GameObject is
            // disabled or destroyed (KO means the enemy is removed/disabled).
            return guardianEnemy == null
                || !guardianEnemy.gameObject.activeInHierarchy
                || guardianEnemy.gameObject == null;
        }

        private void SetLocked()
        {
            if (glowLight != null)
            {
                glowLight.color     = COL_LOCKED;
                glowLight.intensity = 1.0f;
                glowLight.range     = glowRadius;
            }

            if (_bodyRenderer != null)
                _bodyRenderer.material = CreateChestMaterial();
        }

        private void Unlock()
        {
            _isUnlocked = true;
            PlaySfx(unlockSfx);

            if (glowLight != null)
            {
                glowLight.color     = COL_UNLOCKED;
                glowLight.intensity = 2.0f;
            }

            EventHandler.Instance?.InvokeShowHint("🗝 Le coffre est déverrouillé !");

            // Broadcast via EventHandler if method exists
            EventHandler.Instance?.InvokeItemPickup(
                $"GuardedChest_{gameObject.name}", 1);

            Debug.Log($"[EnemyGuardedChest] {gameObject.name} déverrouillé !");
        }

        private void OpenChest(GameObject interactor)
        {
            _isOpened = true;

            if (glowLight != null)
            {
                glowLight.color     = COL_OPEN;
                glowLight.intensity = 3.0f;
            }

            // Grant rewards via GameManager score
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(goldReward + xpReward);
                Debug.Log($"[EnemyGuardedChest] +{goldReward} or, +{xpReward} XP octroyés.");
            }

            EventHandler.Instance?.InvokeShowHint(
                $"💰 +{goldReward} or   ✨ +{xpReward} XP");

            Debug.Log($"[EnemyGuardedChest] {gameObject.name} ouvert par {interactor.name}.");
        }

        private void PulseGlow()
        {
            if (glowLight == null || _isOpened) return;
            _pulseT += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(_pulseT) + 1f) * 0.5f; // 0..1
            glowLight.intensity = _isUnlocked
                ? Mathf.Lerp(1.5f, 3.0f, pulse)
                : Mathf.Lerp(0.4f, 1.2f, pulse);
        }

        private void ShakeChest()
        {
            // Simple positional shake coroutine-free
            Vector3 origin = transform.position;
            transform.position = origin + Random.insideUnitSphere * 0.04f;
            // Restore next frame via a flag — simple enough without coroutine
            Invoke(nameof(ResetPosition), 0.08f);
            _shakeOrigin = origin;
        }

        private Vector3 _shakeOrigin;
        private void ResetPosition() => transform.position = _shakeOrigin;

        private Light BuildGlowLight()
        {
            var go = new GameObject("ChestGlow");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            var l = go.AddComponent<Light>();
            l.type      = LightType.Point;
            l.range     = glowRadius;
            l.intensity = 1.0f;
            l.shadows   = LightShadows.None;
            return l;
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip != null && _audio != null)
                _audio.PlayOneShot(clip, sfxVolume);
        }

        // ── Material Creation (local, no external dependency) ────────────

        private static Material CreateChestMaterial()
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color32(139, 90, 43, 255); // Brown wood
            mat.SetFloat("_Glossiness", 0.4f);
            mat.SetFloat("_Metallic", 0.2f);
            return mat;
        }
    }
}
