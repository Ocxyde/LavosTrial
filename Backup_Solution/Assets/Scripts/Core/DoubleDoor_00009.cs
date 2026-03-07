// DoubleDoor.cs
// Double-sided door with glowing halo/aura and 8-bit pixel art flame style
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Inherits from BehaviorEngine for ItemEngine integration

using UnityEngine;
using Code.Lavos.Core;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Procedural double-sided door with 8-bit pixel art style.
    /// Features: Glowing halo/aura, flame decorations, randomized appearance.
    /// Inherits from BehaviorEngine for ItemEngine integration.
    /// </summary>
    public class DoubleDoor : BehaviorEngine
    {
        [Header("Door Settings")]
        [SerializeField] private float cellSize = 4f;
        [SerializeField] private float wallHeight = 3f;
        [SerializeField] private float doorWidth = 2.2f;
        [SerializeField] private float doorHeight = 2.6f;

        [Header("Glow/Halo Settings")]
        [SerializeField] private Color haloColor = new Color(1f, 0.6f, 0.2f, 0.5f);
        [SerializeField] private float haloIntensity = 1f;
        [SerializeField] private float haloPulseSpeed = 2f;
        [SerializeField] private float haloRadius = 3f;

        [Header("Flame Settings")]
        [SerializeField] private Color flameCoreColor = new Color(1f, 0.9f, 0.5f);
        [SerializeField] private Color flameOuterColor = new Color(1f, 0.4f, 0.1f);
        [SerializeField] private float flameFlickerSpeed = 8f;
        [SerializeField] private int flamePixelCount = 12;

        [Header("Luminance")]
        [Range(0.5f, 2f)]
        [SerializeField] private float luminanceMultiplier = 1f;

        [Header("Door Type")]
        [SerializeField] private DoorType doorType = DoorType.Start;

        private Material _doorLeftMat;
        private Material _doorRightMat;
        private Material _frameMat;
        private Material _haloMat;
        private Material _flameMat;

        private Light _haloLight;
        private GameObject _haloEffect;
        private GameObject[] _leftFlamePixels;
        private GameObject[] _rightFlamePixels;

        private float _flickerTimer = 0f;
        private static Shader _unlitShader;
        private static Shader _litShader;

        public DoorType DoorType => doorType;
        public bool IsOpen { get; private set; }

        private new void Awake()
        {
            // Set item type
            SetItemType(ItemType.Door);
            interactionRange = 3f;

            InitShaders();
        }

        public override void Interact(GameObject interactor)
        {
            base.Interact(interactor);
            Toggle();
        }

        public void Initialize(float cellSize, float wallHeight, float doorWidth, float doorHeight, DoorType type, float luminance = 1f)
        {
            this.cellSize = cellSize;
            this.wallHeight = wallHeight;
            doorType = type;
            luminanceMultiplier = luminance;

            // Set door dimensions based on maze
            this.doorWidth = doorWidth;
            this.doorHeight = doorHeight;

            // Adjust colors based on door type
            if (doorType == DoorType.Exit)
            {
                haloColor = new Color(0.2f, 0.8f, 1f, 0.6f); // Brighter blue for exit
                haloIntensity = 1.5f;
                luminanceMultiplier = 1.5f;
            }
            else if (doorType == DoorType.Random)
            {
                // Randomize appearance
                RandomizeDoor();
            }

            CreateMaterials();
            BuildDoor();
            CreateHaloEffect();
            CreateFlamePixels();
        }

        private void RandomizeDoor()
        {
            // Random halo color
            int colorChoice = Random.Range(0, 4);
            switch (colorChoice)
            {
                case 0: // Orange flame
                    haloColor = new Color(1f, 0.6f, 0.2f, 0.5f);
                    break;
                case 1: // Purple magic
                    haloColor = new Color(0.8f, 0.3f, 1f, 0.5f);
                    break;
                case 2: // Green poison
                    haloColor = new Color(0.3f, 1f, 0.5f, 0.5f);
                    break;
                case 3: // Red danger
                    haloColor = new Color(1f, 0.2f, 0.2f, 0.5f);
                    break;
            }

            // Random luminance
            luminanceMultiplier = Random.Range(0.8f, 1.3f);

            // Random flame count
            flamePixelCount = Random.Range(8, 16);
        }

        private static void InitShaders()
        {
            _unlitShader ??= Shader.Find("Universal Render Pipeline/Unlit") ?? 
                             Shader.Find("URP/Unlit") ?? 
                             Shader.Find("Unlit/Color");
            _litShader ??= Shader.Find("Universal Render Pipeline/Lit") ?? 
                           Shader.Find("URP/Lit") ?? 
                           Shader.Find("Standard");
        }

        private void CreateMaterials()
        {
            // Door material (8-bit pixel art texture)
            var doorTex = GenerateDoorTexture();
            _doorLeftMat = MakeMaterial(doorTex, true);
            _doorRightMat = MakeMaterial(doorTex, true);

            // Frame material (stone with glow)
            var frameTex = GenerateFrameTexture();
            _frameMat = MakeMaterial(frameTex, false);

            // Halo material (glowing aura)
            _haloMat = new Material(_unlitShader ?? Shader.Find("Sprites/Default"));
            _haloMat.color = haloColor;
            _haloMat.EnableKeyword("_EMISSION");
            _haloMat.SetColor("_EmissionColor", haloColor * haloIntensity * luminanceMultiplier);

            // Flame material
            _flameMat = new Material(_unlitShader ?? Shader.Find("Sprites/Default"));
            _flameMat.color = flameOuterColor;
        }

        private Texture2D GenerateDoorTexture()
        {
            const int w = 32;
            const int h = 48;
            var canvas = new DoorPixelCanvas(w, h);

            // 8-bit color palette
            var woodDark = new Color32(50, 35, 20, 255);
            var woodMid = new Color32(70, 50, 30, 255);
            var woodLight = new Color32(90, 65, 40, 255);
            var iron = new Color32(70, 75, 80, 255);
            var gold = new Color32(200, 170, 50, 255);

            // Draw wood panels (pixel art style)
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Wood grain pattern
                    int grainPattern = ((x / 4) + (y / 3)) % 3;
                    Color32 woodColor = grainPattern switch
                    {
                        0 => woodDark,
                        1 => woodMid,
                        _ => woodLight
                    };

                    // Add metal reinforcement bands
                    if (y < 4 || y >= h - 4 || x < 3 || x >= w - 3)
                    {
                        woodColor = iron;
                    }

                    // Add decorative studs
                    if ((x == 6 || x == w - 7) && (y == 8 || y == h/2 || y == h - 9))
                    {
                        woodColor = gold;
                    }

                    canvas.SetPixel(x, y, woodColor);
                }
            }

            return canvas.ToTexture();
        }

        private Texture2D GenerateFrameTexture()
        {
            const int w = 32;
            const int h = 48;
            var canvas = new DoorPixelCanvas(w, h);

            var stoneDark = new Color32(40, 38, 35, 255);
            var stoneMid = new Color32(60, 55, 50, 255);
            var stoneLight = new Color32(80, 75, 68, 255);

            // Draw stone frame with glow hints
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isFrame = x < 4 || x >= w - 4 || y < 4 || y >= h - 4;

                    if (isFrame)
                    {
                        // Brick/stone pattern
                        int brickX = (x / 8) % 2;
                        int brickY = (y / 6) % 2;
                        int pattern = (brickX + brickY) % 3;

                        Color32 stoneColor = pattern switch
                        {
                            0 => stoneDark,
                            1 => stoneMid,
                            _ => stoneLight
                        };

                        // Add glow hints on inner edge
                        if (x == 3 || x == w - 4)
                        {
                            stoneColor.r = (byte)Mathf.Min(255, stoneColor.r + 40);
                            stoneColor.g = (byte)Mathf.Min(255, stoneColor.g + 30);
                            stoneColor.b = (byte)Mathf.Min(255, stoneColor.b + 20);
                        }

                        canvas.SetPixel(x, y, stoneColor);
                    }
                }
            }

            return canvas.ToTexture();
        }

        private Material MakeMaterial(Texture2D tex, bool useLitShader)
        {
            var shader = useLitShader ? (_litShader ?? Shader.Find("Standard")) : (_unlitShader ?? Shader.Find("Sprites/Default"));
            var mat = new Material(shader);
            mat.mainTexture = tex;
            mat.SetFloat("_Smoothness", 0.2f);
            return mat;
        }

        private void BuildDoor()
        {
            float doorY = doorHeight / 2f;

            var doorRoot = new GameObject("DoorGeometry").transform;
            doorRoot.SetParent(transform);
            doorRoot.localPosition = Vector3.zero;

            // Left door panel
            CreateDoorPanel(doorRoot, "LeftPanel", 
                new Vector3(-doorWidth / 4f, doorY, 0.1f), 
                Quaternion.identity, 
                _doorLeftMat,
                new Vector2(doorWidth / 2f, doorHeight));

            // Right door panel
            CreateDoorPanel(doorRoot, "RightPanel", 
                new Vector3(doorWidth / 4f, doorY, 0.1f), 
                Quaternion.identity, 
                _doorRightMat,
                new Vector2(doorWidth / 2f, doorHeight));

            // Back panels (for double-sided)
            CreateDoorPanel(doorRoot, "LeftPanel_Back", 
                new Vector3(-doorWidth / 4f, doorY, -0.1f), 
                Quaternion.Euler(0, 180, 0), 
                _doorLeftMat,
                new Vector2(doorWidth / 2f, doorHeight));

            CreateDoorPanel(doorRoot, "RightPanel_Back", 
                new Vector3(doorWidth / 4f, doorY, -0.1f), 
                Quaternion.Euler(0, 180, 0), 
                _doorRightMat,
                new Vector2(doorWidth / 2f, doorHeight));

            // Frame pieces
            float frameThickness = 0.2f;
            float frameDepth = 0.3f;

            CreateFramePiece(doorRoot, "Frame_Top", 
                new Vector3(0, doorHeight, 0), 
                new Vector3(doorWidth + frameThickness * 2, frameThickness, frameDepth), 
                _frameMat);

            CreateFramePiece(doorRoot, "Frame_Bottom", 
                new Vector3(0, 0, 0), 
                new Vector3(doorWidth + frameThickness * 2, frameThickness, frameDepth), 
                _frameMat);

            CreateFramePiece(doorRoot, "Frame_Left", 
                new Vector3(-doorWidth / 2f - frameThickness / 2f, doorHeight / 2f, 0), 
                new Vector3(frameThickness, doorHeight, frameDepth), 
                _frameMat);

            CreateFramePiece(doorRoot, "Frame_Right", 
                new Vector3(doorWidth / 2f + frameThickness / 2f, doorHeight / 2f, 0), 
                new Vector3(frameThickness, doorHeight, frameDepth), 
                _frameMat);
        }

        private void CreateDoorPanel(Transform parent, string name, Vector3 pos, Quaternion rot, Material mat, Vector2 size)
        {
            var panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
            panel.name = name;
            panel.transform.SetParent(parent);
            panel.transform.SetPositionAndRotation(pos, rot);
            panel.transform.localScale = new Vector3(size.x, size.y, 1f);
            panel.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private void CreateFramePiece(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = name;
            piece.transform.SetParent(parent);
            piece.transform.localPosition = pos;
            piece.transform.localScale = scale;
            piece.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private void CreateHaloEffect()
        {
            // Create glowing halo behind door
            _haloEffect = new GameObject("HaloEffect");
            _haloEffect.transform.SetParent(transform);
            _haloEffect.transform.localPosition = new Vector3(0, doorHeight / 2f, -0.5f);
            _haloEffect.transform.localScale = new Vector3(doorWidth * 0.8f, doorHeight * 0.9f, 1f);

            var renderer = _haloEffect.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _haloMat;

            var filter = _haloEffect.AddComponent<MeshFilter>();
            filter.mesh = CreateHaloMesh();

            // Add point light for actual illumination
            GameObject lightObj = new GameObject("HaloLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = new Vector3(0, doorHeight * 0.7f, -1f);

            _haloLight = lightObj.AddComponent<Light>();
            _haloLight.type = LightType.Point;
            _haloLight.color = haloColor;
            _haloLight.intensity = haloIntensity * luminanceMultiplier;
            _haloLight.range = haloRadius;
        }

        private Mesh CreateHaloMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "HaloMesh";

            // Simple quad with rounded corners effect
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0)
            };

            int[] triangles = new int[] { 0, 2, 1, 0, 3, 2 };

            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            return mesh;
        }

        private void CreateFlamePixels()
        {
            _leftFlamePixels = new GameObject[flamePixelCount];
            _rightFlamePixels = new GameObject[flamePixelCount];

            // Create flame pixels on left side
            for (int i = 0; i < flamePixelCount; i++)
            {
                _leftFlamePixels[i] = CreateFlamePixel($"LeftFlame_{i}", -doorWidth / 2f - 0.3f, i);
                _rightFlamePixels[i] = CreateFlamePixel($"RightFlame_{i}", doorWidth / 2f + 0.3f, i);
            }
        }

        private GameObject CreateFlamePixel(string name, float xOffset, int index)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);

            // Random position along door height
            float yPos = Random.Range(0.3f, doorHeight - 0.3f);
            float zOffset = Random.Range(-0.2f, 0.2f);
            go.transform.localPosition = new Vector3(xOffset, yPos, zOffset);

            // Small pixel quad
            float pixelSize = 0.08f;
            go.transform.localScale = new Vector3(pixelSize, pixelSize * Random.Range(1f, 2f), 1f);

            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _flameMat;

            var filter = go.AddComponent<MeshFilter>();
            filter.mesh = CreatePixelMesh();

            return go;
        }

        private Mesh CreatePixelMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "PixelMesh";

            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0)
            };

            int[] triangles = new int[] { 0, 2, 1, 0, 3, 2 };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private void Update()
        {
            // Pulse halo effect
            if (_haloLight != null)
            {
                float pulse = Mathf.Sin(_flickerTimer * haloPulseSpeed) * 0.2f + 0.8f;
                _haloLight.intensity = haloIntensity * luminanceMultiplier * pulse;
            }

            // Flicker flame pixels
            _flickerTimer += Time.deltaTime * flameFlickerSpeed;
            UpdateFlamePixels();
        }

        private void UpdateFlamePixels()
        {
            Color flickerCore = Color.Lerp(flameCoreColor, flameOuterColor, Mathf.Abs(Mathf.Sin(_flickerTimer)));
            Color flickerOuter = Color.Lerp(flameOuterColor, Color.red, Mathf.Abs(Mathf.Cos(_flickerTimer * 0.7f)));

            for (int i = 0; i < flamePixelCount; i++)
            {
                if (_leftFlamePixels[i] != null && _leftFlamePixels[i].TryGetComponent<MeshRenderer>(out var renderer))
                {
                    float individualFlicker = Mathf.Sin(_flickerTimer + i * 0.5f) * 0.3f + 0.7f;
                    renderer.sharedMaterial.color = Color.Lerp(flickerOuter, flickerCore, individualFlicker);
                }

                if (_rightFlamePixels[i] != null && _rightFlamePixels[i].TryGetComponent<MeshRenderer>(out renderer))
                {
                    float individualFlicker = Mathf.Cos(_flickerTimer + i * 0.5f) * 0.3f + 0.7f;
                    renderer.sharedMaterial.color = Color.Lerp(flickerOuter, flickerCore, individualFlicker);
                }
            }
        }

        /// <summary>
        /// Open the double door.
        /// </summary>
        public void Open()
        {
            if (IsOpen) return;

            var doorRoot = transform.Find("DoorGeometry");
            if (doorRoot != null)
            {
                var leftPanel = doorRoot.Find("LeftPanel");
                var rightPanel = doorRoot.Find("RightPanel");

                if (leftPanel != null)
                {
                    leftPanel.localRotation = Quaternion.Euler(0, -90, 0);
                }
                if (rightPanel != null)
                {
                    rightPanel.localRotation = Quaternion.Euler(0, 90, 0);
                }
            }

            IsOpen = true;
        }

        /// <summary>
        /// Close the double door.
        /// </summary>
        public void Close()
        {
            if (!IsOpen) return;

            var doorRoot = transform.Find("DoorGeometry");
            if (doorRoot != null)
            {
                var leftPanel = doorRoot.Find("LeftPanel");
                var rightPanel = doorRoot.Find("RightPanel");

                if (leftPanel != null)
                {
                    leftPanel.localRotation = Quaternion.identity;
                }
                if (rightPanel != null)
                {
                    rightPanel.localRotation = Quaternion.identity;
                }
            }

            IsOpen = false;
        }

        /// <summary>
        /// Toggle door open/close state.
        /// </summary>
        public void Toggle()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        private new void OnDestroy()
        {
            base.OnDestroy();

            if (_doorLeftMat != null) Destroy(_doorLeftMat);
            if (_doorRightMat != null) Destroy(_doorRightMat);
            if (_frameMat != null) Destroy(_frameMat);
            if (_haloMat != null) Destroy(_haloMat);
            if (_flameMat != null) Destroy(_flameMat);
            
            // Fix memory leak: Destroy dynamically created GameObjects
            if (_haloEffect != null) Destroy(_haloEffect);
            if (_haloLight != null) Destroy(_haloLight.gameObject);
        }

        private void OnDrawGizmos()
        {
            // Draw halo radius
            Gizmos.color = new Color(haloColor.r, haloColor.g, haloColor.b, 0.3f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * doorHeight * 0.7f, haloRadius);

            // Draw door bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * doorHeight / 2f,
                new Vector3(doorWidth, doorHeight, 0.5f));
        }
    }

    /// <summary>
    /// Pixel canvas helper for procedural door texture generation.
    /// </summary>
    public class DoorPixelCanvas
    {
        private readonly Color32[] _pixels;
        public int Width { get; }
        public int Height { get; }
        public Color32 DefaultColor { get; set; } = new Color32(0, 0, 0, 255);

        public DoorPixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new Color32[width * height];
            for (int i = 0; i < _pixels.Length; i++)
            {
                _pixels[i] = DefaultColor;
            }
        }

        public void SetPixel(int x, int y, Color32 color)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                _pixels[y * Width + x] = color;
            }
        }

        public void FillRect(int x, int y, int w, int h, Color32 color)
        {
            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    SetPixel(x + dx, y + dy, color);
                }
            }
        }

        public Texture2D ToTexture()
        {
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.SetPixels32(_pixels);
            tex.Apply();
            return tex;
        }
    }
}
