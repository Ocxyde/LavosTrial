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
// ChestBehavior.cs
// Treasure chest with 8-bit pixel art style and glowing effects
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Inherits from BehaviorEngine for ItemEngine integration
// Uses PixelArtTextureFactory for procedural textures
// Integrates with EventHandler for plug-in-out architecture

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Procedural treasure chest with 8-bit pixel art style.
    /// Features: Glowing aura, randomized contents, open/close animation.
    /// Inherits from BehaviorEngine for ItemEngine integration.
    /// Uses PixelArtTextureFactory for procedural textures.
    /// Integrates with EventHandler for plug-in-out architecture.
    /// Implements IInteractable for E-key interaction.
    /// </summary>
    public class ChestBehavior : BehaviorEngine, IInteractable
    {
        [Header("Chest Settings")]
        [SerializeField] private float chestWidth = 1.5f;
        [SerializeField] private float chestHeight = 1.2f;
        [SerializeField] private float chestDepth = 1f;

        [Header("Glow Settings")]
        [SerializeField] private Color glowColor = new Color(1f, 0.85f, 0.4f, 0.4f);
        [SerializeField] private float glowIntensity = 0.6f;
        [SerializeField] private float glowPulseSpeed = 0.8f;

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

        [Header("Events")]
        [SerializeField] private bool raiseEvents = true;

        private Material _chestMat;
        private Material _glowMat;
        private Light _glowLight;
        private Transform _lidTransform;
        private bool _isOpen = false;
        private float _pulseTimer = 0f;
        private int _goldAmount = 0;

        private static Shader _unlitShader;
        private static Shader _litShader;

        public bool IsOpen => _isOpen;
        public LootTable LootTable => lootTable;
        public int GoldAmount => _goldAmount;

        private new void Awake()
        {
            // Set item type
            SetItemType(ItemType.Chest);
            CanInteractValue = true;
            CanCollectValue = false;
            InteractionRangeValue = 3f;

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
                GenerateLoot(interactor);
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

            // 8-bit pixel art color palette (vibrant, classic style)
            var woodDark = new Color32(52, 28, 12, 255);      // Dark brown
            var woodMid = new Color32(88, 50, 20, 255);       // Medium brown
            var woodLight = new Color32(120, 75, 35, 255);    // Light brown
            var goldBright = new Color32(255, 220, 60, 255);  // Bright gold
            var goldDark = new Color32(200, 160, 40, 255);    // Dark gold
            var iron = new Color32(70, 75, 85, 255);          // Iron/steel
            var ironHighlight = new Color32(120, 130, 145, 255); // Iron highlight
            var gemRed = new Color32(220, 40, 40, 255);       // Ruby gem
            var gemGlow = new Color32(255, 100, 100, 255);    // Gem glow

            // Draw pixel art chest (front view)
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color32 pixel = woodMid;

                    // === LID SECTION (top 40%) ===
                    bool isLid = y < h * 0.4f;
                    
                    if (isLid)
                    {
                        // Lid wood with curved top
                        int grainPattern = ((x / 3) + (y / 2)) % 3;
                        pixel = grainPattern switch
                        {
                            0 => woodDark,
                            1 => woodMid,
                            _ => woodLight
                        };

                        // Lid gold trim (border)
                        if (y < 4 || x < 3 || x >= w - 3)
                        {
                            pixel = ((x + y) % 2 == 0) ? goldBright : goldDark;
                        }

                        // Decorative gold pattern on lid
                        if (y >= 4 && y < 8 && x >= 8 && x < w - 8)
                        {
                            if ((x + y) % 4 == 0)
                                pixel = goldBright;
                        }

                        // Center gem on lid
                        if (y >= 5 && y <= 9 && x >= w/2 - 3 && x <= w/2 + 3)
                        {
                            int gemDist = Mathf.Abs(x - w/2) + Mathf.Abs(y - 7);
                            if (gemDist <= 2)
                                pixel = gemRed;
                            else if (gemDist == 3)
                                pixel = gemGlow;
                        }
                    }
                    // === BODY SECTION (bottom 60%) ===
                    else
                    {
                        // Body wood grain (horizontal planks)
                        int plankRow = (y - h/2) / 5;
                        int grainPattern = ((x / 4) + plankRow) % 3;
                        pixel = grainPattern switch
                        {
                            0 => woodDark,
                            1 => woodMid,
                            _ => woodLight
                        };

                        // Metal bands (horizontal reinforcement)
                        int bodyY = y - h/2;
                        if (bodyY < 3 || bodyY >= (h/2) - 3)
                        {
                            // Band with rivets
                            pixel = ((x % 8) < 2) ? ironHighlight : iron;
                        }

                        // Vertical metal straps
                        if ((x < 4 || x >= w - 4 || x >= w/2 - 2 && x <= w/2 + 1))
                        {
                            if (y >= h * 0.4f + 2 && y < h - 3)
                            {
                                pixel = ((y % 6) < 2) ? ironHighlight : iron;
                            }
                        }

                        // Ornate lock plate (center)
                        int lockCenterY = h/2 + 4;
                        int lockCenterX = w/2;
                        int lockDist = Mathf.Max(Mathf.Abs(x - lockCenterX), Mathf.Abs(y - lockCenterY));
                        
                        if (lockDist <= 4 && y >= h * 0.45f)
                        {
                            // Lock plate background
                            pixel = goldDark;
                            
                            // Lock keyhole
                            int keyholeDist = Mathf.Max(Mathf.Abs(x - lockCenterX), Mathf.Abs(y - lockCenterY));
                            if (keyholeDist <= 2)
                            {
                                pixel = iron;
                            }
                            // Keyhole opening (cross shape)
                            if ((x == lockCenterX && Mathf.Abs(y - lockCenterY) <= 1) ||
                                (y == lockCenterY && Mathf.Abs(x - lockCenterX) <= 1))
                            {
                                pixel = new Color32(30, 30, 35, 255); // Dark keyhole
                            }
                            
                            // Gold studs around lock
                            if (lockDist == 4 && (x + y) % 2 == 0)
                            {
                                pixel = goldBright;
                            }
                        }
                    }

                    // Edge highlighting (3D effect)
                    if (!isLid && x == 3)
                        pixel = LightenColor(pixel, 30);
                    if (!isLid && x == w - 4)
                        pixel = DarkenColor(pixel, 30);

                    canvas.SetPixel(x, y, pixel);
                }
            }

            return canvas.ToTexture();
        }

        private static Color32 LightenColor(Color32 c, int amount)
        {
            return new Color32(
                (byte)Mathf.Min(255, c.r + amount),
                (byte)Mathf.Min(255, c.g + amount),
                (byte)Mathf.Min(255, c.b + amount),
                c.a
            );
        }

        private static Color32 DarkenColor(Color32 c, int amount)
        {
            return new Color32(
                (byte)Mathf.Max(0, c.r - amount),
                (byte)Mathf.Max(0, c.g - amount),
                (byte)Mathf.Max(0, c.b - amount),
                c.a
            );
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
            
            var pieceRenderer = piece.GetComponent<MeshRenderer>();
            if (pieceRenderer != null)
                pieceRenderer.sharedMaterial = mat;
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
            _glowLight.range = 3f;

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
        /// Open the chest and raise events.
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

            // Increase glow when open (subtle)
            if (_glowLight != null)
            {
                _glowLight.intensity = glowIntensity * 1.2f;
            }

            // Raise event through EventHandler (plug-in-out architecture)
            if (raiseEvents && EventHandler.Instance != null)
            {
                EventHandler.Instance.InvokeChestOpened(transform.position, _goldAmount);
            }
        }

        /// <summary>
        /// Close the chest and raise events.
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

            // Raise event through EventHandler
            if (raiseEvents && EventHandler.Instance != null)
            {
                EventHandler.Instance.InvokeChestClosed(transform.position);
            }
        }

        /// <summary>
        /// Generate random loot for this chest and notify through EventHandler.
        /// </summary>
        private void GenerateLoot(GameObject looter)
        {
            _goldAmount = Random.Range(minGold, maxGold + 1);
            Debug.Log($"[ChestBehavior] Generated loot: {_goldAmount} gold");

            // Raise loot event through EventHandler
            if (raiseEvents && EventHandler.Instance != null)
            {
                EventHandler.Instance.InvokeChestLootGenerated(transform.position, _goldAmount, looter);
            }

            // Additional item chance
            if (Random.value < itemChance && lootTable != null)
            {
                // Spawn item from loot table
                Debug.Log($"[ChestBehavior] Additional item spawned");

                if (EventHandler.Instance != null)
                {
                    EventHandler.Instance.InvokeChestItemSpawned(transform.position, lootTable);
                }
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

        private new void OnDestroy()
        {
            base.OnDestroy();

            if (_chestMat != null) Destroy(_chestMat);
            if (_glowMat != null) Destroy(_glowMat);

            // Fix memory leak: Destroy dynamically created GameObjects
            if (_glowLight != null) Destroy(_glowLight.gameObject);
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw chest bounds
            Gizmos.color = _isOpen ? Color.yellow : Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * chestHeight / 2f,
                new Vector3(chestWidth, chestHeight, chestDepth));
        }

        //  IInteractable Implementation 
        /// <summary>
        /// Interaction prompt for chest (E-key).
        /// </summary>
        public string InteractionPrompt => "Open Chest";

        /// <summary>
        /// Check if player can interact with this chest.
        /// </summary>
        bool IInteractable.CanInteract(MonoBehaviour player) => !_isOpen;

        /// <summary>
        /// Open chest when player presses E.
        /// </summary>
        void IInteractable.OnInteract(MonoBehaviour player)
        {
            if (!((IInteractable)this).CanInteract(player)) return;

            Open();
            GenerateLoot(player.gameObject);
            Debug.Log($"[ChestBehavior] Player opened chest at {transform.position}");
        }

        /// <summary>
        /// Highlight chest when player looks at it (subtle).
        /// </summary>
        void IInteractable.OnHighlightEnter(MonoBehaviour player)
        {
            // Optional: Add highlight effect
            if (_glowLight != null)
            {
                _glowLight.intensity = glowIntensity * 1.2f;
            }
        }

        /// <summary>
        /// Remove highlight when player looks away.
        /// </summary>
        void IInteractable.OnHighlightExit(MonoBehaviour player)
        {
            // Restore normal glow
            if (_glowLight != null)
            {
                _glowLight.intensity = glowIntensity;
            }
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
