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
// BraseroFlame.cs
// Brazier flame effect with 2D 8-bit pixel art style
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Core system - works with FlameAnimator.cs
//
// 8-BIT PIXEL ART FEATURES:
// - Discrete color bands (no smooth gradient)
// - 3 orange shades for retro shading
// - Particle-based animation (not deleted)
// - Pixel-perfect billboard rendering

using UnityEngine;

namespace Code.Lavos.Core
{
    public class BraseroFlame : MonoBehaviour
    {
    [Header("Particle Settings")]
    [SerializeField] private int maxParticles = 100;
    [SerializeField] private float particleSize = 0.15f;
    [SerializeField] private float particleSizeVariation = 0.05f;
    [SerializeField] private float particleLifetime = 0.8f;
    [SerializeField] private float emissionRate = 50f;
    [SerializeField] private float flameHeight = 0.6f;
    [SerializeField] private float flameWidth = 0.25f;

    [Header("2D 8-bit Flame Colors")]
    [Tooltip("Bright orange-yellow (8-bit shade: brightest pixel)")]
    [SerializeField] private Color coreColor = new Color(1f, 0.8f, 0.2f);  // Bright orange-yellow
    
    [Tooltip("Pure orange (8-bit shade: medium pixel)")]
    [SerializeField] private Color middleColor = new Color(1f, 0.5f, 0.1f);  // Pure orange
    
    [Tooltip("Dark orange-red (8-bit shade: dark pixel)")]
    [SerializeField] private Color outerColor = new Color(0.9f, 0.3f, 0.05f);  // Dark orange-red

    [Header("Movement")]
    [SerializeField] private float turbulence = 0.3f;
    [SerializeField] private float flickerSpeed = 8f;
    [SerializeField] private float flickerAmount = 0.2f;

    private ParticleSystem _particleSystem;
    private float _randomOffset;

    void Awake()
    {
        _randomOffset = Random.Range(0f, 100f);
        SetupParticleSystem();
    }

    void Start()
    {
        // Ensure particles are playing
        if (_particleSystem != null && !_particleSystem.isPlaying)
        {
            _particleSystem.Play();
            Debug.Log($"[BraseroFlame] Particles started at {transform.position}");
        }
    }

    void OnEnable()
    {
        // Restart particles when enabled
        if (_particleSystem != null)
        {
            _particleSystem.Play();
        }
    }

    private void SetupParticleSystem()
    {
        if (gameObject == null) return;

        var config = CreateCampfireConfig();
        _particleSystem = ParticleGenerator.CreateParticleSystem(gameObject, config);
    }

    private ParticleConfig CreateCampfireConfig()
    {
        var config = new ParticleConfig
        {
            maxParticles = maxParticles,
            startLifetime = particleLifetime,
            startLifetimeMax = particleLifetime * 1.5f,
            startSpeed = flameHeight * 0.5f,
            startSpeedMax = flameHeight * 1.2f,
            startSize = particleSize - particleSizeVariation,
            startSizeMax = particleSize + particleSizeVariation,
            startColor = coreColor,
            simulationSpace = ParticleSystemSimulationSpace.Local,
            loop = true,
            playOnAwake = true,
            emissionRate = emissionRate,
            shapeType = ParticleSystemShapeType.Cone,
            shapeAngle = 15f,
            shapeRadius = flameWidth * 0.5f,
            shapePosition = Vector3.zero,
            shapeRotation = new Vector3(-90f, 0f, 0f),
            useVelocityOverLifetime = true,
            velocitySpace = ParticleSystemSimulationSpace.Local,
            useVelocityCurves = true,
            velocityXMin = -turbulence,
            velocityXMax = turbulence,
            velocityYMin = turbulence,
            velocityYMax = turbulence * 2f,
            velocityZMin = -turbulence,
            velocityZMax = turbulence,
            useColorOverLifetime = true,
            colorGradient = CreateFireGradient(),
            useSizeOverLifetime = true,
            separateAxes = true,
            sizeMultiplier = 1f,
            renderMode = ParticleSystemRenderMode.Billboard,
            enableEmissionKeyword = true,
            emissionColor = new Color(1f, 0.7f, 0.3f, 1f)  // Orange emission (no pink)
        };

        // Ensure curves exist; fallback to simple linear curves if helper missing
        config.velocityXCurve = ParticleGenerator.CreateSmoothCurve(-turbulence, turbulence, -turbulence);
        config.velocityZCurve = ParticleGenerator.CreateSmoothCurve(-turbulence, turbulence, -turbulence);
        config.sizeCurve = ParticleGenerator.CreateSmoothCurve(0.3f, 1f, 0.1f);

        return config;
    }

    private Gradient CreateFireGradient()
    {
        // 8-BIT STYLE: Sharp color transitions (no smooth gradient)
        // Creates discrete color bands for retro pixel art shading
        return ParticleGenerator.CreateGradient(
            new Color[] { 
                coreColor,   // Bright orange (top)
                coreColor,   // Hold bright
                middleColor, // Pure orange (middle)
                middleColor, // Hold pure
                outerColor,  // Dark orange-red (bottom)
                outerColor,  // Hold dark
                Color.clear  // Fade out at end
            },
            new float[] { 
                0.0f,  // Bright starts
                0.15f, // Bright holds
                0.2f,  // Middle starts
                0.5f,  // Middle holds
                0.55f, // Dark starts
                0.8f,  // Dark holds
                1.0f   // Fade out
            },
            new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 0f }, // Alpha
            new float[] { 0f, 0.15f, 0.2f, 0.5f, 0.55f, 0.8f, 1f } // Keys
        );
    }

    void Update()
    {
        if (_particleSystem == null) return;

        float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed + _randomOffset, 0f);
        flicker = 1f + (flicker - 0.5f) * flickerAmount;

        var emission = _particleSystem.emission;
        emission.rateOverTime = emissionRate * flicker;

        float currentSize = particleSize * flicker;
        var main = _particleSystem.main;
        main.startSize = new ParticleSystem.MinMaxCurve(currentSize - particleSizeVariation, currentSize + particleSizeVariation);
    }

    public void SetIntensity(float intensity)
    {
        if (_particleSystem == null) return;

        var emission = _particleSystem.emission;
        emission.rateOverTime = emissionRate * Mathf.Clamp01(intensity);
    }
}
}
