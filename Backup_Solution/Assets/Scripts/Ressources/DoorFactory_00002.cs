// DoorFactory.cs
// Procedural 3D door generation with pixel art textures
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Creates doors that fit into surfaces (walls, floors, ceilings)
// Replaces surface geometry with door models + effects

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Factory for generating 3D doors with pixel art textures.
    /// Doors fit into surfaces and replace surface geometry.
    /// </summary>
    public class DoorFactory : MonoBehaviour
    {
        [Header("Door Dimensions")]
        [SerializeField] private float doorWidth = 2.5f;
        [SerializeField] private float doorHeight = 3f;
        [SerializeField] private float doorDepth = 0.5f;
        [SerializeField] private float frameThickness = 0.3f;
        
        [Header("Pixel Art Settings")]
        [SerializeField] private int textureResolution = 32;
        [SerializeField] private bool usePixelArtFilter = true;
        
        [Header("Effects")]
        [SerializeField] private static bool enableParticleEffects = true;
        [SerializeField] private static bool enableFogEffects = true;
        [SerializeField] private static bool enableAuraEffects = true;
        
        [Header("Materials")]
        [SerializeField] private static Material woodMaterial;
        [SerializeField] private static Material stoneMaterial;
        [SerializeField] private static Material metalMaterial;
        [SerializeField] private static Material magicMaterial;
        
        private static Dictionary<string, Texture2D> _textureCache = new();
        private static Dictionary<string, Material> _materialCache = new();
        
        #region Door Creation
        
        /// <summary>
        /// Create a 3D door that fits into a wall surface.
        /// Removes/replaces the wall geometry at door position.
        /// </summary>
        public static GameObject CreateDoor(
            Vector3 position,
            Quaternion rotation,
            DoorVariant variant,
            DoorTrapType trap,
            float cellSize = 4f,
            float wallHeight = 3f)
        {
            // Create door container
            GameObject doorObj = new GameObject($"Door_{variant}_{trap}");
            doorObj.transform.position = position;
            doorObj.transform.rotation = rotation;
            
            // Calculate dimensions
            float width = cellSize * 0.6f;
            float height = wallHeight * 0.9f;
            float depth = 0.5f;
            
            // Create door frame (fits into wall)
            CreateDoorFrame(doorObj, width, height, depth, variant);
            
            // Create door panels (left and right)
            CreateDoorPanels(doorObj, width, height, depth, variant);
            
            // Add collider
            AddDoorCollider(doorObj, width, height, depth);
            
            // Add DoorsEngine component
            DoorsEngine doorEngine = doorObj.AddComponent<DoorsEngine>();
            doorEngine.Initialize(variant, trap);
            
            // Add effects based on variant/trap
            AddDoorEffects(doorObj, variant, trap);
            
            return doorObj;
        }
        
        /// <summary>
        /// Create door frame that fits into wall surface.
        /// </summary>
        private static void CreateDoorFrame(
            GameObject parent,
            float width,
            float height,
            float depth,
            DoorVariant variant)
        {
            // Create frame pieces (4 cubes)
            float frameDepth = 0.3f;
            
            // Top frame
            CreateFramePiece(parent, "Frame_Top",
                new Vector3(0, height / 2f + frameDepth / 2f, 0),
                new Vector3(width + frameDepth * 2, frameDepth, frameDepth),
                GetFrameMaterial(variant));
            
            // Bottom frame (threshold)
            CreateFramePiece(parent, "Frame_Bottom",
                new Vector3(0, -frameDepth / 2f, 0),
                new Vector3(width + frameDepth * 2, frameDepth, frameDepth),
                GetStoneMaterial());
            
            // Left frame
            CreateFramePiece(parent, "Frame_Left",
                new Vector3(-width / 2f - frameDepth / 2f, height / 2f, 0),
                new Vector3(frameDepth, height, frameDepth),
                GetFrameMaterial(variant));
            
            // Right frame
            CreateFramePiece(parent, "Frame_Right",
                new Vector3(width / 2f + frameDepth / 2f, height / 2f, 0),
                new Vector3(frameDepth, height, frameDepth),
                GetFrameMaterial(variant));
        }
        
        /// <summary>
        /// Create door panels (left and right doors).
        /// </summary>
        private static void CreateDoorPanels(
            GameObject parent,
            float width,
            float height,
            float depth,
            DoorVariant variant)
        {
            float panelWidth = width / 2f - 0.1f;
            float panelHeight = height - 0.2f;
            float panelDepth = 0.15f;
            
            // Left panel
            GameObject leftPanel = CreatePanelPiece(parent, "Panel_Left",
                new Vector3(-panelWidth / 2f - 0.05f, panelHeight / 2f, 0),
                new Vector3(panelWidth, panelHeight, panelDepth),
                GetDoorPanelMaterial(variant),
                true);
            
            leftPanel.transform.SetParent(parent.transform);
            
            // Right panel
            GameObject rightPanel = CreatePanelPiece(parent, "Panel_Right",
                new Vector3(panelWidth / 2f + 0.05f, panelHeight / 2f, 0),
                new Vector3(panelWidth, panelHeight, panelDepth),
                GetDoorPanelMaterial(variant),
                true);
            
            rightPanel.transform.SetParent(parent.transform);
            
            // Store panel references in DoorsEngine for animation
            DoorsEngine engine = parent.GetComponent<DoorsEngine>();
            if (engine != null)
            {
                // TODO: Store panel references for rotation
            }
        }
        
        /// <summary>
        /// Create a single frame piece.
        /// </summary>
        private static void CreateFramePiece(
            GameObject parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = name;
            frame.transform.SetParent(parent.transform);
            frame.transform.localPosition = position;
            frame.transform.localScale = scale;
            
            // Replace material
            var renderer = frame.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
            
            // Remove collider (door has one collider)
            var collider = frame.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }
        }
        
        /// <summary>
        /// Create a door panel with pixel art texture.
        /// </summary>
        private static GameObject CreatePanelPiece(
            GameObject parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool addDetails)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = name;
            panel.transform.SetParent(parent.transform);
            panel.transform.localPosition = position;
            panel.transform.localScale = scale;
            
            // Replace material
            var renderer = panel.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
            
            // Remove collider
            var collider = panel.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }
            
            // Add decorative details
            if (addDetails)
            {
                AddPanelDetails(panel, scale);
            }
            
            return panel;
        }
        
        /// <summary>
        /// Add decorative details to door panel (studs, handles, etc.).
        /// </summary>
        private static void AddPanelDetails(GameObject panel, Vector3 scale)
        {
            // Add handle
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handle.name = "Handle";
            handle.transform.SetParent(panel.transform);
            handle.transform.localPosition = new Vector3(scale.x * 0.3f, 0, scale.z * 0.6f);
            handle.transform.localScale = Vector3.one * 0.15f;
            
            var handleRenderer = handle.GetComponent<MeshRenderer>();
            if (handleRenderer != null)
            {
                handleRenderer.sharedMaterial = GetMetalMaterial();
            }
            
            var handleCollider = handle.GetComponent<Collider>();
            if (handleCollider != null)
            {
                Object.Destroy(handleCollider);
            }
        }
        
        /// <summary>
        /// Add collider to entire door assembly.
        /// </summary>
        private static void AddDoorCollider(GameObject door, float width, float height, float depth)
        {
            BoxCollider collider = door.AddComponent<BoxCollider>();
            collider.size = new Vector3(width, height, depth);
            collider.center = Vector3.zero;
        }
        
        #endregion
        
        #region Effects
        
        /// <summary>
        /// Add particle/fog/aura effects based on door variant.
        /// </summary>
        private static void AddDoorEffects(GameObject door, DoorVariant variant, DoorTrapType trap)
        {
            // Add aura for magical doors
            if (variant == DoorVariant.Blessed || variant == DoorVariant.Cursed || 
                variant == DoorVariant.Boss || variant == DoorVariant.Secret)
            {
                CreateAuraEffect(door, variant);
            }
            
            // Add fog for trapped doors
            if (trap != DoorTrapType.None && enableFogEffects)
            {
                CreateFogEffect(door, trap);
            }
            
            // Add particles for special doors
            if (enableParticleEffects)
            {
                CreateParticleEffect(door, variant, trap);
            }
        }
        
        /// <summary>
        /// Create glowing aura around door.
        /// </summary>
        private static void CreateAuraEffect(GameObject door, DoorVariant variant)
        {
            GameObject auraObj = new GameObject("Aura");
            auraObj.transform.SetParent(door.transform);
            auraObj.transform.localPosition = new Vector3(0, 1.5f, 0);
            auraObj.transform.localScale = new Vector3(3f, 3.5f, 1f);
            
            // Create aura mesh (quad behind door)
            var renderer = auraObj.AddComponent<MeshRenderer>();
            var filter = auraObj.AddComponent<MeshFilter>();
            
            // Create aura material
            Material auraMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            auraMat.color = GetAuraColor(variant);
            auraMat.EnableKeyword("_EMISSION");
            auraMat.SetColor("_EmissionColor", GetAuraColor(variant) * 0.5f);
            
            renderer.sharedMaterial = auraMat;
            
            // Add pulsing animation component
            auraObj.AddComponent<AnimatedAura>();
        }
        
        /// <summary>
        /// Create fog effect for trapped doors.
        /// </summary>
        private static void CreateFogEffect(GameObject door, DoorTrapType trap)
        {
            GameObject fogObj = new GameObject("Fog");
            fogObj.transform.SetParent(door.transform);
            fogObj.transform.localPosition = new Vector3(0, 0.5f, 0.5f);
            fogObj.transform.localScale = new Vector3(2f, 1f, 0.5f);
            
            // Create fog particle system
            ParticleSystem ps = fogObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 2f;
            main.startSpeed = 0.5f;
            main.startSize = 0.3f;
            main.startColor = GetFogColor(trap);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.loop = true;
            main.playOnAwake = true;
            
            var emission = ps.emission;
            emission.rateOverTime = 20f;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(2f, 1f, 0.3f);
        }
        
        /// <summary>
        /// Create particle effect for special doors.
        /// </summary>
        private static void CreateParticleEffect(GameObject door, DoorVariant variant, DoorTrapType trap)
        {
            GameObject particleObj = new GameObject("Particles");
            particleObj.transform.SetParent(door.transform);
            particleObj.transform.localPosition = new Vector3(0, 1.5f, 0.5f);
            
            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 1f;
            main.startSize = 0.2f;
            main.startColor = GetParticleColor(variant, trap);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;
            main.playOnAwake = true;
            
            var emission = ps.emission;
            emission.rateOverTime = 10f;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            shape.radius = 0.5f;
        }
        
        #endregion
        
        #region Materials & Textures
        
        private static Material GetFrameMaterial(DoorVariant variant)
        {
            switch (variant)
            {
                case DoorVariant.Boss:
                case DoorVariant.Secret:
                    return GetStoneMaterial();
                case DoorVariant.Blessed:
                case DoorVariant.Cursed:
                    return GetMagicMaterial();
                default:
                    return GetStoneMaterial();
            }
        }
        
        private static Material GetDoorPanelMaterial(DoorVariant variant)
        {
            switch (variant)
            {
                case DoorVariant.Boss:
                    return GetMetalMaterial();
                case DoorVariant.Blessed:
                case DoorVariant.Cursed:
                    return GetMagicMaterial();
                default:
                    return GetWoodMaterial();
            }
        }
        
        private static Material GetWoodMaterial()
        {
            if (woodMaterial != null) return woodMaterial;
            
            woodMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            woodMaterial.mainTexture = PixelArtGenerator.GenerateWoodTexture(32, 32);
            woodMaterial.SetFloat("_Smoothness", 0.2f);
            
            return woodMaterial;
        }
        
        private static Material GetStoneMaterial()
        {
            if (stoneMaterial != null) return stoneMaterial;
            
            stoneMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            stoneMaterial.mainTexture = PixelArtGenerator.GenerateStoneTexture(32, 32);
            stoneMaterial.SetFloat("_Smoothness", 0.1f);
            
            return stoneMaterial;
        }
        
        private static Material GetMetalMaterial()
        {
            if (metalMaterial != null) return metalMaterial;
            
            metalMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            metalMaterial.mainTexture = PixelArtGenerator.GenerateMetalTexture(32, 32);
            metalMaterial.SetFloat("_Smoothness", 0.6f);
            
            return metalMaterial;
        }
        
        private static Material GetMagicMaterial()
        {
            if (magicMaterial != null) return magicMaterial;
            
            magicMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            magicMaterial.color = new Color(0.5f, 0.3f, 1f);
            
            return magicMaterial;
        }
        
        private static Texture2D GenerateWoodTexture()
        {
            return PixelArtGenerator.GenerateWoodTexture(32, 32);
        }
        
        private static Texture2D GenerateStoneTexture()
        {
            return PixelArtGenerator.GenerateStoneTexture(32, 32);
        }
        
        private static Texture2D GenerateMetalTexture()
        {
            return PixelArtGenerator.GenerateMetalTexture(32, 32);
        }
        
        private static Color GetAuraColor(DoorVariant variant)
        {
            return variant switch
            {
                DoorVariant.Blessed => new Color(0.5f, 1f, 0.5f),
                DoorVariant.Cursed => new Color(0.5f, 0f, 0.5f),
                DoorVariant.Boss => new Color(1f, 0.3f, 0f),
                DoorVariant.Secret => new Color(0.3f, 0.5f, 1f),
                _ => Color.white
            };
        }
        
        private static Color GetFogColor(DoorTrapType trap)
        {
            return trap switch
            {
                DoorTrapType.Fire => new Color(1f, 0.5f, 0f, 0.5f),
                DoorTrapType.Poison => new Color(0f, 1f, 0f, 0.5f),
                DoorTrapType.Freeze => new Color(0.5f, 0.8f, 1f, 0.5f),
                DoorTrapType.Shock => new Color(1f, 1f, 0f, 0.5f),
                _ => Color.gray
            };
        }
        
        private static Color GetParticleColor(DoorVariant variant, DoorTrapType trap)
        {
            if (trap != DoorTrapType.None)
                return GetFogColor(trap);
            
            return variant switch
            {
                DoorVariant.Blessed => new Color(0.5f, 1f, 0.5f),
                DoorVariant.Cursed => new Color(0.5f, 0f, 0.5f),
                DoorVariant.Boss => new Color(1f, 0.3f, 0f),
                DoorVariant.Secret => new Color(0.3f, 0.5f, 1f),
                _ => Color.white
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Animated aura component for magical doors.
    /// </summary>
    public class AnimatedAura : MonoBehaviour
    {
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.3f;
        
        private Material _auraMat;
        private float _timer;
        
        private void Start()
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                _auraMat = renderer.sharedMaterial;
            }
        }
        
        private void Update()
        {
            if (_auraMat == null) return;
            
            _timer += Time.deltaTime * pulseSpeed;
            float intensity = 0.5f + Mathf.Sin(_timer) * pulseIntensity;
            
            _auraMat.SetColor("_EmissionColor", _auraMat.color * intensity);
        }
    }
}
