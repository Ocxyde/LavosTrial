// AddDoorSystemToScene.cs
// Editor script to add complete door system to existing maze
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Usage: Select Maze GameObject, run "Add Door System Components"

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Code.Lavos.Core
{
    /// <summary>
    /// AddDoorSystemToScene - Add missing door components to maze.
    /// Run from Editor menu or Inspector context menu.
    /// </summary>
    public class AddDoorSystemToScene : MonoBehaviour
    {
        [MenuItem("Tools/PeuImporte/Add Door System to Maze")]
        public static void AddDoorSystemToMaze()
        {
            // Find MazeGenerator in scene using Unity 6 API
            var mazeGenerators = Object.FindObjectsByType<MazeGenerator>(FindObjectsSortMode.None);

            GameObject mazeObj = null;
            MazeGenerator mazeGen = null;

            if (mazeGenerators.Length > 0)
            {
                mazeGen = mazeGenerators[0];
                mazeObj = mazeGen.gameObject;
                Debug.Log($"✅ Found existing Maze: {mazeObj.name}");
            }
            else
            {
                // Create new Maze GameObject
                mazeObj = new GameObject("Maze");
                mazeGen = mazeObj.AddComponent<MazeGenerator>();
                Debug.Log("✅ Created new Maze GameObject");
            }

            // Select it
            Selection.activeGameObject = mazeObj;

            // Add missing components
            int added = 0;

            if (mazeObj.GetComponent<RoomGenerator>() == null)
            {
                mazeObj.AddComponent<RoomGenerator>();
                Debug.Log("✅ Added RoomGenerator");
                added++;
            }
            else
            {
                Debug.Log("✅ RoomGenerator already present");
            }

            if (mazeObj.GetComponent<DoorHolePlacer>() == null)
            {
                mazeObj.AddComponent<DoorHolePlacer>();
                Debug.Log("✅ Added DoorHolePlacer");
                added++;
            }
            else
            {
                Debug.Log("✅ DoorHolePlacer already present");
            }

            if (mazeObj.GetComponent<RoomDoorPlacer>() == null)
            {
                mazeObj.AddComponent<RoomDoorPlacer>();
                Debug.Log("✅ Added RoomDoorPlacer");
                added++;
            }
            else
            {
                Debug.Log("✅ RoomDoorPlacer already present");
            }

            if (mazeObj.GetComponent<MazeRenderer>() == null)
            {
                mazeObj.AddComponent<MazeRenderer>();
                Debug.Log("✅ Added MazeRenderer");
                added++;
            }
            else
            {
                Debug.Log("✅ MazeRenderer already present");
            }

            if (mazeObj.GetComponent<MazeIntegration>() == null)
            {
                mazeObj.AddComponent<MazeIntegration>();
                Debug.Log("✅ Added MazeIntegration");
                added++;
            }
            else
            {
                Debug.Log("✅ MazeIntegration already present");
            }

            if (mazeObj.GetComponent<SeedManager>() == null)
            {
                mazeObj.AddComponent<SeedManager>();
                Debug.Log("✅ Added SeedManager");
                added++;
            }
            else
            {
                Debug.Log("✅ SeedManager already present");
            }

            if (mazeObj.GetComponent<DrawingPool>() == null)
            {
                mazeObj.AddComponent<DrawingPool>();
                Debug.Log("✅ Added DrawingPool");
                added++;
            }
            else
            {
                Debug.Log("✅ DrawingPool already present");
            }

            Debug.Log($"=============================");
            Debug.Log($"✅ Door System Setup Complete!");
            Debug.Log($"   Components added: {added}");
            Debug.Log($"   Maze GameObject: {mazeObj.name}");
            Debug.Log($"=============================");
            Debug.Log($"▶️ Press Play to generate maze with rooms and doors!");
        }

        [ContextMenu("Add Door System Components")]
        public void AddComponents()
        {
            AddDoorSystemToMaze();
        }
    }
}
#endif
