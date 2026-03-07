// TrapBehavior.cs
// Procedural trap behavior with 8-bit pixel art style
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Core system - works with SpawnPlacerEngine

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Procedural trap with different types and behaviors.
    /// Spawns at large distances from player for fair gameplay.
    /// </summary>
    public class TrapBehavior : BehaviorEngine
    {
        [Header("Trap Settings")]
        [SerializeField] private TrapType trapType = TrapType.Spike;
        [SerializeField] private float damage = 20f;
        [SerializeField] private float triggerRadius = 1.5f;
        [SerializeField] private float cooldownTime = 2f;
        [SerializeField] private bool isVisible = true;

        [Header("Visual")]
        [SerializeField] private GameObject trapVisual;
        [SerializeField] private Color trapColor = Color.red;

        [Header("Audio")]
        [SerializeField] private AudioClip triggerSound;
        [SerializeField] private AudioClip damageSound;

        private float _lastTriggerTime;
        private bool _isTriggered;
        private Collider _triggerCollider;

        public TrapType TrapType => trapType;
        public float Damage => damage;
        public bool IsTriggered => _isTriggered;

        private new void Awake()
        {
            SetItemType(ItemType.Switch);
            canInteract = false;
            canCollect = false;
            interactionRange = triggerRadius;

            CreateTrapVisual();
        }

        public void Initialize(TrapType type, float dmg, float radius, float cooldown)
        {
            trapType = type;
            damage = dmg;
            triggerRadius = radius;
            cooldownTime = cooldown;

            CreateTrapVisual();
        }

        private void CreateTrapVisual()
        {
            if (trapVisual != null)
            {
                trapVisual.SetActive(isVisible);
            }
            else
            {
                // Auto-create visual based on trap type
                trapVisual = new GameObject("TrapVisual");
                trapVisual.transform.SetParent(transform);
                trapVisual.transform.localPosition = Vector3.zero;

                var renderer = trapVisual.AddComponent<SpriteRenderer>();
                renderer.color = trapColor;

                // Create pixel art texture for trap
                var texture = CreateTrapTexture();
                var sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 1f);
                renderer.sprite = sprite;

                trapVisual.SetActive(isVisible);
            }

            // Add trigger collider
            if (_triggerCollider == null)
            {
                _triggerCollider = gameObject.AddComponent<SphereCollider>();
                _triggerCollider.radius = triggerRadius;
                _triggerCollider.isTrigger = true;
            }
        }

        private Texture2D CreateTrapTexture()
        {
            const int size = 32;
            var texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;

            // 8-bit color palette
            var trapDark = new Color32(80, 20, 20, 255);
            var trapMid = new Color32(120, 40, 40, 255);
            var trapLight = new Color32(160, 60, 60, 255);
            var spike = new Color32(100, 100, 110, 255);

            // Draw based on trap type
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color32 color;

                    switch (trapType)
                    {
                        case TrapType.Spike:
                            // Spike pattern
                            if (y < x / 2 || y < (size - x) / 2)
                                color = spike;
                            else
                                color = ((x + y) % 4 < 2) ? trapDark : trapMid;
                            break;

                        case TrapType.Pit:
                            // Dark pit pattern
                            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2, size / 2));
                            color = dist < size / 3 ? Color.black : trapMid;
                            break;

                        case TrapType.Dart:
                            // Dart launcher pattern
                            if (x < 8 || x > size - 8)
                                color = spike;
                            else
                                color = ((x + y) % 4 < 2) ? trapDark : trapMid;
                            break;

                        default:
                            color = trapMid;
                            break;
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                TriggerTrap(other.gameObject);
            }
        }

        private void TriggerTrap(GameObject player)
        {
            if (Time.time - _lastTriggerTime < cooldownTime)
                return;

            _lastTriggerTime = Time.time;
            _isTriggered = true;

            // Play trigger sound
            if (triggerSound != null)
            {
                AudioSource.PlayClipAtPoint(triggerSound, transform.position);
            }

            // Deal damage to player
            var playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage, DamageType.Physical);
                Debug.Log($"[Trap] {trapType} dealt {damage} damage to player");
            }

            // Visual feedback
            if (trapVisual != null)
            {
                trapVisual.GetComponent<SpriteRenderer>().color = Color.white;
                Invoke(nameof(ResetVisual), 0.2f);
            }

            _isTriggered = false;
        }

        private void ResetVisual()
        {
            if (trapVisual != null)
            {
                trapVisual.GetComponent<SpriteRenderer>().color = trapColor;
            }
        }

        public override void Reset()
        {
            base.Reset();
            _isTriggered = false;
            _lastTriggerTime = 0f;
        }

        private new void OnDestroy()
        {
            base.OnDestroy();
            if (trapVisual != null)
            {
                Destroy(trapVisual);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw trigger radius
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, triggerRadius);

            // Draw trap bounds
            Gizmos.color = trapColor;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }

    /// <summary>
    /// Types of traps available in the game.
    /// </summary>
    public enum TrapType
    {
        Spike,      // Floor spike - damages on contact
        Pit,        // Pit trap - player falls in
        Dart,       // Dart launcher - shoots projectile
        Flame,      // Flame trap - periodic fire damage
        Poison,     // Poison gas - DoT effect
        Electric,   // Electric trap - chain lightning
        Ice,        // Ice trap - slows player
        Explosion   // Explosive trap - AOE damage
    }
}
