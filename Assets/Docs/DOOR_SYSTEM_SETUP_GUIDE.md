﻿# Door System Setup Guide

**Date:** 2026-03-11
**Status:** ✅ READY TO USE
**Features:** 8-bit Pixel Art Textures Included!

---

## 📋 OVERVIEW

Complete interactive door system with:
- ✅ **Brown 2D Pixel Art Textures** (8-bit style)
- ✅ Outward rotation (90° swing)
- ✅ "F" key interaction
- ✅ Smooth animation
- ✅ Lock/unlock support
- ✅ Auto-generated prefabs
- ✅ Sound effects support

---

## 🎨 QUICK START - PIXEL ART DOORS (5 Minutes)

### **Step 1: Generate Pixel Art Textures**

**In Unity Editor:**

1. **Tools → Generate Door Pixel Art Textures**
2. Click **"Generate All Door Textures"**
3. Textures created in `Assets/Textures/Doors/`:
   - `Door_Wood.png` (brown wood planks)
   - `Door_Metal.png` (reinforced with bands)
   - `Door_Secret.png` (dark, subtle)
   - `Door_Exit.png` (stone/panel style)

**Each texture includes:**
- Wood planks or panels
- Door frame/border
- Metal hinges
- Door handle/knocker
- 8-bit pixel art style (64x64)

---

### **Step 2: Create Door Prefabs**

**In Unity Editor:**

1. **Tools → Create Door Prefab**
2. Select door type (Normal/Locked/Secret/Exit)
3. Click **"Create Door Prefab"**
4. Prefab auto-assigns pixel art texture!
5. Repeat for each type

**Prefabs created in:** `Assets/Resources/Prefabs/`

---

### **Step 3: Assign to CompleteMazeBuilder8**

**In Unity Editor:**

1. Select `MazeBuilder` GameObject
2. Find **CompleteMazeBuilder8** component
3. **Door Prefabs** section:
   - **Door Prefab:** `DoorPrefab`
   - **Locked Door Prefab:** `LockedDoorPrefab`
   - **Secret Door Prefab:** `SecretDoorPrefab`
   - **Exit Door Prefab:** `ExitDoorPrefab`

---

### **Step 4: Add Player Interaction**

**In Unity Editor:**

1. Select **Player** GameObject
2. **Add Component:** `DoorInteractionHandler`
3. Configure settings:
   - **Interaction Distance:** 3.0
   - **Interaction Key:** F
   - **Show Prompt:** ✓

---

### **Step 5: Test!**

1. **Tools → Maze → Generate Maze** (Ctrl+Alt+G)
2. **Press Play**
3. **Walk up to a door**
4. **Press "F"** - Pixel art door swings open!
5. **Press "F" again** - Door closes!

---

## 🎨 DOOR TEXTURE DETAILS

| Texture | Style | Size | Features |
|---------|-------|------|----------|
| **Door_Wood.png** | Wood planks | 64x64 | Vertical planks, hinges, handle |
| **Door_Metal.png** | Reinforced | 64x64 | Wood + metal bands, rivets |
| **Door_Secret.png** | Hidden | 64x64 | Subtle pattern, hidden handle |
| **Door_Exit.png** | Panel/Stone | 64x64 | Recessed panels, ornate |

**All textures:**
- 8-bit pixel art style
- Point filter (no blur)
- Brown wood color palette
- Metal details (hinges, handles)
- Transparent background

---

## 🔧 MANUAL TEXTURE SETUP (Optional)

If textures don't auto-assign:

1. Select door prefab (e.g., `DoorPrefab`)
2. Find **Mesh Renderer** component
3. **Materials** → Select material (e.g., `Door_Wood_Mat`)
4. **Inspector** → Drag texture to **Albedo** slot:
   - `Door_Wood.png` → Wood door
   - `Door_Metal.png` → Locked door
   - etc.

---

## 📁 FILES CREATED

| File | Purpose |
|------|---------|
| `DoorController.cs` | Main door behavior with rotation |
| `DoorInteractionHandler.cs` | Player "F" key interaction |
| `CreateDoorPrefab.cs` | Editor tool to auto-create prefabs |
| `DoorPixelArtGenerator.cs` | **Generates 8-bit door textures** |
| `DOOR_SYSTEM_SETUP_GUIDE.md` | This document |

**Textures Generated:**
- `Assets/Textures/Doors/Door_Wood.png`
- `Assets/Textures/Doors/Door_Metal.png`
- `Assets/Textures/Doors/Door_Secret.png`
- `Assets/Textures/Doors/Door_Exit.png`

---

## ✅ VERIFICATION CHECKLIST

After setup, verify:

- [ ] Pixel art textures generated (Tools → Generate Door Pixel Art Textures)
- [ ] 4 door prefabs created (Tools → Create Door Prefab)
- [ ] Prefabs assigned to CompleteMazeBuilder8
- [ ] DoorInteractionHandler on Player
- [ ] Generate maze (Ctrl+Alt+G)
- [ ] Press Play
- [ ] Walk to door
- [ ] **Verify brown 2D pixel art appearance**
- [ ] Press "F" - door opens outward
- [ ] Press "F" - door closes
- [ ] No console errors

---

**License:** GPL-3.0
**Copyright © 2026 CodeDotLavos. All rights reserved.**

---

*Last Updated: 2026-03-11 | Unity 6000.3.10f1 | UTF-8 encoding - Unix LF*

**Status:** ✅ READY - Brown 2D pixel art doors included!

**Happy coding with me : Ocxyde :)**
