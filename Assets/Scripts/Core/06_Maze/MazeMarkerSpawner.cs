// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// MazeMarkerSpawner.cs - Handles spawning of entrance/exit markers

using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Handles spawning of entrance/exit markers with visual effects.
    /// </summary>
    public static class MazeMarkerSpawner
    {
        /// <summary>
        /// Spawn entrance and exit room markers with visual effects.
        /// </summary>
        public static void SpawnRoomMarkers(
            DungeonMazeData mazeData,
            float cellSize,
            float markerHeight,
            float markerScale,
            float markerLightIntensity)
        {
            if (mazeData == null)
            {
                Debug.LogError("[MazeMarkerSpawner] Cannot spawn markers - mazeData is null!");
                return;
            }

            // Entrance marker (GREEN - spawn point)
            Vector3 entrancePos = new Vector3(
                (mazeData.SpawnCell.x + 0.5f) * cellSize,
                0f,
                (mazeData.SpawnCell.z + 0.5f) * cellSize
            );
            SpawnEnhancedMarker(entrancePos, Color.green, "EntranceMarker", true,
                markerHeight, markerScale, markerLightIntensity);

            // Exit marker (RED - goal)
            Vector3 exitPos = new Vector3(
                (mazeData.ExitCell.x + 0.5f) * cellSize,
                0f,
                (mazeData.ExitCell.z + 0.5f) * cellSize
            );
            SpawnEnhancedMarker(exitPos, Color.red, "ExitMarker", false,
                markerHeight, markerScale, markerLightIntensity);

            Debug.Log($"[MazeMarkerSpawner] Spawned entrance/exit markers");
        }

        private static void SpawnEnhancedMarker(
            Vector3 position,
            Color color,
            string name,
            bool isEntrance,
            float markerHeight,
            float markerScale,
            float markerLightIntensity)
        {
            // Create marker parent
            var markerObj = new GameObject(name);
            markerObj.transform.position = position;

            // Spawn floating ring
            SpawnFloatingRing(position, color, isEntrance, markerScale);

            // Add point light
            var lightObj = new GameObject($"{name}_Light");
            lightObj.transform.SetParent(markerObj.transform);
            lightObj.transform.localPosition = new Vector3(0f, markerHeight * 0.5f, 0f);

            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = 10f;
            light.intensity = markerLightIntensity;
            light.shadows = LightShadows.None;

            // Add particle system
            var particleObj = new GameObject($"{name}_Particles");
            particleObj.transform.SetParent(markerObj.transform);
            particleObj.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            var particleSystem = particleObj.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.startColor = color;
            main.startSize = 0.1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;
            main.playOnAwake = true;

            var emission = particleSystem.emission;
            emission.rateOverTime = 10f;

            Debug.Log($"[MazeMarkerSpawner] {name} spawned at {position}");
        }

        private static void SpawnFloatingRing(
            Vector3 position,
            Color color,
            bool isEntrance,
            float scale)
        {
            // Create ring mesh
            var ringObj = new GameObject("FloatingRing");
            ringObj.transform.position = new Vector3(position.x, 0.5f, position.z);
            ringObj.transform.localScale = Vector3.one * scale;

            // Add mesh filter and renderer
            var filter = ringObj.AddComponent<MeshFilter>();
            filter.mesh = CreateTorusMesh(0.3f, 0.05f, 32);

            var renderer = ringObj.AddComponent<MeshRenderer>();
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = color;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2f);
            renderer.material = material;

            // Add rotation animation
            var rotator = ringObj.AddComponent<MazeRingRotator>();
            rotator.rotationSpeed = isEntrance ? 30f : -20f;
        }

        private static Mesh CreateTorusMesh(float radius, float thickness, int segments)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Torus";

            var vertices = new Vector3[segments * 16];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[segments * 16 * 3];

            int vertIndex = 0;
            int triIndex = 0;

            for (int i = 0; i < segments; i++)
            {
                float u = (float)i / segments * Mathf.PI * 2f;
                float uNext = (float)(i + 1) / segments * Mathf.PI * 2f;

                for (int j = 0; j < 16; j++)
                {
                    float v = (float)j / 16 * Mathf.PI * 2f;

                    float x = (radius + thickness * Mathf.Cos(v)) * Mathf.Cos(u);
                    float y = thickness * Mathf.Sin(v);
                    float z = (radius + thickness * Mathf.Cos(v)) * Mathf.Sin(u);

                    vertices[vertIndex] = new Vector3(x, y, z);
                    normals[vertIndex] = new Vector3(Mathf.Cos(u) * Mathf.Cos(v), Mathf.Sin(v), Mathf.Sin(u) * Mathf.Cos(v));
                    uvs[vertIndex] = new Vector2((float)i / segments, (float)j / 16);
                    vertIndex++;
                }

                if (i > 0)
                {
                    int baseVert = (i - 1) * 16;
                    for (int j = 0; j < 15; j++)
                    {
                        triangles[triIndex++] = baseVert + j;
                        triangles[triIndex++] = baseVert + j + 1;
                        triangles[triIndex++] = baseVert + j + 16;

                        triangles[triIndex++] = baseVert + j + 16;
                        triangles[triIndex++] = baseVert + j + 1;
                        triangles[triIndex++] = baseVert + j + 17;
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            return mesh;
        }
    }

    /// <summary>
    /// Simple component to rotate marker rings.
    /// </summary>
    public class MazeRingRotator : MonoBehaviour
    {
        public float rotationSpeed = 30f;

        void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
