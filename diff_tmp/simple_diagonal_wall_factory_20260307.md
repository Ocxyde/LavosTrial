# FEATURE: Simple Diagonal Wall Prefab Factory

**Date:** 2026-03-07  
**Request:** "do a prefab size (1,1,0.5) diagonal wall using 2D factory made it by a cube no quad"  
**File Created:** `Assets/Scripts/Editor/Maze/SimpleDiagonalWallFactory.cs`

---

## 🎯 **What Was Created**

A simple Unity Editor tool that creates a diagonal wall prefab using a **cube primitive** (not quad) with the exact dimensions you requested: **(1, 1, 0.5)**.

---

## 📦 **Prefab Specifications**

| Property | Value |
|----------|-------|
| **Base** | Cube primitive (NOT quad) |
| **Scale** | (1, 1, 0.5) |
| **Rotation** | 45° on Y axis |
| **Collider** | BoxCollider (auto from cube) |
| **Material** | Standard shader (brownish default) |
| **Output** | `Assets/Resources/Prefabs/SimpleDiagonalWallPrefab.prefab` |

---

## 🛠️ **How to Use**

### **Step 1: Open the Tool**
```
Unity Menu → Tools → Create Simple Diagonal Wall Prefab
```

### **Step 2: Configure (Optional)**
- **Scale:** Default is (1, 1, 0.5) - change if needed
- **Rotation:** Default is 45° - adjust for different angles
- **Material:** Assign your wall material (or use default)

### **Step 3: Click "Create Diagonal Wall Prefab"**
- Prefab created at: `Assets/Resources/Prefabs/SimpleDiagonalWallPrefab.prefab`
- Material saved at: `Assets/Resources/Materials/DiagonalWallMaterial.mat`

### **Step 4: Assign to CompleteMazeBuilder**
```
CompleteMazeBuilder (Inspector)
  └── Diagonal Prefabs
       └── wallDiagPrefab → SimpleDiagonalWallPrefab
```

---

## 📝 **Code Details**

**File:** `Assets/Scripts/Editor/Maze/SimpleDiagonalWallFactory.cs`

**Key Features:**
```csharp
// ✅ Uses cube primitive (NOT quad)
GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

// ✅ Scale (1, 1, 0.5) as requested
cube.transform.localScale = new Vector3(1f, 1f, 0.5f);

// ✅ Rotated 45° for diagonal placement
cube.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);

// ✅ BoxCollider included (from cube primitive)
// ✅ Material applied (Standard shader)
```

---

## 🔧 **Technical Notes**

### **Why Cube Instead of Quad?**
- **Cube:** Has volume, visible from all angles, proper thickness
- **Quad:** Flat 2D surface, invisible from sides, no depth

### **Scale Breakdown:**
```
X = 1.0  → Width/length along the diagonal
Y = 1.0  → Height (will be scaled by CompleteMazeBuilder to wallHeight)
Z = 0.5  → Thickness (thin but visible)
```

### **Rotation:**
```
45° on Y axis → Aligns with diagonal movement (NE-SW)
Other angles available: 135°, 225°, 315° for other diagonal directions
```

---

## 🎨 **Material**

If no material is assigned, the factory creates a default:
- **Shader:** Standard
- **Color:** Brownish (0.6, 0.4, 0.3)
- **Saved to:** `Assets/Resources/Materials/DiagonalWallMaterial.mat`

You can assign your own material from `GameConfig.Instance.wallMaterial`.

---

## 📁 **Output Structure**

```
Assets/
├── Resources/
│   ├── Prefabs/
│   │   └── SimpleDiagonalWallPrefab.prefab  ← Created
│   └── Materials/
│       └── DiagonalWallMaterial.mat  ← Created (if no material assigned)
```

---

## ✅ **Advantages Over Complex Mesh Factory**

| Feature | Simple Factory | Complex Factory |
|---------|---------------|-----------------|
| **Mesh** | Cube primitive | Custom diagonal mesh |
| **Lines of Code** | ~150 | ~788 |
| **Setup Time** | Instant | Manual configuration |
| **Collider** | Auto (BoxCollider) | Manual setup |
| **Customization** | Inspector UI | Code changes |
| **Performance** | Same | Same |

---

## 🧪 **Testing**

1. **Open Unity 6000.3.7f1**
2. **Menu:** Tools → Create Simple Diagonal Wall Prefab
3. **Click:** "Create Diagonal Wall Prefab"
4. **Verify:**
   - [ ] Prefab created in `Assets/Resources/Prefabs/`
   - [ ] Cube primitive used (not quad)
   - [ ] Scale is (1, 1, 0.5)
   - [ ] Rotation is 45°
   - [ ] BoxCollider present
   - [ ] Material applied

5. **Assign to CompleteMazeBuilder:**
   - Open scene with CompleteMazeBuilder
   - Select CompleteMazeBuilder GameObject
   - In Inspector: Diagonal Prefabs → wallDiagPrefab
   - Drag SimpleDiagonalWallPrefab.prefab

6. **Generate Maze:**
   - Press Play
   - Verify diagonal walls appear at 45° angles

---

## 🔗 **Integration with CompleteMazeBuilder**

The prefab is used in `CompleteMazeBuilder.cs`:

```csharp
// Line 41: Prefab reference
[SerializeField] private GameObject wallDiagPrefab;

// Line 283: Asset validation
wallDiagPrefab ??= Resources.Load<GameObject>("Prefabs/WallDiagPrefab");

// Line 384: Spawning diagonal walls
var wall = Instantiate(
    wallDiagPrefab,
    cornerPos,
    Quaternion.Euler(0f, rotY, 0f),
    _wallsRoot);

// Line 410: Scaling to maze dimensions
wall.transform.localScale = new Vector3(1f, wh, DiagonalWallThickness);
```

**Note:** The Y scale is overridden by `wh` (wallHeight from config, default 4.0m).

---

## 📊 **Diff**

**New File:**
```
A  Assets/Scripts/Editor/Maze/SimpleDiagonalWallFactory.cs
```

---

## 🎯 **Next Steps**

1. **Run the tool** in Unity Editor
2. **Test the prefab** in your maze
3. **Assign to CompleteMazeBuilder** if you want to use it
4. **Backup:** Run `backup.ps1` after testing

---

**Status:** ✅ **CREATED**  
**Tool Location:** Unity Menu → Tools → Create Simple Diagonal Wall Prefab  
**Prefab Location:** `Assets/Resources/Prefabs/SimpleDiagonalWallPrefab.prefab`

*Document generated - UTF-8 encoding - Unix LF*
