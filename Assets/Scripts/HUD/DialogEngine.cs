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
// DialogEngine.cs
// Central Dialog and Floating Text Engine
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Reusable system for dialogs, notifications, and floating text
// - Floating combat text (damage, heal, stats)
// - Dialog system for conversations
// - Status notifications
// - Custom messages
// - Quest updates

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Code.Lavos.HUD
{
    /// <summary>
    /// Central engine for all dialog and floating text systems.
    /// Provides floating combat text, dialogs, and notifications.
    /// </summary>
    public class DialogEngine : MonoBehaviour
    {
        public static DialogEngine Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Parent canvas for dialogs (finds automatically if not set)")]
        [SerializeField] private Transform canvasParent;
        [SerializeField] private bool dontDestroyOnLoad = true;

        private Canvas _canvas; // Cached canvas reference

        [Header("Floating Text Settings")]
        [SerializeField] private float defaultFloatDuration = 1.5f;
        [SerializeField] private float floatSpeed = 40f;
        [SerializeField] private int fontSize = 32;

        [Header("Dialog Settings")]
        [SerializeField] private float dialogDisplayTime = 3f;
        [SerializeField] private float dialogFadeTime = 0.3f;
        [SerializeField] private Vector2 dialogPosition = new Vector2(-300, -200); // Bottom-left
        [SerializeField] private Vector2 dialogSize = new Vector2(500, 120);
        [SerializeField] private bool dialogResizable = true;

        [Header("Colors")]
        [SerializeField] private Color damageColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private Color healColor = new Color(0.2f, 1f, 0.3f);
        [SerializeField] private Color manaLossColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color manaGainColor = Color.cyan;
        [SerializeField] private Color staminaLossColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color staminaGainColor = Color.yellow;
        [SerializeField] private Color critColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color infoColor = Color.white;
        [SerializeField] private Color warningColor = new Color(1f, 0.6f, 0.2f);

        private Transform _floatingTextParent;
        private Transform _dialogParent;

        // Static color access
        public static Color DamageRed => new Color(1f, 0.2f, 0.2f);
        public static Color HealGreen => new Color(0.2f, 1f, 0.3f);
        public static Color ManaBlue => new Color(0.3f, 0.6f, 1f);
        public static Color StaminaYellow => new Color(1f, 0.9f, 0.2f);
        public static Color CritOrange => new Color(1f, 0.5f, 0f);
        public static Color ShieldPurple => new Color(0.8f, 0.4f, 1f);
        public static Color WarningOrange => new Color(1f, 0.6f, 0.2f);
        public static Color InfoWhite => Color.white;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            CreateParents();
        }

        private void CreateParents()
        {
            // PLUG-IN-OUT: Find existing canvas
            _canvas = FindFirstObjectByType<Canvas>();
            
            if (_canvas == null)
            {
                Debug.LogError("[DialogEngine] No Canvas found in scene! Dialogs will not be visible.");
                enabled = false;
                return;
            }

            // Create floating text parent (DYNAMIC VFX: acceptable for transient text)
            var floatGO = new GameObject("FloatingTextContainer");
            floatGO.transform.SetParent(_canvas.transform, false);
            _floatingTextParent = floatGO.AddComponent<RectTransform>().transform;

            // Create dialog parent (DYNAMIC UI: acceptable for variable dialogs)
            var dialogGO = new GameObject("DialogContainer");
            dialogGO.transform.SetParent(_canvas.transform, false);
            _dialogParent = dialogGO.AddComponent<RectTransform>().transform;
            
            Debug.Log($"[DialogEngine] Created parents under canvas '{_canvas.name}'");
        }

        private void OnDestroy()
        {
            // Stop all coroutines to prevent memory leaks
            StopAllCoroutines();

            // Clean up dynamically created GameObjects
            if (_floatingTextParent != null)
                Destroy(_floatingTextParent.gameObject);
            if (_dialogParent != null)
                Destroy(_dialogParent.gameObject);
        }

        #region Floating Combat Text

        /// <summary>
        /// Show floating combat text (damage, heal, etc.).
        /// NOTE: Creates dynamic UI elements - acceptable for transient VFX.
        /// </summary>
        public void ShowFloatingText(string text, Color color, Vector2 position, float? duration = null)
        {
            // DYNAMIC VFX: Create temporary floating text (acceptable for transient VFX)
            var go = new GameObject($"Float_{text}");
            go.transform.SetParent(_floatingTextParent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 50);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;

            // Outline for visibility
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(3, 3);

            // Shadow for depth
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, 2);

            StartCoroutine(AnimateFloatingText(go, tmp, duration ?? defaultFloatDuration));
        }

        /// <summary>
        /// Show floating damage number.
        /// </summary>
        public void ShowDamage(int amount, Vector2 position, bool isCritical = false)
        {
            string text = isCritical ? $"CRIT {amount}!" : amount.ToString();
            Color color = isCritical ? critColor : damageColor;
            float scale = isCritical ? 1.5f : 1f;
            ShowFloatingText(text, color, position, defaultFloatDuration * scale);
        }

        /// <summary>
        /// Show floating heal number.
        /// </summary>
        public void ShowHeal(int amount, Vector2 position)
        {
            ShowFloatingText($"+{amount}", healColor, position);
        }

        /// <summary>
        /// Show floating mana change.
        /// </summary>
        public void ShowManaChange(int amount, Vector2 position)
        {
            if (amount < 0)
            {
                ShowFloatingText(amount.ToString(), manaLossColor, position);
            }
            else
            {
                ShowFloatingText($"+{amount}", manaGainColor, position);
            }
        }

        /// <summary>
        /// Show floating stamina change.
        /// </summary>
        public void ShowStaminaChange(int amount, Vector2 position)
        {
            if (amount < 0)
            {
                ShowFloatingText(amount.ToString(), staminaLossColor, position);
            }
            else
            {
                ShowFloatingText($"+{amount}", staminaGainColor, position);
            }
        }

        /// <summary>
        /// Show floating stat gain.
        /// </summary>
        public void ShowStatGain(string statName, int amount, Vector2 position)
        {
            ShowFloatingText($"+{amount} {statName}", infoColor, position);
        }

        /// <summary>
        /// Show floating stat loss.
        /// </summary>
        public void ShowStatLoss(string statName, int amount, Vector2 position)
        {
            ShowFloatingText($"-{amount} {statName}", new Color(0.7f, 0.7f, 0.7f), position);
        }

        private IEnumerator AnimateFloatingText(GameObject go, TextMeshProUGUI text, float duration)
        {
            float elapsed = 0f;
            Color original = text.color;
            Vector3 startPos = go.transform.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Fade out
                text.color = new Color(original.r, original.g, original.b, original.a * (1 - t));

                // Float upward with sine wave
                go.transform.localPosition = startPos + Vector3.up * (elapsed * floatSpeed)
                    + Vector3.right * Mathf.Sin(elapsed * 3f) * 8f;

                // Scale up slightly
                float scale = 1f + t * 0.3f;
                go.transform.localScale = Vector3.one * scale;

                yield return null;
            }

            Destroy(go);
        }

        #endregion

        #region Dialog System

        /// <summary>
        /// Show a dialog message at the bottom-left of the screen.
        /// Resizable and manageable for conversations.
        /// </summary>
        public void ShowDialog(string message, float? displayTime = null, Color? color = null, Vector2? position = null, Vector2? size = null)
        {
            var go = new GameObject("Dialog");
            go.transform.SetParent(_dialogParent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size ?? dialogSize;
            rect.anchorMin = new Vector2(0, 0); // Bottom-left anchor
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.anchoredPosition = position ?? dialogPosition;

            // Background (semi-transparent for conversation style)
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(go.transform, false);
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.08f, 0.85f);

            // Border
            var borderGO = new GameObject("Border");
            borderGO.transform.SetParent(go.transform, false);
            var borderRect = borderGO.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-2, -2);
            borderRect.offsetMax = new Vector2(2, 2);
            var borderImage = borderGO.AddComponent<Image>();
            borderImage.color = new Color(0.3f, 0.3f, 0.35f, 0.5f);

            // Text
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = message;
            tmp.fontSize = 18;
            tmp.fontStyle = TMPro.FontStyles.Normal;
            tmp.color = color ?? infoColor;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;

            // Margin for text
            var textRect = tmp.rectTransform;
            textRect.offsetMin = new Vector2(15, 15);
            textRect.offsetMax = new Vector2(-15, -15);

            // Outline for readability
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, 2);

            // Note: Resize handle feature can be added in future implementation
            // For now, dialogs are fixed size but can be positioned anywhere

            StartCoroutine(AnimateDialog(go, tmp, displayTime ?? dialogDisplayTime));
        }

        private GameObject CreateResizeHandle(GameObject dialog)
        {
            var handleGO = new GameObject("ResizeHandle");
            handleGO.transform.SetParent(dialog.transform, false);
            var handleRect = handleGO.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);
            handleRect.anchorMin = new Vector2(1, 0);
            handleRect.anchorMax = new Vector2(1, 0);
            handleRect.pivot = new Vector2(1, 0);
            handleRect.anchoredPosition = Vector2.zero;

            var handleImage = handleGO.AddComponent<Image>();
            handleImage.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

            // Corner triangle visual
            var cornerGO = new GameObject("Corner");
            cornerGO.transform.SetParent(handleGO.transform, false);
            var cornerRect = cornerGO.AddComponent<RectTransform>();
            cornerRect.anchorMin = new Vector2(0, 0);
            cornerRect.anchorMax = new Vector2(1, 1);
            var cornerImage = cornerGO.AddComponent<Image>();
            cornerImage.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);

            return handleGO;
        }

        /// <summary>
        /// Set dialog position for conversations.
        /// </summary>
        public void SetDialogPosition(Vector2 newPosition)
        {
            dialogPosition = newPosition;
        }

        /// <summary>
        /// Set dialog size for conversations.
        /// </summary>
        public void SetDialogSize(Vector2 newSize)
        {
            dialogSize = newSize;
        }

        /// <summary>
        /// Enable/disable dialog resizing.
        /// </summary>
        public void SetDialogResizable(bool resizable)
        {
            dialogResizable = resizable;
        }

        /// <summary>
        /// Show a quest/notification message.
        /// </summary>
        public void ShowNotification(string title, string message, float displayTime = 5f)
        {
            ShowDialog($"{title}\n{message}", displayTime, infoColor);
        }

        /// <summary>
        /// Show a warning message.
        /// </summary>
        public void ShowWarning(string message, float displayTime = 4f)
        {
            ShowDialog(message, displayTime, warningColor);
        }

        /// <summary>
        /// Show an info message.
        /// </summary>
        public void ShowInfo(string message, float displayTime = 3f)
        {
            ShowDialog(message, displayTime, infoColor);
        }

        private IEnumerator AnimateDialog(GameObject go, TextMeshProUGUI text, float displayTime)
        {
            // Fade in
            float elapsed = 0f;
            Color original = text.color;
            text.color = new Color(original.r, original.g, original.b, 0f);

            while (elapsed < dialogFadeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dialogFadeTime;
                text.color = new Color(original.r, original.g, original.b, t);
                yield return null;
            }

            // Wait
            yield return new WaitForSeconds(displayTime - dialogFadeTime);

            // Fade out
            elapsed = 0f;
            while (elapsed < dialogFadeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dialogFadeTime;
                text.color = new Color(original.r, original.g, original.b, original.a * (1 - t));
                yield return null;
            }

            Destroy(go);
        }

        #endregion

        #region Utility

        /// <summary>
        /// Clear all floating text.
        /// </summary>
        public void ClearAllFloatingText()
        {
            foreach (Transform child in _floatingTextParent)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Clear all dialogs.
        /// </summary>
        public void ClearAllDialogs()
        {
            foreach (Transform child in _dialogParent)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Set the canvas parent for UI elements.
        /// </summary>
        public void SetCanvasParent(Transform newParent)
        {
            canvasParent = newParent;
            if (_floatingTextParent != null)
                _floatingTextParent.SetParent(newParent, false);
            if (_dialogParent != null)
                _dialogParent.SetParent(newParent, false);
        }

        #endregion
    }
}
