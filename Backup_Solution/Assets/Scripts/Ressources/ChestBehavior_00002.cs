// ChestBehavior.cs
// Treasure chest with 8-bit pixel art style and glowing effects
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Inherits from ItemBehavior for ItemEngine integration

using UnityEngine;
using Code.Lavos;

namespace Code.Lavos
{
    /// <summary>
    /// Procedural treasure chest with 8-bit pixel art style.
    /// Features: Glowing aura, randomized contents, open/close animation.
    /// </summary>
    public class ChestBehavior : ItemBehavior
    {
        [Header("Chest Settings")]
        [SerializeField] private float chestWidth = 1.5f;
        [SerializeField] private float chestHeight = 1.2f;
        [SerializeField] private float chestDepth = 1f;

        [Header("Glow Settings")]
        [SerializeField] private Color glowColor = new Color(1f, 0.8f, 0.3f, 0.6f);
        [SerializeField] private float glowIntensity = 1f;
        [SerializeField] private float glowPulseSpeed = 1.5f;

        [Header("Contents")]
        [SerializeField] private LootTable lootTable;
        [SerializeField] private int minGold = 10;
        [SerializeField] private int maxGold = 50;
        [SerializeField] private float itemChance = 0.3f;

        [Header("Visual")]
        [SerializeField] private GameObject lidPivot;
        [SerializeField] private float lidOpenAngle = -110f;

        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;

        private Material _chestMat;
        private Material _glowMat;
        private Light _glowLight;
        private Transform _lidTransform;
        private bool _isOpen = false;
        private float _pulseTimer = 0f;

        private static Shader _unlitShader;
        private static Shader _litShader;

        public bool IsOpen => _isOpen;
        public LootTable LootTable => lootTable;

        private void Awake()
        {
            // Set item type
            SetItemType(ItemType.Chest);
            canInteract = true;
            canCollect = false;
            interactionRange = 3f;

            InitShaders();
        }

        public override void Interact(GameObject interactor)
        {
            if (!CanInteract) return;

            if (_isOpen)
            {
                Close();
            }
            else
            {
                Open();
                GenerateLoot();
            }

            base.Interact(interactor);
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

        public void Initialize(float width, float height, LootTable table = null)
        {
            chestWidth = width;
            chestHeight = height;
            lootTable = table;

            CreateMaterials();
            BuildChest();
            CreateGlowEffect();
        }

        private void CreateMaterials()
        {
            // Chest material (8-bit pixel art texture)
            var chestTex = GenerateChestTexture();
            _chestMat = MakeMaterial(chestTex, true);

            // Glow material
            _glowMat = new Material(_unlitShader ?? Shader.Find("Sprites/Default"));
            _glowMat.color = glowColor;
            _glowMat.EnableKeyword("_EMISSION");
            _glowMat.SetColor("_EmissionColor", glowColor * glowIntensity);
        }

        private Texture2D GenerateChestTexture()
        {
            const int w = 32;
            const int h = 32;
            var canvas = new ChestPixelCanvas(w, h);

            // 8-bit color palette
            var woodDark = new Color32(60, 40, 20, 255);
            var woodMid = new Color32(80, 55, 30, 255);
            var woodLight = new Color32(100, 70, 40, 255);
            var gold = new Color32(255, 215, 0, 255);
            var iron = new Color32(80, 85, 90, 255);

            // Draw wood body
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Wood grain
                    int grainPattern = ((x / 4) + (y / 3)) % 3;
                    Color32 woodColor = grainPattern switch
                    {
                        0 => woodDark,
                        1 => woodMid,
                        _ => woodLight
                    };

                    // Metal bands
                    if (y < 3 || y >= h - 3 || x < 3 || x >= w - 3)
                    {
                        woodColor = iron;
                    }

                    // Gold trim on lid
                    if (y < 8 && (x == 4 || x == w - 5))
                    {
                        woodColor = gold;
                    }

                    // Lock in center
                    if (y >= h/2 - 2 && y <= h/2 + 2 && x >= w/2 - 2 && x <= w/2 + 2)
                    {
                        woodColor = gold;
                    }

                    canvas.SetPixel(x, y, woodColor);
                }
            }

            return canvas.ToTexture();
        }

        private Material MakeMaterial(Texture2D tex, bool useLitShader)
        {
            var shader = useLitShader ? (_litShader ?? Shader.Find("Standard")) : (_unlitShader ?? Shader.Find("Sprites/Default"));
            var mat = new Material(shader);
            mat.mainTexture = tex;
            mat.SetFloat("_Smoothness", 0.3f);
            return mat;
        }

        private void BuildChest()
        {
            var chestRoot = new GameObject("ChestGeometry").transform;
            chestRoot.SetParent(transform);
            chestRoot.localPosition = Vector3.zero;

            // Base
            CreateChestPiece(chestRoot, "Base",
                new Vector3(0, 0.1f, 0),
                new Vector3(chestWidth, 0.2f, chestDepth),
                _chestMat);

            // Lid pivot
            GameObject lidPivotObj = new GameObject("LidPivot");
            lidPivotObj.transform.SetParent(chestRoot);
            lidPivotObj.transform.localPosition = new Vector3(0, chestHeight - 0.1f, -chestDepth / 2f + 0.1f);
            _lidTransform = lidPivotObj.transform;

            // Lid
            CreateChestPiece(_lidTransform, "Lid",
                new Vector3(0, 0.1f, chestDepth / 2f),
                new Vector3(chestWidth, 0.2f, chestDepth),
                _chestMat);

            // Sides
            CreateChestPiece(chestRoot, "LeftSide",
                new Vector3(-chestWidth / 2f, chestHeight / 2f, 0),
                new Vector3(0.1f, chestHeight, chestDepth),
                _chestMat);

            CreateChestPiece(chestRoot, "RightSide",
                new Vector3(chestWidth / 2f, chestHeight / 2f, 0),
                new Vector3(0.1f, chestHeight, chestDepth),
                _chestMat);

            CreateChestPiece(chestRoot, "BackSide",
                new Vector3(0, chestHeight / 2f, -chestDepth / 2f),
                new Vector3(chestWidth, chestHeight, 0.1f),
                _chestMat);
        }

        private void CreateChestPiece(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = name;
            piece.transform.SetParent(parent);
            piece.transform.localPosition = pos;
            piece.transform.localScale = scale;
            piece.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private void CreateGlowEffect()
        {
            // Glow light
            GameObject lightObj = new GameObject("GlowLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = new Vector3(0, chestHeight / 2f, 0);

            _glowLight = lightObj.AddComponent<Light>();
            _glowLight.type = LightType.Point;
            _glowLight.color = glowColor;
            _glowLight.intensity = glowIntensity;
            _glowLight.range = 5f;

            // Glow mesh (visible aura)
            GameObject glowObj = new GameObject("GlowMesh");
            glowObj.transform.SetParent(transform);
            glowObj.transform.localPosition = new Vector3(0, chestHeight / 2f, 0);
            glowObj.transform.localScale = new Vector3(chestWidth * 1.2f, chestHeight * 1.2f, chestDepth * 1.2f);

            var renderer = glowObj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _glowMat;

            var filter = glowObj.AddComponent<MeshFilter>();
            filter.mesh = CreateGlowMesh();
        }

        private Mesh CreateGlowMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "GlowMesh";

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
            if (!_isEnabled) return;

            // Pulse glow effect
            _pulseTimer += Time.deltaTime * glowPulseSpeed;
            if (_glowLight != null)
            {
                float pulse = Mathf.Sin(_pulseTimer) * 0.2f + 0.8f;
                _glowLight.intensity = glowIntensity * pulse;
            }
        }

        /// <summary>
        /// Open the chest.
        /// </summary>
        public void Open()
        {
            if (_isOpen) return;

            if (_lidTransform != null)
            {
                _lidTransform.localRotation = Quaternion.Euler(lidOpenAngle, 0, 0);
            }

            if (openSound != null)
            {
                AudioSource.PlayClipAtPoint(openSound, transform.position);
            }

            _isOpen = true;

            // Increase glow when open
            if (_glowLight != null)
            {
                _glowLight.intensity = glowIntensity * 1.5f;
            }
        }

        /// <summary>
        /// Close the chest.
        /// </summary>
        public void Close()
        {
            if (!_isOpen) return;

            if (_lidTransform != null)
            {
                _lidTransform.localRotation = Quaternion.identity;
            }

            if (closeSound != null)
            {
                AudioSource.PlayClipAtPoint(closeSound, transform.position);
            }

            _isOpen = false;

            // Restore normal glow
            if (_glowLight != null)
            {
                _glowLight.intensity = glowIntensity;
            }
        }

        /// <summary>
        /// Generate random loot for this chest.
        /// </summary>
        private void GenerateLoot()
        {
            int goldAmount = Random.Range(minGold, maxGold + 1);
            Debug.Log($"[ChestBehavior] Generated loot: {goldAmount} gold");

            // Additional item chance
            if (Random.value < itemChance && lootTable != null)
            {
                // Spawn item from loot table
                Debug.Log($"[ChestBehavior] Additional item spawned");
            }
        }

        public override void Reset()
        {
            base.Reset();
            _isOpen = false;
            if (_lidTransform != null)
            {
                _lidTransform.localRotation = Quaternion.identity;
            }
        }

        private void OnDestroy()
        {
            base.OnDestroy();

            if (_chestMat != null) Destroy(_chestMat);
            if (_glowMat != null) Destroy(_glowMat);
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw chest bounds
            Gizmos.color = _isOpen ? Color.yellow : Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * chestHeight / 2f,
                new Vector3(chestWidth, chestHeight, chestDepth));
        }
    }

    /// <summary>
    /// Pixel canvas for chest texture generation.
    /// </summary>
    public class ChestPixelCanvas
    {
        private readonly Color32[] _pixels;
        public int Width { get; }
        public int Height { get; }
        public Color32 DefaultColor { get; set; } = new Color32(0, 0, 0, 255);

        public ChestPixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new Color32[width * height];
            for (int i = 0; i < _pixels.Length; i++)
                _pixels[i] = DefaultColor;
        }

        public void SetPixel(int x, int y, Color32 color)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                _pixels[y * Width + x] = color;
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
