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
// ParticleGenerator.cs
// Particle system generator for VFX
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Core system - creates particle effects

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    public enum ParticlePreset
    {
    None,
    CampfireFlame,
    AmbientFog,
    Smoke,
    Sparks,
    Rain,
    Snow,
    Dust,
    Explosion
}

[System.Serializable]
public class ParticleConfig
{
    [Header("Main Module")]
    public float startLifetime = 1f;
    public float startLifetimeMax = 2f;
    public float startSpeed = 1f;
    public float startSpeedMax = 2f;
    public float startSize = 0.15f;
    public float startSizeMax = 0.2f;
    public Color startColor = Color.white;
    public ParticleSystemSimulationSpace simulationSpace = ParticleSystemSimulationSpace.Local;
    public int maxParticles = 100;
    public bool loop = true;
    public bool playOnAwake = true;

    [Header("Emission")]
    public float emissionRate = 50f;

    [Header("Shape")]
    public ParticleSystemShapeType shapeType = ParticleSystemShapeType.Cone;
    public float shapeAngle = 25f;
    public float shapeRadius = 0.5f;
    public Vector3 shapePosition = Vector3.zero;
    public Vector3 shapeRotation = Vector3.zero;

    [Header("Velocity Over Lifetime")]
    public bool useVelocityOverLifetime = false;
    public ParticleSystemSimulationSpace velocitySpace = ParticleSystemSimulationSpace.Local;
    public float velocityXMin = -0.5f;
    public float velocityXMax = 0.5f;
    public float velocityYMin = 1f;
    public float velocityYMax = 2f;
    public float velocityZMin = -0.5f;
    public float velocityZMax = 0.5f;
    public bool useVelocityCurves = false;
    public AnimationCurve velocityXCurve;
    public AnimationCurve velocityZCurve;

    [Header("Color Over Lifetime")]
    public bool useColorOverLifetime = false;
    public Gradient colorGradient;

    [Header("Size Over Lifetime")]
    public bool useSizeOverLifetime = false;
    public bool separateAxes = false;
    public float sizeMultiplier = 1f;
    public AnimationCurve sizeCurve;

    [Header("Renderer")]
    public ParticleSystemRenderMode renderMode = ParticleSystemRenderMode.Billboard;
    public string shaderName = "Particles/Standard Unlit";
    public bool enableEmissionKeyword = true;
    public Color emissionColor = Color.white;

    [Header("Presets")]
    public ParticlePreset preset = ParticlePreset.None;

    public static ParticleConfig CreateCampfireFlame()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.CampfireFlame,
            startLifetime = 0.8f,
            startLifetimeMax = 1.2f,
            startSpeed = 0.3f,
            startSpeedMax = 0.72f,
            startSize = 0.15f,
            startSizeMax = 0.2f,
            startColor = new Color(1f, 0.9f, 0.3f),
            simulationSpace = ParticleSystemSimulationSpace.Local,
            maxParticles = 100,
            loop = true,
            playOnAwake = true,
            emissionRate = 50f,
            shapeType = ParticleSystemShapeType.Cone,
            shapeAngle = 15f,
            shapeRadius = 0.125f,
            shapePosition = Vector3.zero,
            shapeRotation = new Vector3(-90f, 0f, 0f),
            useVelocityOverLifetime = true,
            velocitySpace = ParticleSystemSimulationSpace.Local,
            useVelocityCurves = true,
            velocityXMin = -0.3f,
            velocityXMax = 0.3f,
            velocityYMin = 0.3f,
            velocityYMax = 0.72f,
            velocityZMin = -0.3f,
            velocityZMax = 0.3f,
            useColorOverLifetime = true,
            colorGradient = CreateFireGradient(),
            useSizeOverLifetime = true,
            separateAxes = true,
            sizeMultiplier = 1f,
            renderMode = ParticleSystemRenderMode.Billboard,
            enableEmissionKeyword = true,
            emissionColor = new Color(1f, 0.9f, 0.3f) * 2f
        };
    }

    public static ParticleConfig CreateAmbientFog()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.AmbientFog,
            startLifetime = 5f,
            startLifetimeMax = 10f,
            startSpeed = 0.1f,
            startSpeedMax = 0.3f,
            startSize = 2f,
            startSizeMax = 4f,
            startColor = new Color(0.8f, 0.8f, 0.85f, 0.3f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            maxParticles = 50,
            loop = true,
            playOnAwake = true,
            emissionRate = 10f,
            shapeType = ParticleSystemShapeType.Box,
            shapeAngle = 0f,
            shapeRadius = 5f,
            shapePosition = Vector3.zero,
            shapeRotation = Vector3.zero,
            useVelocityOverLifetime = true,
            velocitySpace = ParticleSystemSimulationSpace.World,
            velocityXMin = -0.1f,
            velocityXMax = 0.1f,
            velocityYMin = 0.05f,
            velocityYMax = 0.1f,
            velocityZMin = -0.1f,
            velocityZMax = 0.1f,
            useColorOverLifetime = true,
            colorGradient = CreateFogGradient(),
            useSizeOverLifetime = true,
            separateAxes = false,
            sizeMultiplier = 1f,
            renderMode = ParticleSystemRenderMode.Billboard,
            enableEmissionKeyword = false,
            emissionColor = Color.white
        };
    }

    public static ParticleConfig CreateSmoke()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Smoke,
            startLifetime = 2f,
            startLifetimeMax = 4f,
            startSpeed = 0.5f,
            startSpeedMax = 1f,
            startSize = 0.5f,
            startSizeMax = 1.5f,
            startColor = new Color(0.3f, 0.3f, 0.3f, 0.5f),
            simulationSpace = ParticleSystemSimulationSpace.Local,
            maxParticles = 50,
            loop = true,
            playOnAwake = true,
            emissionRate = 20f,
            shapeType = ParticleSystemShapeType.Cone,
            shapeAngle = 20f,
            shapeRadius = 0.3f,
            shapePosition = Vector3.zero,
            shapeRotation = new Vector3(-90f, 0f, 0f),
            useVelocityOverLifetime = true,
            velocitySpace = ParticleSystemSimulationSpace.Local,
            velocityXMin = -0.2f,
            velocityXMax = 0.2f,
            velocityYMin = 0.5f,
            velocityYMax = 1f,
            velocityZMin = -0.2f,
            velocityZMax = 0.2f,
            useColorOverLifetime = true,
            colorGradient = CreateSmokeGradient(),
            useSizeOverLifetime = true,
            separateAxes = false,
            sizeMultiplier = 1f,
            renderMode = ParticleSystemRenderMode.Billboard,
            enableEmissionKeyword = false,
            emissionColor = Color.white
        };
    }

    public static ParticleConfig CreateSparks()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Sparks,
            startLifetime = 0.5f,
            startLifetimeMax = 1f,
            startSpeed = 2f,
            startSpeedMax = 4f,
            startSize = 0.05f,
            startSizeMax = 0.1f,
            startColor = new Color(1f, 0.9f, 0.5f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            maxParticles = 30,
            loop = true,
            playOnAwake = true,
            emissionRate = 15f,
            shapeType = ParticleSystemShapeType.Sphere,
            shapeAngle = 0f,
            shapeRadius = 0.1f,
            shapePosition = Vector3.zero,
            shapeRotation = Vector3.zero,
            useVelocityOverLifetime = true,
            velocitySpace = ParticleSystemSimulationSpace.World,
            velocityXMin = -1f,
            velocityXMax = 1f,
            velocityYMin = 1f,
            velocityYMax = 3f,
            velocityZMin = -1f,
            velocityZMax = 1f,
            useColorOverLifetime = true,
            colorGradient = CreateSparkGradient(),
            useSizeOverLifetime = true,
            separateAxes = false,
            sizeMultiplier = 1f,
            renderMode = ParticleSystemRenderMode.Stretch,
            enableEmissionKeyword = true,
            emissionColor = Color.white * 2f
        };
    }

    private static Gradient CreateFireGradient()
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.9f, 0.3f), 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0.3f),
                new GradientColorKey(new Color(0.8f, 0.2f, 0.05f), 0.7f),
                new GradientColorKey(new Color(0.8f, 0.2f, 0.05f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0.4f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        return gradient;
    }

    private static Gradient CreateFogGradient()
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.8f, 0.8f, 0.85f), 0f),
                new GradientColorKey(new Color(0.7f, 0.7f, 0.75f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.3f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        return gradient;
    }

    private static Gradient CreateSmokeGradient()
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.4f, 0.4f, 0.4f), 0f),
                new GradientColorKey(new Color(0.2f, 0.2f, 0.2f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.5f, 0f),
                new GradientAlphaKey(0.3f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        return gradient;
    }

    private static Gradient CreateSparkGradient()
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 1f, 0.8f), 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f),
                new GradientColorKey(new Color(0.5f, 0.1f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        return gradient;
    }
}

public static class ParticleGenerator
{
    private static Shader _defaultParticleShader;
    private static Material _defaultParticleMaterial;
    private static readonly Dictionary<string, Material> MaterialCache = new();
    private static readonly Dictionary<string, Shader> ShaderCache = new();

    public static ParticleSystem CreateParticleSystem(GameObject target, ParticleConfig config)
    {
        if (target == null)
        {
            Debug.LogWarning("ParticleGenerator: Target GameObject is null.");
            return null;
        }

        var existingSystem = target.GetComponent<ParticleSystem>();
        if (existingSystem != null)
        {
            Object.Destroy(existingSystem);
        }

        var particleSystem = target.AddComponent<ParticleSystem>();
        ConfigureParticleSystem(particleSystem, config);
        particleSystem.Play();
        return particleSystem;
    }

    public static ParticleSystem CreateParticleSystem(GameObject target, ParticlePreset preset)
    {
        var config = preset switch
        {
            ParticlePreset.CampfireFlame => ParticleConfig.CreateCampfireFlame(),
            ParticlePreset.AmbientFog => ParticleConfig.CreateAmbientFog(),
            ParticlePreset.Smoke => ParticleConfig.CreateSmoke(),
            ParticlePreset.Sparks => ParticleConfig.CreateSparks(),
            _ => new ParticleConfig()
        };
        return CreateParticleSystem(target, config);
    }

    public static void ConfigureParticleSystem(ParticleSystem particleSystem, ParticleConfig config)
    {
        if (particleSystem == null || config == null) return;

        ConfigureMainModule(particleSystem.main, config);
        ConfigureEmissionModule(particleSystem.emission, config);
        ConfigureShapeModule(particleSystem.shape, config);
        ConfigureRenderer(particleSystem.GetComponent<ParticleSystemRenderer>(), config);

        if (config.useVelocityOverLifetime)
        {
            ConfigureVelocityOverLifetime(particleSystem.velocityOverLifetime, config);
        }

        if (config.useColorOverLifetime)
        {
            ConfigureColorOverLifetime(particleSystem.colorOverLifetime, config);
        }

        if (config.useSizeOverLifetime)
        {
            ConfigureSizeOverLifetime(particleSystem.sizeOverLifetime, config);
        }
    }

    private static void ConfigureMainModule(ParticleSystem.MainModule main, ParticleConfig config)
    {
        main.startLifetime = new ParticleSystem.MinMaxCurve(config.startLifetime, config.startLifetimeMax);
        main.startSpeed = new ParticleSystem.MinMaxCurve(config.startSpeed, config.startSpeedMax);
        main.startSize = new ParticleSystem.MinMaxCurve(config.startSize, config.startSizeMax);
        main.startColor = config.startColor;
        main.simulationSpace = config.simulationSpace;
        main.maxParticles = config.maxParticles;
        main.loop = config.loop;
        main.playOnAwake = config.playOnAwake;
    }

    private static void ConfigureEmissionModule(ParticleSystem.EmissionModule emission, ParticleConfig config)
    {
        emission.rateOverTime = config.emissionRate;
    }

    private static void ConfigureShapeModule(ParticleSystem.ShapeModule shape, ParticleConfig config)
    {
        shape.shapeType = config.shapeType;
        shape.angle = config.shapeAngle;
        shape.radius = config.shapeRadius;
        shape.position = config.shapePosition;
        shape.rotation = config.shapeRotation;
    }

    private static void ConfigureVelocityOverLifetime(ParticleSystem.VelocityOverLifetimeModule velocity, ParticleConfig config)
    {
        velocity.enabled = true;
        velocity.space = config.velocitySpace;

        if (config.useVelocityCurves)
        {
            if (config.velocityXCurve != null)
            {
                var minMaxX = new ParticleSystem.MinMaxCurve();
                minMaxX.mode = ParticleSystemCurveMode.Curve;
                minMaxX.curve = config.velocityXCurve;
                velocity.x = minMaxX;
            }

            if (config.velocityZCurve != null)
            {
                var minMaxZ = new ParticleSystem.MinMaxCurve();
                minMaxZ.mode = ParticleSystemCurveMode.Curve;
                minMaxZ.curve = config.velocityZCurve;
                velocity.z = minMaxZ;
            }
        }
        else
        {
            velocity.x = new ParticleSystem.MinMaxCurve(config.velocityXMin, config.velocityXMax);
            velocity.y = new ParticleSystem.MinMaxCurve(config.velocityYMin, config.velocityYMax);
            velocity.z = new ParticleSystem.MinMaxCurve(config.velocityZMin, config.velocityZMax);
        }
    }

    private static void ConfigureColorOverLifetime(ParticleSystem.ColorOverLifetimeModule color, ParticleConfig config)
    {
        color.enabled = true;
        if (config.colorGradient != null)
        {
            color.color = config.colorGradient;
        }
    }

    private static void ConfigureSizeOverLifetime(ParticleSystem.SizeOverLifetimeModule size, ParticleConfig config)
    {
        size.enabled = true;
        size.separateAxes = config.separateAxes;

        if (config.sizeCurve != null)
        {
            size.size = new ParticleSystem.MinMaxCurve(config.sizeMultiplier, config.sizeCurve);
        }
        else
        {
            size.size = config.sizeMultiplier;
        }
    }

    private static void ConfigureRenderer(ParticleSystemRenderer renderer, ParticleConfig config)
    {
        if (renderer == null) return;

        renderer.renderMode = config.renderMode;
        renderer.material = GetOrCreateMaterial(config.shaderName, config.enableEmissionKeyword, config.emissionColor);
    }

    private static Shader _fallbackShader;
    private static bool _fallbackShaderInitialized;

    private static Material GetOrCreateMaterial(string shaderName, bool enableEmission, Color emissionColor)
    {
        string key = $"{shaderName}_{enableEmission}";

        if (MaterialCache.TryGetValue(key, out var cachedMaterial))
        {
            cachedMaterial.SetColor("_EmissionColor", emissionColor);
            return cachedMaterial;
        }

        Shader shader = GetShader(shaderName);

        if (shader == null)
        {
            Debug.LogWarning($"ParticleGenerator: Shader '{shaderName}' not found. Using fallback shader.");
            shader = GetFallbackShader();
        }

        if (shader == null)
        {
            Debug.LogError("ParticleGenerator: No valid particle shader found. Creating material with default shader.");
            return null;
        }

        var material = new Material(shader);
        if (enableEmission)
        {
            material.EnableKeyword("_EMISSION");
        }
        material.SetColor("_EmissionColor", emissionColor);

        MaterialCache[key] = material;
        return material;
    }

    private static Shader GetShader(string shaderName)
    {
        if (string.IsNullOrEmpty(shaderName)) return null;

        if (ShaderCache.TryGetValue(shaderName, out var cachedShader))
        {
            return cachedShader;
        }

        var shader = Shader.Find(shaderName);
        if (shader != null)
        {
            ShaderCache[shaderName] = shader;
        }
        return shader;
    }

    private static Shader GetFallbackShader()
    {
        if (_fallbackShaderInitialized) return _fallbackShader;

        string[] fallbacks = new[]
        {
            "Particles/Standard Unlit",
            "Particles/Standard Surface",
            "Legacy Shaders/Particles/Alpha Blended",
            "Sprites/Default",
            "Unlit/Transparent"
        };

        foreach (var fallback in fallbacks)
        {
            var shader = Shader.Find(fallback);
            if (shader != null)
            {
                _fallbackShader = shader;
                Debug.Log($"[ParticleGenerator] Using fallback shader: {fallback}");
                break;
            }
        }

        _fallbackShaderInitialized = true;
        return _fallbackShader;
    }

    public static AnimationCurve CreateSmoothCurve(float start, float mid, float end)
    {
        var curve = new AnimationCurve();
        curve.AddKey(CreateSmoothKey(0f, start));
        curve.AddKey(CreateSmoothKey(0.5f, mid));
        curve.AddKey(CreateSmoothKey(1f, end));
        return curve;
    }

    public static Keyframe CreateSmoothKey(float time, float value)
    {
        var key = new Keyframe(time, value);
        key.inTangent = 0f;
        key.outTangent = 0f;
        return key;
    }

    public static AnimationCurve CreateLinearCurve(float start, float end)
    {
        var curve = new AnimationCurve();
        curve.AddKey(0f, start);
        curve.AddKey(1f, end);
        return curve;
    }

    public static AnimationCurve CreateBellCurve(float peak = 1f, float start = 0f, float end = 0f)
    {
        return CreateSmoothCurve(start, peak, end);
    }

    public static Gradient CreateGradient(Color[] colors, float[] colorPositions, float[] alphas, float[] alphaPositions)
    {
        var gradient = new Gradient();

        var colorKeys = new GradientColorKey[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            colorKeys[i] = new GradientColorKey(colors[i], colorPositions[i]);
        }

        var alphaKeys = new GradientAlphaKey[alphas.Length];
        for (int i = 0; i < alphas.Length; i++)
        {
            alphaKeys[i] = new GradientAlphaKey(alphas[i], alphaPositions[i]);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }

    public static void ClearCache()
    {
        foreach (var material in MaterialCache.Values)
        {
            if (material != null)
            {
                Object.Destroy(material);
            }
        }
        MaterialCache.Clear();
        ShaderCache.Clear();
    }

    #region Additional Particle Presets

    public static ParticleConfig CreateBloodSplatter()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Sparks,
            startLifetime = 0.5f,
            startSpeed = 2f,
            startSize = 0.1f,
            startColor = new Color(0.8f, 0.1f, 0.1f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 30f,
            shapeType = ParticleSystemShapeType.Cone,
            shapeAngle = 45f,
            maxParticles = 50
        };
    }

    public static ParticleConfig CreateFireHit()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.CampfireFlame,
            startLifetime = 0.6f,
            startSpeed = 1.5f,
            startSize = 0.3f,
            startColor = new Color(1f, 0.7f, 0.2f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 50f,
            maxParticles = 80
        };
    }

    public static ParticleConfig CreateIceHit()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Snow,
            startLifetime = 0.8f,
            startSpeed = 1f,
            startSize = 0.15f,
            startColor = new Color(0.7f, 0.9f, 1f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 40f,
            maxParticles = 60
        };
    }

    public static ParticleConfig CreateHealEffect()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Dust,
            startLifetime = 1f,
            startSpeed = 0.5f,
            startSize = 0.2f,
            startColor = new Color(0.3f, 1f, 0.5f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 30f,
            useColorOverLifetime = true,
            colorGradient = CreateGradient(
                new[] { Color.green, Color.white },
                new[] { 0f, 1f },
                new[] { 1f, 0f },
                new[] { 0f, 1f }
            ),
            maxParticles = 50
        };
    }

    public static ParticleConfig CreateDeathEffect()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Smoke,
            startLifetime = 1.5f,
            startSpeed = 0.8f,
            startSize = 0.4f,
            startColor = new Color(0.3f, 0.3f, 0.3f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 20f,
            maxParticles = 40
        };
    }

    public static ParticleConfig CreatePickupEffect()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Sparks,
            startLifetime = 0.7f,
            startSpeed = 1.2f,
            startSize = 0.15f,
            startColor = Color.yellow,
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 40f,
            maxParticles = 50
        };
    }

    public static ParticleConfig CreateItemUseEffect()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Dust,
            startLifetime = 0.8f,
            startSpeed = 0.6f,
            startSize = 0.2f,
            startColor = Color.cyan,
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 35f,
            maxParticles = 45
        };
    }

    public static ParticleConfig CreateJumpEffect()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Dust,
            startLifetime = 0.5f,
            startSpeed = 1f,
            startSize = 0.25f,
            startColor = new Color(0.8f, 0.7f, 0.6f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 25f,
            maxParticles = 30
        };
    }

    public static ParticleConfig CreateLandEffect()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Dust,
            startLifetime = 0.4f,
            startSpeed = 0.8f,
            startSize = 0.3f,
            startColor = new Color(0.7f, 0.6f, 0.5f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 30f,
            maxParticles = 35
        };
    }

    public static ParticleConfig CreateArcaneHit()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Sparks,
            startLifetime = 0.7f,
            startSpeed = 1.5f,
            startSize = 0.2f,
            startColor = new Color(0.8f, 0.3f, 1f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 50f,
            useColorOverLifetime = true,
            colorGradient = CreateGradient(
                new[] { Color.purple, Color.blue, Color.white },
                new[] { 0f, 0.5f, 1f },
                new[] { 1f, 0.5f, 0f },
                new[] { 0f, 1f }
            ),
            maxParticles = 70
        };
    }

    public static ParticleConfig CreateHolyHit()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Dust,
            startLifetime = 0.9f,
            startSpeed = 1f,
            startSize = 0.25f,
            startColor = new Color(1f, 0.9f, 0.5f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 45f,
            useColorOverLifetime = true,
            colorGradient = CreateGradient(
                new[] { Color.gold, Color.white },
                new[] { 0f, 1f },
                new[] { 1f, 0f },
                new[] { 0f, 1f }
            ),
            maxParticles = 60
        };
    }

    public static ParticleConfig CreateShadowHit()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Smoke,
            startLifetime = 1f,
            startSpeed = 0.5f,
            startSize = 0.3f,
            startColor = new Color(0.3f, 0.1f, 0.5f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 40f,
            useColorOverLifetime = true,
            colorGradient = CreateGradient(
                new[] { Color.black, Color.purple },
                new[] { 0f, 1f },
                new[] { 0.8f, 0f },
                new[] { 0f, 1f }
            ),
            maxParticles = 50
        };
    }

    public static ParticleConfig CreatePoisonHit()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Smoke,
            startLifetime = 1.2f,
            startSpeed = 0.6f,
            startSize = 0.2f,
            startColor = new Color(0.3f, 0.8f, 0.3f),
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 35f,
            useColorOverLifetime = true,
            colorGradient = CreateGradient(
                new[] { Color.green, Color.yellow },
                new[] { 0f, 1f },
                new[] { 1f, 0f },
                new[] { 0f, 1f }
            ),
            maxParticles = 50
        };
    }

    public static ParticleConfig CreateLevelUpEffect()
    {
        return new ParticleConfig
        {
            preset = ParticlePreset.Sparks,
            startLifetime = 1.5f,
            startSpeed = 2f,
            startSize = 0.4f,
            startColor = Color.gold,
            simulationSpace = ParticleSystemSimulationSpace.World,
            emissionRate = 60f,
            useColorOverLifetime = true,
            colorGradient = CreateGradient(
                new[] { Color.gold, Color.white, Color.yellow },
                new[] { 0f, 0.5f, 1f },
                new[] { 1f, 0.5f, 0f },
                new[] { 0f, 1f }
            ),
            maxParticles = 100
        };
    }

    #endregion
}
}
