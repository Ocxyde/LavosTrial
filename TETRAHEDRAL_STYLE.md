# Tetrahedral Art Style Guide - LavosTrial

**Goal:** Make all geometry appear to be constructed from tetrahedrons (4-sided pyramids)

---

## What Is Tetrahedral Style?

A **low-poly aesthetic** where all 3D models appear to be built from tetrahedrons, giving the game a distinctive geometric look similar to:
- Low-poly art
- Quixel-style photogrammetry with geometric simplification
- Voxel-based games but with triangles instead of cubes

---

## Implementation Methods

### Method 1: Tetrahedral Meshes (Recommended)
Use the `TetrahedronMesh.cs` script to generate tetrahedron-based geometry:

```csharp
// In MazeRenderer.cs, replace wall creation with:
TetrahedronMesh.CreateTetrahedralWall(position, rotation, size, material);
```

### Method 2: Low-Poly Materials
Create materials with:
- **Flat shading** (no smooth normals)
- **High contrast** between faces
- **Stone/earth tones** (grays, browns, warm colors)

### Method 3: Post-Processing
Add post-processing effects:
- **Vignette** (darkened edges)
- **Ambient Occlusion** (depth shadows)
- **Color Grading** (warm, earthy tones)

---

## Material Settings

### Stone Wall Material
```
Shader: Universal Render Pipeline/Lit
Albedo: #6B6359 (warm gray-brown)
Smoothness: 0.1 (very matte)
Normal: Subtle stone noise
```

### Floor Material
```
Shader: Universal Render Pipeline/Lit
Albedo: #4A4238 (darker brown)
Smoothness: 0.15
Tiling: 0.5 (larger pattern)
```

### Torch Flame Material
```
Shader: Unlit/Transparent
Color: Gradient from yellow to orange
Blend Mode: Additive
```

---

## Lighting Setup

### Key Settings
- **Ambient Light:** Warm orange-brown (#2A2420)
- **Fog:** Linear, warm brown (#1A1510)
- **Point Lights:** Orange (1.0, 0.6, 0.3) for torches
- **Shadows:** Soft, low resolution for retro feel

---

## Next Steps

1. **Open MazeRenderer.cs**
2. **Replace primitive cubes** with tetrahedral meshes
3. **Apply low-poly materials**
4. **Adjust lighting** for warm, dungeon atmosphere

---

## Visual Reference

```
Standard Cube Wall:          Tetrahedral Wall:
┌─────────────┐             ▲▲▲▲▲▲▲▲▲▲▲▲
│             │            ▲▲▲▲▲▲▲▲▲▲▲▲▲▲
│   Smooth    │           ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
│   Surface   │          ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
│             │         ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
└─────────────┘        ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                      (Made of individual tetrahedrons)
```

---

## Files Created

| File | Purpose |
|------|---------|
| `TetrahedronMesh.cs` | Generate tetrahedral geometry |
| `TetrahedronMesh.cs.meta` | Unity metadata |

---

## Usage Example

```csharp
// In your maze generation code:
GameObject wall = TetrahedronMesh.CreateTetrahedralWall(
    position: new Vector3(x, y, z),
    rotation: Quaternion.identity,
    size: new Vector3(4f, 3f, 0.3f),
    material: stoneMaterial
);
```
