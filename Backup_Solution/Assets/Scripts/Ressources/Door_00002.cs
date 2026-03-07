// Door.cs
// Legacy door class - kept for backward compatibility
// Use DoubleDoor for new door implementations
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;

namespace Code.Lavos
{
    /// <summary>
    /// Legacy door class - kept for backward compatibility.
    /// Use DoubleDoor for new door implementations with ItemEngine integration.
    /// </summary>
    public class Door : MonoBehaviour
    {
        [Header("Door Settings")]
        [SerializeField] private float cellSize = 4f;
        [SerializeField] private float wallHeight = 3f;
        [SerializeField] private float doorHeight = 2.4f;
        [SerializeField] private float doorWidth = 2f;

        private Material _doorFrontMat;
        private Material _doorBackMat;
        private Material _doorFrameMat;

        private static Shader _litShader;

        public void Initialize(float cellSize, float wallHeight)
        {
            this.cellSize = cellSize;
            this.wallHeight = wallHeight;
            doorHeight = wallHeight * 0.8f;
            doorWidth = cellSize * 0.5f;

            CreateMaterials();
            BuildDoor();
        }

        private void CreateMaterials()
        {
            InitShaders();

            var frontTex = GenerateDoorTexture(true);
            var backTex = GenerateDoorTexture(false);
            var frameTex = GenerateDoorFrameTexture();

            _doorFrontMat = MakeMaterial(frontTex);
            _doorBackMat = MakeMaterial(backTex);
            _doorFrameMat = MakeMaterial(frameTex);
        }

        private static void InitShaders()
        {
            _litShader ??= Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("URP/Lit") ?? Shader.Find("Standard");
            if (_litShader == null)
                _litShader = Shader.Find("Sprites/Default");
        }

        private Texture2D GenerateDoorTexture(bool front)
        {
            const int w = 32;
            const int h = 48;
            var canvas = new DoorPixelCanvas(w, h);

            var woodDark = new Color32(45, 30, 18, 255);
            var woodMid = new Color32(65, 45, 25, 255);
            var woodLight = new Color32(85, 60, 35, 255);
            var woodHighlight = new Color32(100, 75, 45, 255);
            var iron = new Color32(60, 65, 70, 255);
            var ironDark = new Color32(40, 42, 48, 255);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isPanel = false;
                    int panelIndex = 0;

                    int panelTop = h - 8;
                    int panelMid = h / 2;
                    int panelBot = 8;

                    if (y >= panelTop && x >= 4 && x < w - 4) { isPanel = true; panelIndex = 0; }
                    else if (y >= panelMid - 4 && y <= panelMid + 4 && x >= 4 && x < w - 4) { isPanel = true; panelIndex = 1; }
                    else if (y <= panelBot + 4 && x >= 4 && x < w - 4) { isPanel = true; panelIndex = 2; }

                    if (isPanel)
                    {
                        int shade = (panelIndex + (x / 8) + (front ? 0 : 1)) % 4;
                        canvas.SetPixel(x, y, shade switch
                        {
                            0 => woodDark,
                            1 => woodMid,
                            2 => woodLight,
                            _ => woodHighlight
                        });

                        if (x == 4 || x == w - 5)
                        {
                            int edgeDist = Mathf.Abs(x - 4);
                            if (edgeDist < 2) canvas.SetPixel(x, y, iron);
                        }
                    }
                    else if (x >= 2 && x < w - 2)
                    {
                        int frameShade = (y < 4 || y >= h - 4) ? 0 : ((x < 4 || x >= w - 4) ? 1 : 2);
                        canvas.SetPixel(x, y, frameShade switch
                        {
                            0 => woodDark,
                            1 => ironDark,
                            _ => woodMid
                        });
                    }
                    else
                    {
                        canvas.SetPixel(x, y, iron);
                    }
                }
            }

            if (front)
            {
                int handleX = 6;
                int handleY = h / 2;
                for (int dy = -2; dy <= 2; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx * dx + dy * dy <= 4)
                        {
                            canvas.SetPixel(handleX + dx, handleY + dy, new Color32(180, 170, 140, 255));
                        }
                    }
                }
            }

            return canvas.ToTexture();
        }

        private Texture2D GenerateDoorFrameTexture()
        {
            const int w = 32;
            const int h = 48;
            var canvas = new DoorPixelCanvas(w, h);

            var stoneDark = new Color32(35, 32, 28, 255);
            var stoneMid = new Color32(55, 50, 42, 255);
            var stoneLight = new Color32(70, 65, 55, 255);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isFrame = x < 3 || x >= w - 3 || y < 3 || y >= h - 3;

                    if (isFrame)
                    {
                        int pattern = ((x + y) / 2) % 3;
                        canvas.SetPixel(x, y, pattern switch
                        {
                            0 => stoneDark,
                            1 => stoneMid,
                            _ => stoneLight
                        });
                    }
                    else
                    {
                        canvas.SetPixel(x, y, new Color32(0, 0, 0, 0));
                    }
                }
            }

            return canvas.ToTexture();
        }

        private Material MakeMaterial(Texture2D tex)
        {
            var mat = new Material(_litShader);
            mat.mainTexture = tex;
            mat.SetFloat("_Smoothness", 0.1f);
            return mat;
        }

        private void BuildDoor()
        {
            float doorY = doorHeight / 2f;

            var doorRoot = new GameObject("DoorGeometry").transform;
            doorRoot.SetParent(transform);
            doorRoot.localPosition = Vector3.zero;

            CreateDoorPanel(doorRoot, new Vector3(0, doorY, 0.1f), Quaternion.identity, _doorFrontMat);
            CreateDoorPanel(doorRoot, new Vector3(0, doorY, -0.1f), Quaternion.Euler(0, 180, 0), _doorBackMat);

            float frameThickness = 0.15f;
            float frameDepth = 0.2f;

            CreateFramePiece(doorRoot, new Vector3(0, doorHeight / 2f, 0), new Vector3(doorWidth + frameThickness * 2, frameThickness, frameDepth), _doorFrameMat);
            CreateFramePiece(doorRoot, new Vector3(0, doorHeight, 0), new Vector3(doorWidth + frameThickness * 2, frameThickness, frameDepth), _doorFrameMat);
            CreateFramePiece(doorRoot, new Vector3(-doorWidth / 2f - frameThickness / 2f, doorHeight / 2f, 0), new Vector3(frameThickness, doorHeight, frameDepth), _doorFrameMat);
            CreateFramePiece(doorRoot, new Vector3(doorWidth / 2f + frameThickness / 2f, doorHeight / 2f, 0), new Vector3(frameThickness, doorHeight, frameDepth), _doorFrameMat);
        }

        private void CreateDoorPanel(Transform parent, Vector3 pos, Quaternion rot, Material mat)
        {
            var panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
            panel.transform.SetParent(parent);
            panel.transform.SetPositionAndRotation(pos, rot);
            panel.transform.localScale = new Vector3(doorWidth, doorHeight, 1f);
            panel.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private void CreateFramePiece(Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.transform.SetParent(parent);
            piece.transform.localPosition = pos;
            piece.transform.localScale = scale;
            piece.GetComponent<MeshRenderer>().sharedMaterial = mat;
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
