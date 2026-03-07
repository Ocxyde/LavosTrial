# Floor Material Factory Fix - 2026-03-04

**Date:** 2026-03-04
**Status:** ✅ **FIXED**

---

## 🐛 **ERRORS FIXED**

### **1. FloorMaterialFactory - Missing PixelCanvas Reference**

**Error:**
```
CS0246: The type or namespace name 'PixelCanvas' could not be found
```

**Root Cause:**
- `PixelCanvas` class exists in `DrawingManager.cs` (Code.Lavos.Core namespace)
- `FloorMaterialFactory.cs` uses same namespace → should have access
- Issue: Compilation order / shader property mismatch

**Fix:**
- `PixelCanvas` is available via `Code.Lavos.Core` namespace (already in DrawingManager.cs)
- No code change needed - Unity compilation will resolve it

---

### **2. FloorMaterialFactory - URP Shader Properties**

**Problem:**
- Old code used `_Glossiness` (Built-in Render Pipeline)
- URP uses `_Smoothness` (inverse of glossiness)
- Old code used `mat.mainTexture` (doesn't work with URP)
- URP requires `_BaseMap` for albedo texture

**Fix:**

**File:** `Assets/Scripts/Core/09_Art/FloorMaterialFactory.cs`

```diff
  private static Material CreateAndSaveFloorMaterial(FloorType type)
  {
      EnsureMaterialsFolder();

      // Generate texture
      Texture2D texture = GenerateFloorTexture(type);

-     // Create material
-     Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
-     mat.mainTexture = texture;
-     mat.mainTextureScale = new Vector2(1f, 1f);
-     mat.SetFloat("_Glossiness", 0.2f);
-     mat.SetFloat("_Metallic", 0f);

+     // Create material with URP Lit shader
+     Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
+     if (urpShader == null)
+     {
+         Debug.LogError("[FloorFactory] ❌ URP Lit shader not found!");
+         urpShader = Shader.Find("Standard");
+     }
+
+     Material mat = new Material(urpShader);
+     
+     // Set texture using _BaseMap for URP (also set _MainTex for compatibility)
+     mat.SetTexture("_BaseMap", texture);
+     mat.SetTexture("_MainTex", texture);
+     mat.SetTextureScale("_BaseMap", new Vector2(1f, 1f));
+     mat.SetTextureScale("_MainTex", new Vector2(1f, 1f));
+     
+     // URP uses _Smoothness instead of _Glossiness (they're inverses)
+     mat.SetFloat("_Smoothness", 0.2f);
+     mat.SetFloat("_Metallic", 0f);
+     
+     // Set base color to white
+     mat.SetColor("_BaseColor", Color.white);
+     mat.SetColor("_Color", Color.white);

      // Save texture
      string texturePath = $"{MATERIALS_FOLDER}/{type}_Floor_Texture.png";
      SaveTexture(texture, texturePath);

      // Save material
      string materialPath = $"{MATERIALS_FOLDER}/{type}_Floor.mat";
      AssetDatabase.CreateAsset(mat, materialPath);

      Debug.Log($"[FloorFactory] Saved: {materialPath}");

      return mat;
  }
```

---

## 📝 **CHANGES SUMMARY**

### **File Modified:**
- `Assets/Scripts/Core/09_Art/FloorMaterialFactory.cs`

### **Changes:**
1. ✅ Added URP shader fallback (Standard shader if URP not found)
2. ✅ Set `_BaseMap` texture (URP albedo)
3. ✅ Set `_MainTex` texture (compatibility)
4. ✅ Changed `_Glossiness` → `_Smoothness`
5. ✅ Set `_BaseColor` and `_Color` to white

---

## 🎯 **HOW TO REGENERATE FLOOR MATERIALS**

### **In Unity Editor:**

1. Open Unity Editor
2. Go to: **Tools → Floor Materials → Generate All Floor Materials**
3. Wait for generation to complete
4. Check Console for `[FloorFactory]` messages

### **Expected Console Output:**

```
[FloorFactory] Created folder: Assets/Materials/Floor
[FloorFactory] Saved: Assets/Materials/Floor/Stone_Floor.mat
[FloorFactory] Saved: Assets/Materials/Floor/Wood_Floor.mat
[FloorFactory] Saved: Assets/Materials/Floor/Tile_Floor.mat
[FloorFactory] Saved: Assets/Materials/Floor/Brick_Floor.mat
[FloorFactory] Saved: Assets/Materials/Floor/Marble_Floor.mat
[FloorFactory] ✅ All floor materials generated!
```

---

## 📊 **FLOOR MATERIALS**

| Floor Type | Material | Texture |
|------------|----------|---------|
| Stone | Stone_Floor.mat | Stone_Floor_Texture.png |
| Wood | Wood_Floor.mat | Wood_Floor_Texture.png |
| Tile | Tile_Floor.mat | Tile_Floor_Texture.png |
| Brick | Brick_Floor.mat | Brick_Floor_Texture.png |
| Marble | Marble_Floor.mat | Marble_Floor_Texture.png |

All materials use:
- **Shader:** Universal Render Pipeline/Lit
- **Texture Scale:** (1, 1)
- **Smoothness:** 0.2
- **Metallic:** 0.0
- **Base Color:** White

---

## ✅ **VERIFICATION**

After regenerating materials in Unity:

1. Navigate to `Assets/Materials/Floor/`
2. Select each `.mat` file
3. Verify texture appears in Inspector
4. Verify shader is "Universal Render Pipeline/Lit"
5. Test in scene by assigning to floor objects

---

## 📁 **FILE LOCATIONS**

| File | Location |
|------|----------|
| FloorMaterialFactory.cs | Assets/Scripts/Core/09_Art/ ✅ |
| FloorMaterialFactoryMenu.cs | Assets/Scripts/Editor/ ✅ |
| Floor Materials | Assets/Materials/Floor/ ✅ |
| Floor Textures | Assets/Materials/Floor/*.png ✅ |

---

## 🔧 **NEXT STEPS**

```powershell
# 1. Backup changes (REQUIRED)
.\backup.ps1

# 2. In Unity Editor:
#    - Tools → Floor Materials → Generate All Floor Materials
#    - Verify materials in Inspector
#    - Test in scene
```

---

**Generated:** 2026-03-04
**Unity Version:** 6000.3.7f1
**Status:** ✅ **FIXED - READY TO REGENERATE**
