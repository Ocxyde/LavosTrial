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
// RealisticDoorFactory.cs
// ⚠️ DEPRECATED - Procedural door generation violates plug-in-out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// STATUS: DEPRECATED - Violates plug-in-out architecture (creates GameObjects at runtime)
// REPLACEMENT: Use door prefabs from Assets/Prefabs/DoorPrefab.prefab
// REASON: Factory creates GameObjects and components at runtime instead of using prefabs
//
// DO NOT USE IN NEW CODE - Use door prefabs instead.
// This file is kept for reference only.

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// ⚠️ DEPRECATED - RealisticDoorFactory violates plug-in-out architecture.
    /// 
    /// This factory creates door GameObjects and components at runtime:
    /// - new GameObject("Door") - VIOLATION
    /// - AddComponent<DoorsEngine>() - VIOLATION
    /// - AddComponent<DoorAnimation>() - VIOLATION
    /// 
    /// ✅ CORRECT APPROACH:
    /// Use door prefabs instead (Assets/Prefabs/DoorPrefab.prefab).
    /// Instantiate prefabs at runtime, don't create from scratch.
    /// 
    /// This file is kept for reference only - DO NOT USE IN NEW CODE.
    /// </summary>
    [System.Obsolete("RealisticDoorFactory violates plug-in-out. Use door prefabs instead (Assets/Prefabs/DoorPrefab.prefab).")]
    public static class RealisticDoorFactory
    {
        #region Door Dimensions (Real-world proportions)

        // Standard door proportions
        private const float FRAME_THICKNESS = 0.15f;      // Door frame thickness
        private const float PANEL_THICKNESS = 0.08f;      // Door panel thickness
        private const float HANDLE_RADIUS = 0.04f;        // Door handle size
        private const float HINGE_SIZE = 0.06f;           // Hinge size
        private const float GAP_TOP = 0.05f;              // Gap at top
        private const float GAP_SIDE = 0.03f;             // Gap at sides
        private const float GAP_BOTTOM = 0.10f;           // Gap at bottom (for carpet)

        #endregion

        #region Create Door Prefab

        /// <summary>
        /// Create a complete realistic door with 8-bit pixel art textures.
        /// Returns GameObject ready to use as prefab.
        /// </summary>
        public static GameObject CreateRealisticDoor(
            Vector3 position,
            Quaternion rotation,
            DoorVariant variant,
            DoorTrapType trap,
            float width = 2.5f,
            float height = 3.0f,
            float depth = 0.5f)
        {
            // Create door root
            GameObject doorObj = new GameObject($"Door_{variant}_{trap}");
            doorObj.transform.position = position;
            doorObj.transform.rotation = rotation;
            // Note: Don't set tag to "Door" - tag must be defined in Unity Tag Manager first
            // doorObj.tag = "Door";  // Removed - use "Untagged" instead

            // Create door frame (attached to wall) with pixel art textures
            CreateDoorFrame(doorObj, width, height, depth, variant);

            // Create door leaf (the actual moving door) with pixel art textures
            GameObject doorLeaf = CreateDoorLeaf(doorObj, width, height, depth, variant);

            // Create door handle with pixel art texture
            CreateDoorHandle(doorLeaf, variant);

            // Create hinges with pixel art texture
            CreateHinges(doorLeaf, depth, variant);

            // Add collider
            AddDoorCollider(doorObj, width, height, depth);

            // Add DoorsEngine component with SFX
            DoorsEngine doorEngine = doorObj.AddComponent<DoorsEngine>();
            doorEngine.Initialize(variant, trap);
            doorEngine.SetDoorSounds(
                DoorSFXManager.GetOpenSound(variant),
                DoorSFXManager.GetCloseSound(variant),
                DoorSFXManager.GetLockedSound()
            );

            // Add DoorAnimation component
            DoorAnimation doorAnim = doorObj.AddComponent<DoorAnimation>();
            if (doorLeaf != null)
            {
                Transform leftPanel = doorLeaf.transform.Find("LeftPanel");
                Transform rightPanel = doorLeaf.transform.Find("RightPanel");
                if (leftPanel != null && rightPanel != null)
                {
                    doorAnim.SetPanelReferences(leftPanel, rightPanel);
                }
            }

            return doorObj;
        }

        #endregion

        #region Door Frame (Fixed to wall)

        /// <summary>
        /// Create door frame that attaches to wall.
        /// Frame has: Top piece, two side jambs, threshold.
        /// </summary>
        private static void CreateDoorFrame(
            GameObject parent,
            float width,
            float height,
            float depth,
            DoorVariant variant)
        {
            GameObject frameObj = new GameObject("Frame");
            frameObj.transform.SetParent(parent.transform);
            frameObj.transform.localPosition = Vector3.zero;
            frameObj.transform.localRotation = Quaternion.identity;

            Material frameMat = GetFrameMaterial(variant);

            // Top header (horizontal piece on top)
            CreateFramePiece(frameObj, "TopHeader",
                new Vector3(0, height / 2f + FRAME_THICKNESS / 2f, 0),
                new Vector3(width + FRAME_THICKNESS * 2f, FRAME_THICKNESS, FRAME_THICKNESS),
                frameMat);

            // Left jamb (vertical side piece)
            CreateFramePiece(frameObj, "LeftJamb",
                new Vector3(-width / 2f - FRAME_THICKNESS / 2f, height / 2f, 0),
                new Vector3(FRAME_THICKNESS, height, FRAME_THICKNESS),
                frameMat);

            // Right jamb (vertical side piece)
            CreateFramePiece(frameObj, "RightJamb",
                new Vector3(width / 2f + FRAME_THICKNESS / 2f, height / 2f, 0),
                new Vector3(FRAME_THICKNESS, height, FRAME_THICKNESS),
                frameMat);

            // Threshold (bottom piece - stone/metal)
            Material thresholdMat = GetThresholdMaterial();
            CreateFramePiece(frameObj, "Threshold",
                new Vector3(0, -FRAME_THICKNESS / 2f, 0),
                new Vector3(width + FRAME_THICKNESS * 2f, FRAME_THICKNESS, depth),
                thresholdMat);
        }

        #endregion

        #region Door Leaf (Moving part)

        /// <summary>
        /// Create the actual door leaf (moving part).
        /// Has: Left panel, right panel, stiles, rails.
        /// </summary>
        private static GameObject CreateDoorLeaf(
            GameObject parent,
            float width,
            float height,
            float depth,
            DoorVariant variant)
        {
            GameObject leafObj = new GameObject("DoorLeaf");
            leafObj.transform.SetParent(parent.transform);
            
            // Position at center of opening (inside frame)
            float clearWidth = width - FRAME_THICKNESS * 2f;
            float clearHeight = height - FRAME_THICKNESS * 1.5f;
            
            leafObj.transform.localPosition = new Vector3(0, clearHeight / 2f - GAP_BOTTOM, 0);
            leafObj.transform.localRotation = Quaternion.identity;

            Material panelMat = GetPanelMaterial(variant);
            Material stileMat = GetStileMaterial(variant);

            // Calculate panel dimensions
            float panelWidth = (clearWidth - STILE_WIDTH * 2f) / 2f - GAP_SIDE;
            float panelHeight = clearHeight - RAIL_HEIGHT * 2f - GAP_TOP - GAP_BOTTOM;

            // Create left stile (vertical frame piece)
            CreateFramePiece(leafObj, "LeftStile",
                new Vector3(-clearWidth / 2f + STILE_WIDTH / 2f, clearHeight / 2f, 0),
                new Vector3(STILE_WIDTH, clearHeight, PANEL_THICKNESS),
                stileMat);

            // Create right stile
            CreateFramePiece(leafObj, "RightStile",
                new Vector3(clearWidth / 2f - STILE_WIDTH / 2f, clearHeight / 2f, 0),
                new Vector3(STILE_WIDTH, clearHeight, PANEL_THICKNESS),
                stileMat);

            // Create top rail (horizontal piece)
            CreateFramePiece(leafObj, "TopRail",
                new Vector3(0, clearHeight - RAIL_HEIGHT / 2f, 0),
                new Vector3(clearWidth - STILE_WIDTH * 2f, RAIL_HEIGHT, PANEL_THICKNESS),
                stileMat);

            // Create bottom rail
            CreateFramePiece(leafObj, "BottomRail",
                new Vector3(0, RAIL_HEIGHT / 2f, 0),
                new Vector3(clearWidth - STILE_WIDTH * 2f, RAIL_HEIGHT, PANEL_THICKNESS),
                stileMat);

            // Create middle rail (optional, for larger doors)
            if (clearHeight > 2.0f)
            {
                CreateFramePiece(leafObj, "MiddleRail",
                    new Vector3(0, clearHeight / 2f, 0),
                    new Vector3(clearWidth - STILE_WIDTH * 2f, RAIL_HEIGHT, PANEL_THICKNESS),
                    stileMat);
            }

            // Create left panel (recessed)
            GameObject leftPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPanel.name = "LeftPanel";
            leftPanel.transform.SetParent(leafObj.transform);
            leftPanel.transform.localPosition = new Vector3(
                -clearWidth / 4f + STILE_WIDTH / 2f,
                clearHeight / 2f - RAIL_HEIGHT,
                -0.02f  // Slightly recessed
            );
            leftPanel.transform.localScale = new Vector3(panelWidth, panelHeight, PANEL_THICKNESS * 0.8f);
            
            var leftRenderer = leftPanel.GetComponent<MeshRenderer>();
            if (leftRenderer != null)
                leftRenderer.sharedMaterial = panelMat;

            // Create right panel
            GameObject rightPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPanel.name = "RightPanel";
            rightPanel.transform.SetParent(leafObj.transform);
            rightPanel.transform.localPosition = new Vector3(
                clearWidth / 4f - STILE_WIDTH / 2f,
                clearHeight / 2f - RAIL_HEIGHT,
                -0.02f  // Slightly recessed
            );
            rightPanel.transform.localScale = new Vector3(panelWidth, panelHeight, PANEL_THICKNESS * 0.8f);
            
            var rightRenderer = rightPanel.GetComponent<MeshRenderer>();
            if (rightRenderer != null)
                rightRenderer.sharedMaterial = panelMat;

            return leafObj;
        }

        // Stile and rail dimensions
        private const float STILE_WIDTH = 0.15f;
        private const float RAIL_HEIGHT = 0.12f;

        #endregion

        #region Door Handle

        /// <summary>
        /// Create door handle/knob.
        /// </summary>
        private static void CreateDoorHandle(GameObject doorLeaf, DoorVariant variant)
        {
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(doorLeaf.transform);

            // Position on right stile (standard door handle height)
            float handleHeight = 1.0f;  // Standard handle height from bottom
            handleObj.transform.localPosition = new Vector3(
                STILE_WIDTH / 2f + HANDLE_RADIUS,
                handleHeight,
                PANEL_THICKNESS / 2f + HANDLE_RADIUS
            );

            // Create handle sphere/knob
            GameObject knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            knob.name = "Knob";
            knob.transform.SetParent(handleObj.transform);
            knob.transform.localPosition = Vector3.zero;
            knob.transform.localScale = Vector3.one * HANDLE_RADIUS * 2f;
            
            var knobRenderer = knob.GetComponent<MeshRenderer>();
            if (knobRenderer != null)
                knobRenderer.sharedMaterial = GetHandleMaterial(variant);

            // Create handle plate (rosette)
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plate.name = "Rosette";
            plate.transform.SetParent(handleObj.transform);
            plate.transform.localPosition = new Vector3(0, 0, -0.02f);
            plate.transform.localScale = new Vector3(HANDLE_RADIUS * 3f, 0.02f, HANDLE_RADIUS * 3f);
            
            var plateRenderer = plate.GetComponent<MeshRenderer>();
            if (plateRenderer != null)
                plateRenderer.sharedMaterial = GetHandleMaterial(variant);

            // Rotate cylinder to face correctly
            plate.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        #endregion

        #region Hinges

        /// <summary>
        /// Create door hinges (3 standard hinges).
        /// </summary>
        private static void CreateHinges(GameObject doorLeaf, float depth, DoorVariant variant)
        {
            Material hingeMat = GetHingeMaterial();

            // Top hinge
            CreateHinge(doorLeaf, "Hinge_Top", depth * 0.8f, hingeMat);

            // Middle hinge
            CreateHinge(doorLeaf, "Hinge_Middle", depth * 0.5f, hingeMat);

            // Bottom hinge
            CreateHinge(doorLeaf, "Hinge_Bottom", depth * 0.2f, hingeMat);
        }

        /// <summary>
        /// Create a single hinge.
        /// </summary>
        private static void CreateHinge(GameObject doorLeaf, string name, float yPos, Material material)
        {
            GameObject hingeObj = new GameObject(name);
            hingeObj.transform.SetParent(doorLeaf.transform);

            // Position on left stile
            hingeObj.transform.localPosition = new Vector3(
                -STILE_WIDTH / 2f,
                yPos,
                0
            );

            // Create hinge plates
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "HingePlate";
            plate.transform.SetParent(hingeObj.transform);
            plate.transform.localPosition = Vector3.zero;
            plate.transform.localScale = new Vector3(HINGE_SIZE, HINGE_SIZE * 2f, HINGE_SIZE / 2f);
            
            var plateRenderer = plate.GetComponent<MeshRenderer>();
            if (plateRenderer != null)
                plateRenderer.sharedMaterial = material;
        }

        #endregion

        #region Helper Methods

        private static void CreateFramePiece(GameObject parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = name;
            piece.transform.SetParent(parent.transform);
            piece.transform.localPosition = pos;
            piece.transform.localRotation = Quaternion.identity;
            piece.transform.localScale = scale;
            
            var pieceRenderer = piece.GetComponent<MeshRenderer>();
            if (pieceRenderer != null)
                pieceRenderer.sharedMaterial = mat;
        }

        private static void AddDoorCollider(GameObject door, float width, float height, float depth)
        {
            BoxCollider collider = door.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, height / 2f, 0);
            collider.size = new Vector3(width, height, depth);
        }

        private static void AddDoorEffects(GameObject door, DoorVariant variant, DoorTrapType trap)
        {
            // Placeholder for particle effects, sounds, etc.
            // Can be expanded later
        }

        #endregion

        #region Materials (Using Pixel Art Textures)

        private static Material GetFrameMaterial(DoorVariant variant)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.mainTexture = PixelArtDoorTextures.GetWoodFrameTexture();
            mat.SetFloat("_Smoothness", 0.3f);
            return mat;
        }

        private static Material GetPanelMaterial(DoorVariant variant)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.mainTexture = GetPanelTextureForVariant(variant);
            mat.SetFloat("_Smoothness", 0.5f);
            return mat;
        }

        private static Material GetStileMaterial(DoorVariant variant)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.mainTexture = PixelArtDoorTextures.GetWoodStileTexture();
            mat.SetFloat("_Smoothness", 0.4f);
            return mat;
        }

        private static Material GetThresholdMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.mainTexture = PixelArtDoorTextures.GetWoodFrameTexture();
            mat.SetFloat("_Smoothness", 0.2f);
            return mat;
        }

        private static Material GetHandleMaterial(DoorVariant variant)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.mainTexture = PixelArtDoorTextures.GetHandleTexture();
            mat.SetFloat("_Smoothness", 0.8f);
            return mat;
        }

        private static Material GetHingeMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.mainTexture = PixelArtDoorTextures.GetHingeTexture();
            mat.SetFloat("_Smoothness", 0.6f);
            return mat;
        }

        private static Texture2D GetPanelTextureForVariant(DoorVariant variant)
        {
            return variant switch
            {
                DoorVariant.Normal => PixelArtDoorTextures.GetWoodPanelTexture(),
                DoorVariant.Locked => PixelArtDoorTextures.GetWoodPanelTexture(),
                DoorVariant.Trapped => PixelArtDoorTextures.GetStonePanelTexture(),
                DoorVariant.Secret => PixelArtDoorTextures.GetMagicPanelTexture(),
                DoorVariant.Blessed => PixelArtDoorTextures.GetMagicPanelTexture(),
                DoorVariant.Cursed => PixelArtDoorTextures.GetIronPanelTexture(),
                DoorVariant.Boss => PixelArtDoorTextures.GetIronPanelTexture(),
                DoorVariant.OneWay => PixelArtDoorTextures.GetWoodPanelTexture(),
                _ => PixelArtDoorTextures.GetWoodPanelTexture()
            };
        }

        #endregion
    }
}
