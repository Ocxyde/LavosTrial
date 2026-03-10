// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// MazeMarkerSpawner.cs - Handles spawning of entrance/exit markers

using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Handles spawning of entrance/exit markers with visual effects.
    /// PLUG-IN-OUT: Works with MazeData8 through EventHandler events.
    /// </summary>
    public static class MazeMarkerSpawner
    {
        /// <summary>
        /// Spawn entrance and exit room markers with visual effects.
        /// </summary>
        public static void SpawnRoomMarkers(
            MazeData8 mazeData,
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

            int radialSegments = 12;
            int totalVertices = segments * radialSegments;
            var vertices = new Vector3[totalVertices];
            var normals = new Vector3[totalVertices];
            var uvs = new Vector2[totalVertices];

            int vertIndex = 0;
            for (int i = 0; i < segments; i++)
            {
                float u = (float)i / segments * Mathf.PI * 2f;
                for (int j = 0; j < radialSegments; j++)
                {
                    float v = (float)j / radialSegments * Mathf.PI * 2f;

                    float cosU = Mathf.Cos(u);
                    float sinU = Mathf.Sin(u);
                    float cosV = Mathf.Cos(v);
                    float sinV = Mathf.Sin(v);

                    float x = (radius + thickness * cosV) * cosU;
                    float y = thickness * sinV;
                    float z = (radius + thickness * cosV) * sinU;

                    vertices[vertIndex] = new Vector3(x, y, z);
                    normals[vertIndex] = new Vector3(cosU * cosV, sinV, sinU * cosV);
                    uvs[vertIndex] = new Vector2((float)i / segments, (float)j / radialSegments);
                    vertIndex++;
                }
            }

            int totalTriangles = segments * radialSegments * 6;
            var triangles = new int[totalTriangles];

            int triIndex = 0;
            for (int i = 0; i < segments; i++)
            {
                int nextI = (i + 1) % segments;
                for (int j = 0; j < radialSegments; j++)
                {
                    int nextJ = (j + 1) % radialSegments;
                    int current = i * radialSegments + j;
                    int nextRowCurrent = nextI * radialSegments + j;
                    int currentNextJ = i * radialSegments + nextJ;
                    int nextRowNextJ = nextI * radialSegments + nextJ;

                    triangles[triIndex++] = current;
                    triangles[triIndex++] = nextRowCurrent;
                    triangles[triIndex++] = currentNextJ;

                    triangles[triIndex++] = currentNextJ;
                    triangles[triIndex++] = nextRowCurrent;
                    triangles[triIndex++] = nextRowNextJ;
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
