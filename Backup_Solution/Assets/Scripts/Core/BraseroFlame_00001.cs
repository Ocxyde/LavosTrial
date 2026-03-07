using UnityEngine;

namespace Code.Lavos
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

    [Header("Colors")]
    [SerializeField] private Color coreColor = new Color(1f, 0.9f, 0.3f);
    [SerializeField] private Color middleColor = new Color(1f, 0.5f, 0.1f);
    [SerializeField] private Color outerColor = new Color(0.8f, 0.2f, 0.05f);

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
            emissionColor = coreColor * 2f
        };

        // Ensure curves exist; fallback to simple linear curves if helper missing
        config.velocityXCurve = ParticleGenerator.CreateSmoothCurve(-turbulence, turbulence, -turbulence);
        config.velocityZCurve = ParticleGenerator.CreateSmoothCurve(-turbulence, turbulence, -turbulence);
        config.sizeCurve = ParticleGenerator.CreateSmoothCurve(0.3f, 1f, 0.1f);

        return config;
    }

    private Gradient CreateFireGradient()
    {
        return ParticleGenerator.CreateGradient(
            new Color[] { coreColor, middleColor, outerColor, outerColor },
            new float[] { 0f, 0.3f, 0.7f, 1f },
            new float[] { 1f, 0.8f, 0.4f, 0f },
            new float[] { 0f, 0.3f, 0.7f, 1f }
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
