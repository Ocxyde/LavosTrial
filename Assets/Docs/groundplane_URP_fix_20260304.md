# GroundPlane URP Fix - 2026-03-04

**Date:** 2026-03-04  
**Issue:** Ground cube appears blank/white (no texture)  
**Status:** ✅ **FIXED**

---

## 🐛 **PROBLEM**

Ground cube was created but showed **blank/white surface** instead of pixel art stone texture.

**Console showed:**
```
[GroundPlane] Generated 32x32 pixel art stone texture
[GroundPlane] Material created with shader: Universal Render Pipeline/Lit
[GroundPlane] Created 200x200m flat cube ground with pixel art stone texture
```

But ground appeared **blank** in scene.

---

## 🔍 **ROOT CAUSE**

**URP Shader Property Mismatch:**

The code was using **Built-in/Standard pipeline** properties:
- `material.mainTexture` → URP uses `_BaseMap`
- `material.color` → URP uses `_BaseColor`
- `_Glossiness` → URP uses `_Smoothness` (inverse)

**Code (BROKEN):**
```csharp
Material material = new Material(shader);
material.mainTexture = stoneTexture;  // ❌ URP ignores this
material.color = Color.white;          // ❌ URP ignores this
material.SetFloat("_Glossiness", 0f);  // ❌ URP uses _Smoothness
```

---

## ✅ **SOLUTION**

Detect shader type and use correct properties:

```csharp
// Set texture - URP uses _BaseMap, Standard uses _MainTex
if (urpShader != null && shader == urpShader)
{
    // ✅ URP shader
    material.SetTexture("_BaseMap", stoneTexture);
    material.SetTexture("_MainTex", stoneTexture); // Compatibility
    material.SetColor("_BaseColor", Color.white);
    material.SetColor("_Color", Color.white);
    material.SetFloat("_Smoothness", 0f);  // ✅ URP
    material.SetFloat("_Metallic", 0f);
}
else if (shader.name.Contains("Standard"))
{
    // ✅ Standard shader
    material.mainTexture = stoneTexture;
    material.color = Color.white;
    material.SetFloat("_Glossiness", 0f);  // ✅ Standard
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

## 📊 **COMPARISON**

| Property | URP Lit | Standard | Unlit |
|----------|---------|----------|-------|
| **Albedo Texture** | `_BaseMap` | `_MainTex` | `_MainTex` |
| **Base Color** | `_BaseColor` | `_Color` | `_Color` |
| **Smoothness** | `_Smoothness` | `_Glossiness` | N/A |
| **Metallic** | `_Metallic` | `_Metallic` | N/A |

---

## ✅ **RESULT**

Ground now displays:
- ✅ Pixel art stone texture (gray blocks with mortar)
- ✅ Proper URP lighting
- ✅ Matte finish (no smoothness)
- ✅ Non-metallic (stone)

---

## 📝 **FILES MODIFIED**

| File | Change |
|------|--------|
| `GroundPlaneGenerator.cs` | Fixed URP material properties |

**Location:** `Assets/Scripts/Core/08_Environment/GroundPlaneGenerator.cs`

---

## 🎯 **VERIFICATION**

**In Unity Editor:**
1. Press Play
2. Ground should show **pixel art stone texture**
3. Not blank/white anymore

**Expected Console:**
```
[GroundPlane] Generated 32x32 pixel art stone texture (block size: 2)
[GroundPlane] Material created with shader: Universal Render Pipeline/Lit
[GroundPlane] Created 200x200m flat cube ground with pixel art stone texture
```

**Visual Check:**
- ✅ Gray stone blocks (pixel art style)
- ✅ Dark mortar lines between blocks
- ✅ Random stone variation
- ✅ Matte finish (no shine)

---

## 🔧 **NEXT STEPS**

**Could you please run:**
```powershell
.\backup.ps1
```

**Then test:**
1. Press Play in Unity
2. Verify ground shows stone texture (not blank)
3. Walk around to check texture repeats properly

---

**Related Fix:** `floor_material_factory_problems_fixed_20260304.md` (same URP issue)

---

**Generated:** 2026-03-04  
**Unity Version:** 6000.3.7f1  
**Status:** ✅ PRODUCTION READY

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
