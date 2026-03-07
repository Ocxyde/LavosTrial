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
        [SerializeField] private Transform canvasParent;
        [SerializeField] private bool dontDestroyOnLoad = true;

        [Header("Floating Text Settings")]
        [SerializeField] private float defaultFloatDuration = 1.5f;
        [SerializeField] private float floatSpeed = 40f;
        [SerializeField] private int fontSize = 32;

        [Header("Dialog Settings")]
        [SerializeField] private float dialogDisplayTime = 3f;
        [SerializeField] private float dialogFadeTime = 0.3f;

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
            // Create floating text parent
            var floatGO = new GameObject("FloatingTextContainer");
            floatGO.transform.SetParent(canvasParent != null ? canvasParent : transform, false);
            _floatingTextParent = floatGO.AddComponent<RectTransform>().transform;

            // Create dialog parent
            var dialogGO = new GameObject("DialogContainer");
            dialogGO.transform.SetParent(canvasParent != null ? canvasParent : transform, false);
            _dialogParent = dialogGO.AddComponent<RectTransform>().transform;
        }

        #region Floating Combat Text

        /// <summary>
        /// Show floating combat text (damage, heal, etc.).
        /// </summary>
        public void ShowFloatingText(string text, Color color, Vector2 position, float? duration = null)
        {
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
        /// Show a dialog message at the center of the screen.
        /// </summary>
        public void ShowDialog(string message, float? displayTime = null, Color? color = null)
        {
            var go = new GameObject("Dialog");
            go.transform.SetParent(_dialogParent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600, 100);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            // Background
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(go.transform, false);
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Text
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = message;
            tmp.fontSize = 24;
            tmp.fontStyle = TMPro.FontStyles.Regular;
            tmp.color = color ?? infoColor;
            tmp.alignment = TextAlignmentOptions.Center;

            // Outline
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, 2);

            StartCoroutine(AnimateDialog(go, tmp, displayTime ?? dialogDisplayTime));
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
