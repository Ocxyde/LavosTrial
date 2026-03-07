// SpecialRoomPlacer.cs
// Procedurally places special rooms in the maze
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ENVIRONMENT: Places special rooms with unique atmospheres
// Location: Assets/Scripts/Core/08_Environment/

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// SpecialRoomPlacer - Procedurally places special rooms in the maze.
    /// 
    /// Features:
    /// - Random room placement
    /// - Multiple room types (treasure, trap, boss, secret)
    /// - No doors (open entrance)
    /// - Unique atmosphere per room type
    /// </summary>
    public class SpecialRoomPlacer : MonoBehaviour
    {
        [Header("Room Prefabs")]
        [Tooltip("Assign SpecialRoom prefab for each type")]
        [SerializeField] private SpecialRoom treasureRoomPrefab;
        [SerializeField] private SpecialRoom trapRoomPrefab;
        [SerializeField] private SpecialRoom bossRoomPrefab;
        [SerializeField] private SpecialRoom secretRoomPrefab;

        [Header("Placement Settings")]
        [Tooltip("Number of special rooms to place")]
        [SerializeField] private int roomCount = 3;
        
        [Tooltip("Minimum distance between special rooms")]
        [SerializeField] private float minDistanceBetweenRooms = 20f;
        
        [Tooltip("Try to place rooms near maze edges for variety")]
        [SerializeField] private bool preferEdgePlacement = true;

        [Header("Room Types")]
        [Tooltip("Chance for each room type (should add to 100%)")]
        [Range(0f, 100f)]
        [SerializeField] private float treasureChance = 40f;
        [Range(0f, 100f)]
        [SerializeField] private float trapChance = 30f;
        [Range(0f, 100f)]
        [SerializeField] private float bossChance = 10f;
        [Range(0f, 100f)]
        [SerializeField] private float secretChance = 20f;

        [Header("Visual")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color placedRoomGizmoColor = Color.yellow;

        private List<Vector3> placedRooms = new List<Vector3>();

        void Start()
        {
            PlaceSpecialRooms();
        }

        void PlaceSpecialRooms()
        {
            Debug.Log($"[SpecialRoomPlacer] Placing {roomCount} special rooms...");
            
            placedRooms.Clear();
            
            for (int i = 0; i < roomCount; i++)
            {
                Vector3 position = FindValidRoomPosition();
                
                if (position != Vector3.zero)
                {
                    SpecialRoom roomType = GetRandomRoomType();
                    if (roomType != null)
                    {
                        SpecialRoom newRoom = Instantiate(
                            roomType,
                            position,
                            Quaternion.identity,
                            transform
                        );
                        newRoom.gameObject.name = $"SpecialRoom_{i + 1}_{roomType.GetType().Name}";
                        placedRooms.Add(position);
                        
                        Debug.Log($"[SpecialRoomPlacer] Placed {roomType.GetType().Name} at {position}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SpecialRoomPlacer] Could not find valid position for room {i + 1}");
                }
            }
            
            Debug.Log($"[SpecialRoomPlacer] Placement complete: {placedRooms.Count} rooms placed");
        }

        Vector3 FindValidRoomPosition()
        {
            int maxAttempts = 50;
            int attempts = 0;
            
            while (attempts < maxAttempts)
            {
                Vector3 candidate;
                
                if (preferEdgePlacement)
                {
                    // Try to place near maze edges for variety
                    candidate = GetRandomEdgePosition();
                }
                else
                {
                    // Random position anywhere
                    candidate = GetRandomPosition();
                }
                
                // Check if position is valid (not too close to other rooms)
                if (IsValidPosition(candidate))
                {
                    return candidate;
                }
                
                attempts++;
            }
            
            return Vector3.zero; // Failed to find valid position
        }

        Vector3 GetRandomPosition()
        {
            // Get maze size from MazeGenerator if available
            float mazeSize = 100f; // Default fallback
            
            var mazeGen = FindObjectOfType<MazeGenerator>();
            if (mazeGen != null)
            {
                // Estimate maze size based on grid
                mazeSize = mazeGen.gridSize * 6f; // cellSize is typically 6f
            }
            
            float halfSize = mazeSize / 2f;
            return new Vector3(
                Random.Range(-halfSize, halfSize),
                0,
                Random.Range(-halfSize, halfSize)
            );
        }

        Vector3 GetRandomEdgePosition()
        {
            // Get maze size
            float mazeSize = 100f;
            var mazeGen = FindObjectOfType<MazeGenerator>();
            if (mazeGen != null)
            {
                mazeSize = mazeGen.gridSize * 6f;
            }
            
            float halfSize = mazeSize / 2f;
            
            // Pick a random edge (0=top, 1=right, 2=bottom, 3=left)
            int edge = Random.Range(0, 4);
            
            return edge switch
            {
                0 => new Vector3(Random.Range(-halfSize, halfSize), 0, -halfSize),  // Top
                1 => new Vector3(halfSize, 0, Random.Range(-halfSize, halfSize)),   // Right
                2 => new Vector3(Random.Range(-halfSize, halfSize), 0, halfSize),   // Bottom
                _ => new Vector3(-halfSize, 0, Random.Range(-halfSize, halfSize)),  // Left
            };
        }

        bool IsValidPosition(Vector3 position)
        {
            // Check distance from other placed rooms
            foreach (Vector3 placed in placedRooms)
            {
                if (Vector3.Distance(position, placed) < minDistanceBetweenRooms)
                {
                    return false;
                }
            }
            
            return true;
        }

        SpecialRoom GetRandomRoomType()
        {
            float roll = Random.Range(0f, 100f);
            float cumulative = 0f;
            
            cumulative += treasureChance;
            if (roll <= cumulative && treasureRoomPrefab != null)
                return treasureRoomPrefab;
            
            cumulative += trapChance;
            if (roll <= cumulative && trapRoomPrefab != null)
                return trapRoomPrefab;
            
            cumulative += bossChance;
            if (roll <= cumulative && bossRoomPrefab != null)
                return bossRoomPrefab;
            
            cumulative += secretChance;
            if (roll <= cumulative && secretRoomPrefab != null)
                return secretRoomPrefab;
            
            // Fallback to treasure if nothing else available
            return treasureRoomPrefab;
        }

        void OnDrawGizmos()
        {
            if (!showGizmos) return;
            
            foreach (Vector3 pos in placedRooms)
            {
                Gizmos.color = placedRoomGizmoColor;
                Gizmos.DrawWireSphere(pos, 2f);
            }
        }
    }
}
