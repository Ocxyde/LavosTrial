// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// GameConstants.cs - Centralized constants for the entire project
// 
// Purpose: Replace magic numbers with named constants for better maintainability
// Usage: GameConstants.Player.EyeHeight, GameConstants.Colors.HealthHigh, etc.

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Centralized game constants. Replace magic numbers with these named values.
    /// Organized by category for easy navigation.
    /// </summary>
    public static class GameConstants
    {
        #region Player Constants

        public static class Player
        {
            // Camera & View
            public const float EyeHeight = 1.6f;              // Camera height (middle of eyes)
            public const float CameraZLock = 0f;               // Lock camera Z-axis
            public const float HeadBobFrequency = 10f;         // Head bob oscillation frequency
            public const float HeadBobAmplitude = 0.05f;       // Head bob vertical movement
            
            // Movement
            public const float MoveSpeed = 5f;                 // Base movement speed
            public const float SprintSpeedMultiplier = 1.1f;   // 10% speed boost
            public const float SprintStaminaCost = 0.01f;      // 1% stamina per second
            public const float JumpStaminaCost = 0.01f;        // 1% stamina per jump
            public const float MouseSensitivity = 2f;          // Mouse look sensitivity
            
            // Stats (defaults)
            public const float BaseHealth = 100f;
            public const float BaseMana = 50f;
            public const float BaseStamina = 100f;
            
            // Combat
            public const float InvincibilityTime = 0.5f;       // i-frames after damage
            public const float BaseCritChance = 0.05f;         // 5% base crit chance
            public const float BaseCritDamage = 1.5f;          // 150% crit damage
        }

        #endregion

        #region Maze Constants

        public static class Maze
        {
            // Cell dimensions
            public const float CellSize = 6f;                  // Size of each maze cell
            public const float CellHalfSize = CellSize * 0.5f; // Half cell size (for positioning)
            
            // Wall dimensions
            public const float WallThickness = 0.2f;           // Standard wall thickness
            public const float WallHeight = 4f;                // Standard wall height
            public const float DiagonalWallThickness = 0.5f;   // Diagonal wall thickness
            
            // Spawn positions
            public const float SpawnRoomSize = 5f;             // Spawn room size (5x5)
            public const float ExitRoomSize = 5f;              // Exit room size
            
            // Object placement
            public const float TorchHeight = 0f;               // Torch Y position
            public const float TorchSpacing = 4f;              // Minimum distance between torches
            public const int MaxTorches = 60;                  // Maximum torch count
            
            // Markers
            public const float MarkerHeight = 2f;              // Entrance/exit marker height
            public const float MarkerScale = 0.3f;             // Marker ring scale
            public const float MarkerLightIntensity = 2f;      // Marker light intensity
            
            // Generation
            public const float BaseDifficultyFactor = 1.0f;
            public const float DifficultyPerLevel = 0.15f;     // 15% increase per level
            public const float MaxDifficultyFactor = 3.0f;     // Cap at 3x
            public const int SizeGrowthPerLevel = 2;           // Maze grows by 2 cells per level
        }

        #endregion

        #region UI Constants

        public static class UI
        {
            // Bar colors (health)
            public static readonly Color HealthHigh = new Color(0.2f, 0.9f, 0.3f);     // 100%
            public static readonly Color HealthMid = new Color(0.9f, 0.7f, 0.1f);      // 50%
            public static readonly Color HealthCritical = new Color(0.9f, 0.1f, 0.1f); // 0%
            
            // Bar colors (mana)
            public static readonly Color ManaHigh = new Color(0.2f, 0.5f, 1.0f);       // 100%
            public static readonly Color ManaLow = new Color(0.1f, 0.25f, 0.5f);       // 0%
            
            // Bar colors (stamina)
            public static readonly Color StaminaHigh = new Color(1.0f, 0.8f, 0.2f);    // 100%
            public static readonly Color StaminaLow = new Color(0.5f, 0.4f, 0.1f);     // 0%
            
            // UI layout
            public const float BarSizePercent = 0.75f;         // Bar size relative to screen
            public const float BarBorderThickness = 2f;        // Border thickness
            
            // Floating text
            public const float FloatDuration = 1.5f;           // Default floating text duration
            public const float FloatSpeed = 40f;               // Floating text upward speed
            public const int FontSize = 32;                    // Default font size
            
            // Dialog
            public const float DialogDisplayTime = 3f;         // Default dialog display time
            public const float DialogFadeTime = 0.3f;          // Dialog fade in/out time
            public const float DialogWidth = 500f;             // Dialog width in pixels
            public const float DialogHeight = 120f;            // Dialog height in pixels
            
            // Window colors
            public static readonly Color WindowBg = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            public static readonly Color TitleBg = new Color(0.2f, 0.2f, 0.25f, 1f);
            public static readonly Color BorderColor = new Color(0.3f, 0.3f, 0.35f);
            public static readonly Color WindowBgDark = new Color(0.1f, 0.1f, 0.12f);
            
            // Animation
            public const float OpenDuration = 0.3f;            // Window open animation
            public const float CloseDuration = 0.2f;           // Window close animation
            public const float FlashDuration = 0.3f;           // Bar flash duration
        }

        #endregion

        #region Combat Constants

        public static class Combat
        {
            // Damage types (color coding)
            public static readonly Color PhysicalColor = Color.white;
            public static readonly Color FireColor = new Color(1f, 0.3f, 0.1f);
            public static readonly Color IceColor = new Color(0.3f, 0.7f, 1f);
            public static readonly Color LightningColor = new Color(1f, 1f, 0.3f);
            public static readonly Color NatureColor = new Color(0.3f, 1f, 0.3f);
            public static readonly Color HolyColor = new Color(1f, 0.9f, 0.5f);
            public static readonly Color DarkColor = new Color(0.5f, 0.3f, 0.7f);
            
            // Floating text colors
            public static readonly Color DamageColor = new Color(1f, 0.2f, 0.2f);
            public static readonly Color HealColor = new Color(0.2f, 1f, 0.3f);
            public static readonly Color ManaLossColor = new Color(0.3f, 0.6f, 1f);
            public static readonly Color ManaGainColor = Color.cyan;
            public static readonly Color StaminaLossColor = new Color(1f, 0.8f, 0.2f);
            public static readonly Color StaminaGainColor = Color.yellow;
            public static readonly Color CritColor = new Color(1f, 0.5f, 0f);
            public static readonly Color ShieldColor = new Color(0.8f, 0.4f, 1f);
        }

        #endregion

        #region Lighting Constants

        public static class Lighting
        {
            // Torch lighting
            public const float TorchRange = 15f;               // Light range
            public const float TorchIntensity = 5f;            // Light intensity
            public static readonly Color TorchColor = new Color(1f, 0.7f, 0.3f);  // Warm orange
            
            // Flicker settings
            public const float FlickerSpeed = 2f;              // Flicker oscillation speed
            public const float FlickerAmount = 0.15f;          // Flicker intensity (0-1)
            public const float EmissionFlickerSpeed = 1.5f;    // Emission flicker speed
            public const float EmissionFlickerAmount = 0.2f;   // Emission flicker amount
            
            // Fog of War
            public const float DarknessFalloff = 15f;          // Darkness falloff distance
            public const float DarknessTransitionSpeed = 0.5f; // Darkness transition speed
            public const float BaseDarkness = 0.3f;            // Base darkness level
            
            // Lightning effects
            public const float MinLightningInterval = 30f;     // Min time between flashes
            public const float MaxLightningInterval = 120f;    // Max time between flashes
            public const float LightningIntensityMult = 3f;    // Flash intensity multiplier
            public const float LightningDuration = 0.3f;       // Flash duration
            
            // Ambient light
            public static readonly Color AmbientColor = new Color(0.3f, 0.25f, 0.2f);
            public const float BaseAmbientIntensity = 0.6f;
        }

        #endregion

        #region Inventory Constants

        public static class Inventory
        {
            public const int DefaultCapacity = 20;             // Default inventory slots
            public const int DefaultColumns = 5;               // Default grid columns
            public const int DefaultRows = 4;                  // Default grid rows
            public const float SlotSize = 64f;                 // Slot size in pixels
            public const float SlotSpacing = 8f;               // Spacing between slots
            public const int MaxStack = 99;                    // Max items per stack
        }

        #endregion

        #region Physics Constants

        public static class Physics
        {
            // Character controller
            public const float SkinWidth = 0.08f;              // Character controller skin width
            public const float MinMoveDistance = 0.001f;       // Minimum move distance
            
            // Colliders
            public const float GroundOffset = -0.1f;           // Ground plane Y offset
            public const float GroundThickness = 0.1f;         // Ground plane thickness
            
            // Object placement
            public const float ObjectPlacementYOffset = 0f;    // Default Y offset for objects
        }

        #endregion

        #region Animation Constants

        public static class Animation
        {
            // Ring rotators
            public const float EntranceRotationSpeed = 30f;    // Entrance marker rotation
            public const float ExitRotationSpeed = -20f;       // Exit marker rotation (reverse)
            
            // Head bob
            public const float WalkBobSpeed = 10f;             // Walking head bob
            public const float WalkBobAmplitude = 0.05f;       // Walking bob height
            
            // General
            public const float SmoothTime = 0.1f;              // General smoothing time
        }

        #endregion

        #region Audio Constants

        public static class Audio
        {
            public const float MasterVolumeDefault = 1.0f;
            public const float MusicVolumeDefault = 0.8f;
            public const float SFXVolumeDefault = 1.0f;
            public const float VoiceVolumeDefault = 1.0f;
        }

        #endregion

        #region Time Constants

        public static class Time
        {
            public const float StaminaRegenDelay = 2.0f;       // Delay before stamina regen
            public const float StaminaRegenRate = 10f;         // Stamina per second
            public const float HealthRegenRate = 5f;           // Health per second (if enabled)
            public const float ManaRegenRate = 8f;             // Mana per second
        }

        #endregion

        #region Gizmo Constants

        public static class Gizmos
        {
            public static readonly Color MazeGridColor = new Color(0, 1, 0, 0.3f);
            public const float GizmoSphereRadius = 0.5f;
        }

        #endregion
    }
}
