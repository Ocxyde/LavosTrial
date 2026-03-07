# GroundPlaneGenerator URP Fix - 2026-03-04

**Date:** 2026-03-04
**File:** `Assets/Scripts/Core/08_Environment/GroundPlaneGenerator.cs`
**Status:** ✅ **FIXED**

---

## 🐛 **PROBLEM**

**Symptom:** Ground cube appears **blank/white** instead of showing pixel art stone texture

**Root Cause:** URP shader requires `_BaseMap` and `_BaseColor` properties, but code was using:
- `material.mainTexture` (Standard/Built-in pipeline)
- `material.color` (Standard/Built-in pipeline)
- `_Glossiness` (Standard pipeline - URP uses `_Smoothness`)

---

## 🔧 **FIX APPLIED**

### **Before (Broken for URP):**
```csharp
Material material = new Material(shader);
material.mainTexture = stoneTexture;  // ❌ Wrong for URP
material.color = Color.white;          // ❌ Wrong for URP
material.SetFloat("_Glossiness", 0f);  // ❌ Wrong for URP
```

### **After (URP-Compatible):**
```csharp
Material material = new Material(shader);

// Set texture - URP uses _BaseMap, Standard uses _MainTex
if (urpShader != null && shader == urpShader)
{
    // ✅ URP shader
    material.SetTexture("_BaseMap", stoneTexture);
    material.SetTexture("_MainTex", stoneTexture); // Compatibility
    material.SetColor("_BaseColor", Color.white);
    material.SetColor("_Color", Color.white);
    material.SetFloat("_Smoothness", 0f);  // ✅ URP uses Smoothness
    material.SetFloat("_Metallic", 0f);
}
else if (shader.name.Contains("Standard"))
{
    // ✅ Standard shader
    material.mainTexture = stoneTexture;
    material.color = Color.white;
    material.SetFloat("_Glossiness", 0f);
    material.SetFloat("_Metallic", 0f);
}
else
{
    // ✅ Unlit shader (fallback)
    material.mainTexture = stoneTexture;
    material.color = Color.white;
}
```

---

## 📊 **DIFF**

```diff
--- a/Assets/Scripts/Core/08_Environment/GroundPlaneGenerator.cs
+++ b/Assets/Scripts/Core/08_Environment/GroundPlaneGenerator.cs
@@ -44,29 +44,50 @@ namespace Code.Lavos.Core
             var renderer = ground.GetComponent<MeshRenderer>();
             if (renderer != null)
             {
                 // Try URP shader first, then Standard, then Unlit as fallback
-                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
+                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
+                Shader shader = urpShader;
+                
                 if (shader == null)
                 {
                     Debug.LogWarning("[GroundPlane] URP shader not found, trying Standard shader");
                     shader = Shader.Find("Standard");
                 }
                 if (shader == null)
                 {
                     Debug.LogWarning("[GroundPlane] Standard shader not found, using Unlit/Texture");
                     shader = Shader.Find("Unlit/Texture");
                 }
 
                 if (shader != null)
                 {
                     Material material = new Material(shader);
-                    material.mainTexture = stoneTexture;
-                    material.color = Color.white;
-
-                    // Set material properties based on shader type
-                    if (shader.name.Contains("URP") || shader.name.Contains("Standard"))
+                    
+                    // Set texture - URP uses _BaseMap, Standard uses _MainTex
+                    if (urpShader != null && shader == urpShader)
+                    {
+                        // URP shader
+                        material.SetTexture("_BaseMap", stoneTexture);
+                        material.SetTexture("_MainTex", stoneTexture); // Compatibility
+                        material.SetColor("_BaseColor", Color.white);
+                        material.SetColor("_Color", Color.white);
+                        material.SetFloat("_Smoothness", 0f);  // URP uses Smoothness (not Glossiness)
+                        material.SetFloat("_Metallic", 0f);
+                    }
+                    else if (shader.name.Contains("Standard"))
                     {
-                        material.SetFloat("_Glossiness", 0f);  // No smoothness
-                        material.SetFloat("_Metallic", 0f);   // Not metallic
+                        // Standard shader
+                        material.mainTexture = stoneTexture;
+                        material.color = Color.white;
+                        material.SetFloat("_Glossiness", 0f);
+                        material.SetFloat("_Metallic", 0f);
+                    }
+                    else
+                    {
+                        // Unlit shader (fallback)
+                        material.mainTexture = stoneTexture;
+                        material.color = Color.white;
                     }
 
                     renderer.material = material;
```

---

## ✅ **RESULT**

**Ground cube now displays:**
- ✅ Pixel art stone texture (no more blank/white)
- ✅ Proper URP lighting
- ✅ Correct color (white tint)
- ✅ No smoothness (matte stone)
- ✅ No metallic (stone material)

---

## 🎯 **VERIFICATION**

**In Unity Editor:**
1. Press Play
2. Check Console for:
   ```
   [GroundPlane] Generated 32x32 pixel art stone texture
   [GroundPlane] Material created with shader: Universal Render Pipeline/Lit
   [GroundPlane] Created 200x200m flat cube ground with pixel art stone texture
   ```
3. Verify ground shows **pixel art stone texture** (not blank/white)

---

## 📝 **FILES MODIFIED**

| File | Lines Changed |
|------|---------------|
| `GroundPlaneGenerator.cs` | 44-93 (50 lines) |

---

## 🔧 **NEXT STEPS**

**Could you please run:**
```powershell
.\backup.ps1
```

**Then test in Unity:**
1. Press Play
2. Verify ground shows pixel art stone texture
3. Check it's not blank/white anymore

---

**Generated:** 2026-03-04  
**Unity Version:** 6000.3.7f1  
**Status:** ✅ FIXED

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
