# 2D Pixel Art 8-Bit Material Setup Guide

**Date:** 2026-03-07
**Unity Version:** 6000.3.7f1 (URP)
**Art Style:** 2D Pixel Art 8-Bit Retro

---

## 📋 **MATERIALS TO CREATE**

### **Required Materials:**

| Material | Path | Texture | Purpose |
|----------|------|---------|---------|
| `WallMaterial.mat` | `Assets/Materials/` | `wall_texture.png` | Maze walls |
| `FloorMaterial.mat` | `Assets/Materials/Floor/` | `floor_texture.png` | Maze floor |
| `DoorMaterial.mat` | `Assets/Materials/` | `door_sprite_sheet.png` | Doors |

---

## 🎨 **STEP-BY-STEP SETUP**

### **Step 1: Import Texture Settings**

**For ALL textures (wall, floor, door):**

1. Select texture in Project window
2. In Inspector, set:

```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Filter Mode: Point (no filter)  ← CRITICAL for pixel art!
Compression: None
Format: Truecolor (32 bit)
Pixels Per Unit: 16 (or your PPU)
```

3. Click **Apply**

---

### **Step 2: Create Wall Material**

**File:** `Assets/Materials/WallMaterial.mat`

1. **Select/Create material:**
   - Right-click `Assets/Materials/` → Create → Material
   - Name: `WallMaterial`

2. **Shader Settings:**
   ```
   Shader: Universal Render Pipeline → 2D → Sprite Unlit
   ```

3. **Texture Assignment:**
   ```
   Base Map: Assets/Textures/wall_texture.png
   Main Texture: Assets/Textures/wall_texture.png
   ```

4. **Color Settings:**
   ```
   Base Color: RGBA(255, 255, 255, 255)  ← White (tint from texture)
   ```

5. **Surface Options:**
   ```
   Surface Type: Opaque
   Blend Mode: Alpha
   Cull Mode: Back
   ```

6. **Pixel Art Settings:**
   ```
   Filter Mode: Point (no filter)  ← Already set on texture
   Pixel Snap: On
   ```

---

### **Step 3: Create Floor Material**

**File:** `Assets/Materials/Floor/Stone_Floor.mat`

1. **Select/Create material:**
   - Right-click `Assets/Materials/Floor/` → Create → Material
   - Name: `Stone_Floor`

2. **Shader Settings:**
   ```
   Shader: Universal Render Pipeline → 2D → Sprite Unlit
   ```

3. **Texture Assignment:**
   ```
   Base Map: Assets/Textures/floor_texture.png
   Main Texture: Assets/Textures/floor_texture.png
   ```

4. **Color Settings:**
   ```
   Base Color: RGBA(255, 255, 255, 255)
   ```

5. **Surface Options:**
   ```
   Surface Type: Opaque
   ```

---

### **Step 4: Create Door Material**

**File:** `Assets/Materials/Door_PixelArt.mat`

1. **Select/Create material:**
   - Right-click `Assets/Materials/` → Create → Material
   - Name: `Door_PixelArt`

2. **Shader Settings:**
   ```
   Shader: Universal Render Pipeline → 2D → Sprite Unlit
   ```

3. **Texture Assignment:**
   ```
   Base Map: Assets/Textures/door_sprite_sheet.png
   Main Texture: Assets/Textures/door_sprite_sheet.png
   ```

4. **Sprite Settings (if animated):**
   ```
   Sprite Mode: Multiple
   Click "Sprite Editor" to slice frames
   ```

---

## 🔧 **URP 2D RENDERER SETTINGS**

### **Configure 2D Renderer:**

1. Open `Project Settings` → `Graphics`
2. Set **Scriptable Render Pipeline Asset** to your URP asset
3. In URP Asset settings:
   ```
   Renderer Type: 2D Renderer
   2D Lights: On
   Light Shadows: On (for torch lighting)
   ```

---

## 🎮 **PREFAB UPDATES**

### **WallPrefab:**

1. Open `Assets/Resources/Prefabs/WallPrefab.prefab`
2. Select the GameObject
3. In MeshRenderer component:
   ```
   Materials → Element 0: WallMaterial.mat
   ```
4. Apply changes

### **FloorTilePrefab:**

1. Open `Assets/Resources/Prefabs/FloorTilePrefab.prefab`
2. Select the GameObject
3. In MeshRenderer component:
   ```
   Materials → Element 0: Stone_Floor.mat
   ```
4. Apply changes

---

## ✅ **VERIFICATION CHECKLIST**

After setup, verify:

- [ ] All textures use **Point (no filter)** filter mode
- [ ] All materials use **Sprite Unlit** shader (for 2D pixel art)
- [ ] All materials have textures assigned (not solid colors)
- [ ] WallMaterial is white (no tint)
- [ ] FloorMaterial is white (no tint)
- [ ] Prefabs reference the correct materials
- [ ] No mipmaps enabled (causes blur on pixel art)

---

## 🎨 **8-BIT COLOR PALETTE RECOMMENDATIONS**

For authentic 8-bit look, limit colors:

**Wall Colors (Stone):**
```
#8B8B8B  Light gray
#6B6B6B  Medium gray
#4B4B4B  Dark gray
#3B3B3B  Shadow
```

**Floor Colors (Stone):**
```
#7A7A7A  Base gray
#5A5A5A  Dark gray
#4A4A4A  Shadow
#9A9A9A  Highlight
```

**Door Colors (Wood):**
```
#8B4513  Brown
#A0522D  Light brown
#654321  Dark brown
#D2691E  Highlight
```

---

## 🔍 **TROUBLESHOOTING**

### **Walls appear solid color (no texture):**
- Check material has texture assigned to **Base Map**
- Verify texture import settings (Sprite type, not Default)
- Check shader is URP 2D Sprite shader

### **Textures look blurry:**
- Set **Filter Mode** to **Point (no filter)**
- Disable **Generate Mip Maps** in texture import
- Check camera uses **Orthographic** projection (for 2D)

### **Colors look wrong:**
- Material **Base Color** should be white (RGBA 255,255,255,255)
- Check texture is not sRGB compressed
- Verify URP color space (Project Settings → Player → Color Space)

### **Pixel edges show gaps:**
- Enable **Pixel Snap** in material
- Set camera **Projection** to Orthographic
- Use integer camera orthographic size (e.g., 5, 10, 15)

---

## 📊 **RECOMMENDED CAMERA SETTINGS (2D Pixel Art)**

```
Projection: Orthographic
Size: 5 (or multiple of pixel grid)
Near Clip: 0.3
Far Clip: 1000
Culling Mask: Everything
Allow HDR: Off
Allow MSAA: Off (for pixel art)
```

---

## 🎯 **FINAL STEPS**

1. ✅ Create all materials with correct shaders
2. ✅ Assign textures to materials
3. ✅ Update prefabs to use new materials
4. ✅ Test in Unity Play mode
5. ✅ Verify pixel art looks crisp (no blur)
6. ✅ Run backup: `.\backup.ps1`
7. ✅ Commit to git

---

**Status:** ✅ READY FOR MANUAL SETUP
**Art Style:** 2D Pixel Art 8-Bit
**Render Pipeline:** URP 2D

---

*Guide generated - Unity 6 (6000.3.7f1) URP 2D compatible*
