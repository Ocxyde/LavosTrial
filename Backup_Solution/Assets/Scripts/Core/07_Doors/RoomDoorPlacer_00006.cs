// RoomDoorPlacer.cs
// Places doors in reserved wall holes with random variants and traps
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Finds components (never creates)
// - All values loaded from JSON config
// - No hardcoded values
// - Works with DoorHolePlacer and GridMazeGenerator
//
// LOCATION: Assets/Scripts/Core/07_Doors/

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// RoomDoorPlacer - Places doors in reserved wall holes.
    /// PLUG-IN-OUT COMPLIANT: Finds components, never creates them.
    /// ALL VALUES FROM JSON: Door variants, trap chances, etc. loaded from GameConfig.
    /// </summary>
    public class RoomDoorPlacer : MonoBehaviour
    {
        #region Inspector Fields (Serialized from JSON)

        [Header("🚪 Door Placement (From JSON Config)")]
        [Tooltip("Place doors in pre-carved holes (loaded from JSON)")]
        [SerializeField] private bool placeDoorsInHoles;
        
        [Tooltip("Randomize wall textures around doors (loaded from JSON)")]
        [SerializeField] private bool randomizeWallTextures;

        [Header("🚪 Door Variants (From JSON Config)")]
        [Tooltip("Enable trapped doors (loaded from JSON)")]
        [SerializeField] private bool enableTrappedDoors;
        
        [Tooltip("Chance for trap on door (loaded from JSON)")]
        [SerializeField] private float trapChance;
        
        [Tooltip("Enable locked doors (loaded from JSON)")]
        [SerializeField] private bool enableLockedDoors;
        
        [Tooltip("Enable secret doors (loaded from JSON)")]
        [SerializeField] private bool enableSecretDoors;

        [Header("🔌 Component References (Plug-in-Out)")]
        [Tooltip("Auto-finds DoorHolePlacer in scene")]
        [SerializeField] private DoorHolePlacer holePlacer;
        
        [Tooltip("Auto-finds GridMazeGenerator in scene")]
        [SerializeField] private GridMazeGenerator gridMazeGenerator;

        #endregion

        #region Private Data

        private List<GameObject> _placedDoors = new();
        private List<DoorData> _placedDoorData = new();

        #endregion

        #region Public Accessors

        public List<GameObject> PlacedDoors => _placedDoors;
        public int DoorCount => _placedDoors.Count;
        public List<DoorData> PlacedDoorData => _placedDoorData;
        public bool PlaceDoorsInHoles { get => placeDoorsInHoles; set => placeDoorsInHoles = value; }
        public bool RandomizeWallTextures { get => randomizeWallTextures; set => randomizeWallTextures = value; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // PLUG-IN-OUT: Find components (never create!)
            FindComponents();
            
            // LOAD ALL VALUES FROM JSON CONFIG (NO HARDCODING!)
            LoadConfig();
        }

        #endregion

        #region Plug-in-Out Compliance

        /// <summary>
        /// Find all required components in scene.
        /// PLUG-IN-OUT: Never creates components, only finds existing ones.
        /// </summary>
        private void FindComponents()
        {
            if (holePlacer == null)
                holePlacer = FindFirstObjectByType<DoorHolePlacer>();

            // GridMazeGenerator is created by CompleteMazeBuilder
        }

        #endregion

        #region JSON Config Loading

        /// <summary>
        /// Load ALL values from JSON config.
        /// NO HARDCODED VALUES - everything from GameConfig-default.json.
        /// </summary>
        private void LoadConfig()
        {
            var config = GameConfig.Instance;
            
            // Door placement settings from JSON
            placeDoorsInHoles = true;  // Always place if this component exists
            randomizeWallTextures = config.randomizeWallTextures;
            
            // Door variant settings from JSON
            enableTrappedDoors = config.enableTrappedDoors;
            trapChance = config.defaultTrapChance;
            enableLockedDoors = config.enableLockedDoors;
            enableSecretDoors = config.enableSecretDoors;

            Debug.Log($"[RoomDoorPlacer] 📖 Config loaded from JSON:");
            Debug.Log($"  • Trapped Doors: {enableTrappedDoors} ({trapChance * 100f:F0}% chance)");
            Debug.Log($"  • Locked Doors: {enableLockedDoors}");
            Debug.Log($"  • Secret Doors: {enableSecretDoors}");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Place doors in all reserved holes.
        /// Call after DoorHolePlacer.PlaceAllHoles().
        /// </summary>
        [ContextMenu("Place Doors in Holes")]
        public void PlaceAllDoors()
        {
            if (!placeDoorsInHoles)
            {
                Debug.Log("[RoomDoorPlacer] Door placement disabled");
                return;
            }

            if (holePlacer == null || holePlacer.PlacedHoles == null)
            {
                Debug.LogError("[RoomDoorPlacer] ❌ DoorHolePlacer not initialized!");
                return;
            }

            ClearPlacedDoors();

            Debug.Log("[RoomDoorPlacer] Starting door placement in reserved holes...");

            int doorsPlaced = 0;

            foreach (var hole in holePlacer.PlacedHoles)
            {
                PlaceDoorInHole(hole);
                doorsPlaced++;
            }

            Debug.Log($"[RoomDoorPlacer] ✅ Placed {doorsPlaced} doors in holes");
        }

        /// <summary>
        /// Clear all placed doors.
        /// </summary>
        [ContextMenu("Clear Placed Doors")]
        public void ClearPlacedDoors()
        {
            foreach (var door in _placedDoors)
            {
                if (door != null)
                    DestroyImmediate(door);
            }
            
            _placedDoors.Clear();
            _placedDoorData.Clear();
            
            Debug.Log("[RoomDoorPlacer] 🧹 Cleared all placed doors");
        }

        #endregion

        #region Door Placement

        /// <summary>
        /// Place a single door in a reserved hole.
        /// </summary>
        private void PlaceDoorInHole(DoorHolePlacer.DoorHoleData hole)
        {
            // Determine door variant (random based on config)
            DoorVariant variant = DetermineDoorVariant();
            DoorTrapType trap = DetermineDoorTrap(variant);

            // Create door using RealisticDoorFactory
            GameObject door = RealisticDoorFactory.CreateRealisticDoor(
                hole.Position,
                hole.Rotation,
                variant,
                trap,
                hole.Width,
                hole.Height,
                hole.Depth
            );

            // Store reference
            _placedDoors.Add(door);
            _placedDoorData.Add(new DoorData
            {
                GameObject = door,
                Variant = variant,
                TrapType = trap,
                GridPosition = hole.GridPosition,
                Direction = hole.Direction
            });

            if (showDebugGizmos)
            {
                Debug.Log($"[RoomDoorPlacer] 🚪 Placed {variant} door{(trap != DoorTrapType.None ? $" with {trap} trap" : "")} at {hole.Position}");
            }
        }

        /// <summary>
        /// Determine door variant based on config chances.
        /// </summary>
        private DoorVariant DetermineDoorVariant()
        {
            float roll = Random.value;
            
            // Secret door
            if (enableSecretDoors && roll < GameConfig.Instance.defaultSecretDoorChance)
                return DoorVariant.Secret;
            
            // Locked door
            if (enableLockedDoors && roll < GameConfig.Instance.defaultLockedDoorChance)
                return DoorVariant.Locked;
            
            // Normal door
            return DoorVariant.Normal;
        }

        /// <summary>
        /// Determine door trap type based on config chances.
        /// </summary>
        private DoorTrapType DetermineDoorTrap(DoorVariant variant)
        {
            // Don't add traps to certain variants
            if (variant == DoorVariant.Secret || variant == DoorVariant.Blessed)
                return DoorTrapType.None;
            
            // Roll for trap
            if (!enableTrappedDoors || Random.value > trapChance)
                return DoorTrapType.None;
            
            // Random trap type
            DoorTrapType[] availableTraps = new[]
            {
                DoorTrapType.Spike,
                DoorTrapType.Fire,
                DoorTrapType.Poison,
                DoorTrapType.Shock,
                DoorTrapType.Teleport,
                DoorTrapType.Alarm
            };
            
            return availableTraps[Random.Range(0, availableTraps.Length)];
        }

        #endregion

        #region Door Data

        /// <summary>
        /// Door data structure.
        /// Stores door instance, variant, and trap information.
        /// </summary>
        [System.Serializable]
        public class DoorData
        {
            public GameObject GameObject;      // Door GameObject
            public DoorVariant Variant;        // Door type
            public DoorTrapType TrapType;      // Trap type (if any)
            public Vector2Int GridPosition;    // Grid coordinates
            public Vector2Int Direction;       // Direction door faces
        }

        #endregion

        #region Debug

        [Header("🐛 Debug")]
        [SerializeField] private bool showDebugGizmos;

        #endregion
    }
}
