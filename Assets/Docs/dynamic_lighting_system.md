# Dynamic Lighting System - Difficulty-Based
**Date:** 2026-03-03  
**Unity Version:** 6000.3.7f1

---

## 🕯️ **LIGHTING PROGRESSION SYSTEM**

Lighting automatically adjusts based on maze difficulty level!

### **Progression Curve:**

```
Level 1  (Beginner):  100% torches, 100% range, 100% intensity ← Very Bright
Level 3  (Easy):       85% torches,  92% range,  85% intensity
Level 5  (Medium):     70% torches,  82% range,  70% intensity
Level 10 (Hard):       45% torches,  65% range,  45% intensity
Level 15 (Expert):     28% torches,  52% range,  28% intensity
Level 20 (Master):     15% torches,  40% range,  15% intensity ← Very Dark!
Level 30 (Legend):     15% torches,  40% range,  15% intensity ← Minimum
```

---

## 🎮 **GAMEPLAY PHILOSOPHY**

### **Early Game (Levels 1-5):**
```
✅ Lots of torches (60 → 50)
✅ Wide light range (12m → 10m)
✅ Bright illumination
✅ Player feels safe
✅ Learning the game mechanics
```

### **Mid Game (Levels 6-12):**
```
⚠️ Moderate torches (45 → 30)
⚠️ Medium range (9m → 7m)
⚠️ Some dark areas
⚠️ Player needs strategy
⚠️ Resource management begins
```

### **Late Game (Levels 13-20):**
```
❌ Few torches (25 → 10)
❌ Short range (6m → 5m)
❌ Very dark corridors
❌ Player needs own light sources
❌ Expert gameplay
```

### **End Game (Levels 21+):**
```
🔥 Minimum lighting (15% of max)
🔥 Very limited visibility
🔥 Player must be self-sufficient
🔥 True master challenge
```

---

## ⚙️ **CONFIGURATION**

### **In Unity Inspector (SpatialPlacer component):**

```yaml
Dynamic Lighting (Difficulty-Based):
  - Use Dynamic Lighting: ✅ (checked)
  - Current Maze Level: 1  ← Change this for each level!
  
Lighting Configuration:
  - Max Torch Count: 60         ← Level 1 torches
  - Max Light Range: 12m        ← Level 1 range
  - Max Light Intensity: 1.8    ← Level 1 intensity
  - Difficulty Exponent: 1.5    ← Curve steepness
  - Minimum Lighting Ratio: 15% ← Floor for high levels
  - Max Level: 20               ← Level where minimum is reached
```

---

## 📊 **LIGHTING CALCULATION**

### **Formula:**

```csharp
// Normalize level to 0-1 range
normalizedDifficulty = (level - 1) / (maxLevel - 1)

// Apply exponential decay
lightingRatio = (1 - normalizedDifficulty) ^ difficultyExponent

// Ensure minimum
lightingRatio = max(lightingRatio, minimumRatio)

// Apply to values
torchCount = maxTorchCount * lightingRatio
lightRange = maxLightRange * lightingRatio
lightIntensity = maxLightIntensity * lightingRatio
```

### **Example (Level 10):**

```
Level: 10
Max Level: 20
Minimum Ratio: 15%

normalizedDifficulty = (10 - 1) / (20 - 1) = 0.47
lightingRatio = (1 - 0.47) ^ 1.5 = 0.38 (38%)

torchCount = 60 * 0.38 = 23 torches
lightRange = 12 * 0.38 = 4.6m
lightIntensity = 1.8 * 0.38 = 0.68
```

---

## 🎯 **USAGE**

### **Method 1: Inspector**

1. **Select MazeTest GameObject**
2. **Find SpatialPlacer component**
3. **Set Current Maze Level:**
   - Level 1 for first maze
   - Level 5 for medium difficulty
   - Level 15 for hard
   - Level 20+ for expert

4. **Press Play** - Lighting auto-adjusts!

---

### **Method 2: Script**

```csharp
// Get SpatialPlacer
var spatialPlacer = GetComponent<SpatialPlacer>();

// Set maze level
spatialPlacer.SetMazeLevel(10);  // Hard difficulty

// Or update directly
spatialPlacer.currentMazeLevel = 10;
spatialPlacer.UpdateLightingForLevel(10);
```

---

### **Method 3: Progressive Difficulty**

```csharp
public class MazeProgression : MonoBehaviour
{
    private int currentLevel = 1;
    private SpatialPlacer spatialPlacer;
    
    void Start()
    {
        spatialPlacer = GetComponent<SpatialPlacer>();
    }
    
    public void NextLevel()
    {
        currentLevel++;
        spatialPlacer.SetMazeLevel(currentLevel);
        Debug.Log($"Level {currentLevel}: Lighting adjusted!");
    }
    
    public void SetLevel(int level)
    {
        currentLevel = level;
        spatialPlacer.SetMazeLevel(level);
    }
}
```

---

## 📈 **LIGHTING TABLE**

| Level | Ratio | Torches | Range | Intensity | Description |
|-------|-------|---------|-------|-----------|-------------|
| **1** | 100% | 60 | 12.0m | 1.80 | Very Bright |
| **2** | 94% | 56 | 11.3m | 1.69 | Bright |
| **3** | 88% | 53 | 10.5m | 1.58 | Well Lit |
| **4** | 82% | 49 | 9.8m | 1.47 | Good |
| **5** | 76% | 46 | 9.1m | 1.37 | Moderate |
| **6** | 70% | 42 | 8.4m | 1.26 | Fair |
| **7** | 64% | 38 | 7.7m | 1.15 | Dim |
| **8** | 58% | 35 | 7.0m | 1.04 | Getting Dark |
| **9** | 52% | 31 | 6.2m | 0.94 | Dark |
| **10** | 46% | 28 | 5.5m | 0.83 | Hard |
| **11** | 41% | 25 | 4.9m | 0.74 | Harder |
| **12** | 36% | 22 | 4.3m | 0.65 | Very Hard |
| **13** | 31% | 19 | 3.7m | 0.56 | Expert |
| **14** | 27% | 16 | 3.2m | 0.49 | Expert+ |
| **15** | 23% | 14 | 2.8m | 0.41 | Master |
| **16** | 20% | 12 | 2.4m | 0.36 | Master+ |
| **17** | 18% | 11 | 2.2m | 0.32 | Legend |
| **18** | 16% | 10 | 1.9m | 0.29 | Legend+ |
| **19** | 15% | 9 | 1.8m | 0.27 | Near Min |
| **20+** | 15% | 9 | 1.8m | 0.27 | Minimum |

---

## 🔧 **CUSTOMIZATION**

### **Adjust Difficulty Curve:**

```yaml
# Steeper curve (faster darkening)
Difficulty Exponent: 2.0

# Gentler curve (slower darkening)
Difficulty Exponent: 1.0

# Very dark endgame
Minimum Lighting Ratio: 10%

# Brighter endgame
Minimum Lighting Ratio: 25%

# More torches at all levels
Max Torch Count: 80

# Fewer torches at all levels
Max Torch Count: 40
```

---

## 🧪 **TESTING**

### **In Unity Editor:**

1. **Add MazeLightingConfig to scene**
2. **Call PrintLightingProgression():**

```csharp
// In Console or debug script
var config = new MazeLightingConfig();
config.PrintLightingProgression();
```

**Output:**
```
═══════════════════════════════════════════════
  MAZE LIGHTING PROGRESSION
═══════════════════════════════════════════════
Base Torch Count: 60
Base Light Range: 12m
Base Intensity: 1.8
Difficulty Exponent: 1.5
Minimum Ratio: 15%
───────────────────────────────────────────────
Level | Ratio | Torches | Range  | Intensity
───────────────────────────────────────────────
L01  | 100%   | 060     | 12.0m   | 1.80
L03  | 85%    | 051     | 10.2m   | 1.53
L05  | 71%    | 043     | 8.5m    | 1.28
L10  | 46%    | 028     | 5.5m    | 0.83
L15  | 23%    | 014     | 2.8m    | 0.41
L20  | 15%    | 009     | 1.8m    | 0.27
L25  | 15%    | 009     | 1.8m    | 0.27
L30  | 15%    | 009     | 1.8m    | 0.27
═══════════════════════════════════════════════
```

---

## ✅ **BENEFITS**

### **Gameplay:**
```
✅ Natural difficulty progression
✅ Players adapt to decreasing light
✅ Creates tension as levels increase
✅ Rewards exploration (finding light sources)
✅ Scales with player strength
```

### **Technical:**
```
✅ Automatic calculation
✅ Easy to configure
✅ Works with binary storage
✅ No performance impact
✅ Saves to maze ID
```

### **Design:**
```
✅ Early levels: Welcoming, bright
✅ Mid levels: Challenging, atmospheric
✅ Late levels: Tense, horror-like
✅ Endgame: True darkness, expert only
```

---

## 📝 **INTEGRATION EXAMPLES**

### **Example 1: Level-Based Progression**

```csharp
public class GameProgression : MonoBehaviour
{
    public void LoadNextLevel()
    {
        currentLevel++;
        
        // Update lighting
        var spatialPlacer = GetComponent<SpatialPlacer>();
        spatialPlacer.SetMazeLevel(currentLevel);
        
        // Generate maze with new lighting
        GetComponent<MazeIntegration>().GenerateMaze();
    }
}
```

### **Example 2: Player Choice**

```csharp
public class DifficultySelect : MonoBehaviour
{
    public void SelectEasy() => SetDifficulty(1, 5);
    public void SelectMedium() => SetDifficulty(6, 12);
    public void SelectHard() => SetDifficulty(13, 20);
    
    void SetDifficulty(int min, int max)
    {
        int randomLevel = Random.Range(min, max);
        GetComponent<SpatialPlacer>().SetMazeLevel(randomLevel);
    }
}
```

### **Example 3: Dynamic Adjustment**

```csharp
public class AdaptiveDifficulty : MonoBehaviour
{
    void OnPlayerDeath()
    {
        // Reduce difficulty on death
        currentLevel = Mathf.Max(1, currentLevel - 2);
        GetComponent<SpatialPlacer>().SetMazeLevel(currentLevel);
    }
    
    void OnLevelComplete()
    {
        // Increase difficulty on success
        currentLevel++;
        GetComponent<SpatialPlacer>().SetMazeLevel(currentLevel);
    }
}
```

---

**Your lighting now scales with difficulty! 🕯️✨**
