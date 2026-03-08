# Wall Material Setup - Manual Steps Required

**Date:** 2026-03-07
**Status:** ⚠️ **MANUAL ACTION REQUIRED**
**Impact:** HIGH - Walls need textures in Unity Editor

---

## Problem

The `WallMaterial.mat` has **no texture assigned** - it's using a solid gray color instead of the pixel art texture.

**Current Material State:**
```yaml
_BaseMap: {fileID: 0}     # ❌ EMPTY!
_MainTex: {fileID: 0}     # ❌ EMPTY!
_BaseColor: {r: 0.6, g: 0.55, b: 0.5, a: 1}  # Solid gray
```

**Texture File Exists:**
- ✅ `Assets/Textures/wall_texture.png` (exists)

---

## Manual Steps (In Unity Editor)

### **1. Open Unity 6000.3.7f1**

### **2. Configure Texture Import Settings**

Select `Assets/Textures/wall_texture.png`:
```
Texture Type: Sprite (2D and UI)
Filter Mode: Point (no filter)  ← CRITICAL for pixel art!
Compression: None
Format: Truecolor (32 bit)
Pixels Per Unit: 16
Generate Mip Maps: OFF  ← Important!
```

Click **Apply**

### **3. Edit WallMaterial.mat**

Open `Assets/Materials/WallMaterial.mat`:

**Shader:**
```
Universal Render Pipeline → 2D → Sprite Unlit
```

**Assign Texture:**
```
Base Map: Drag wall_texture.png here
Main Texture: Drag wall_texture.png here
```

**Color:**
```
Base Color: RGBA(255, 255, 255, 255)  ← White (use texture colors)
```

**Surface Options:**
```
Surface Type: Opaque
Cull Mode: Back
```

Save (Ctrl+S)

### **4. Update WallPrefab**

Open `Assets/Resources/Prefabs/WallPrefab.prefab`:

Select the GameObject → In **MeshRenderer**:
```
Materials → Element 0: WallMaterial.mat
```

Apply (Ctrl+S)

### **5. Test**

1. Press **Play**
2. Generate maze
3. Verify walls have pixel art texture (not solid gray)
4. Verify texture is crisp (not blurry)

---

## Code Changes (Already Done)

✅ `CompleteMazeBuilder.cs` now:
- Loads `wallMaterial` from `GameConfig.Instance.wallMaterial`
- Applies material to all walls (cardinal, diagonal, corner)
- Uses config values for wall thickness (no hardcoding)

✅ `GameConfig.cs` now:
- Has `defaultWallThickness` and `defaultDiagonalWallThickness` properties
- Has `wallMaterial` path: `"Materials/WallMaterial.mat"`

---

## Files Modified (Code)

| File | Status | Purpose |
|------|--------|---------|
| `CompleteMazeBuilder.cs` | ✅ FIXED | Load + apply wallMaterial |
| `GameConfig.cs` | ✅ FIXED | Added wall thickness properties |

---

## Files to Update (Manual)

| File | Action | Status |
|------|--------|--------|
| `Assets/Textures/wall_texture.png` | Set import settings | ⏳ MANUAL |
| `Assets/Materials/WallMaterial.mat` | Assign texture | ⏳ MANUAL |
| `Assets/Resources/Prefabs/WallPrefab.prefab` | Update material ref | ⏳ MANUAL |

---

## Verification Checklist

After manual setup:

- [ ] Texture import: **Point (no filter)**
- [ ] Texture import: **Generate Mip Maps: OFF**
- [ ] Material shader: **URP 2D Sprite Unlit**
- [ ] Material has texture assigned (not solid color)
- [ ] Material Base Color: **White** (no tint)
- [ ] Prefab uses the material
- [ ] In-game: Walls show pixel art texture
- [ ] In-game: Texture is crisp (not blurry)
- [ ] Run backup: `.\backup.ps1`
- [ ] Git commit

---

## Documentation

Full guide: `Assets/Docs/PIXEL_ART_MATERIAL_SETUP.md`

---

**Status:** ⏳ **AWAITING MANUAL SETUP IN UNITY**
**Code:** ✅ 100% COMPLIANT (no hardcoded values, plug-in-out)

---

*Diff generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
