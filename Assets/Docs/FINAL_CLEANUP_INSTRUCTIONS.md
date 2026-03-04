# FINAL CLEANUP INSTRUCTIONS

**Date:** 2026-03-03  
**Goal:** Remove ALL singleton Instance properties and comments

---

## 🗑️ **MANUAL CLEANUP REQUIRED**

The automated edit failed due to encoding issues. Please manually remove these sections:

---

### **1. GameManager.cs (Line 19-27)**

**REMOVE THESE 9 LINES:**
```csharp
    /// <summary>
    /// Singleton instance - cached for performance.
    /// [OBSOLETE] Use EventHandler events instead of direct access.
    /// Deprecated: 2026-03-03
    /// </summary>
    [System.Obsolete("Use EventHandler events instead of direct singleton access. Deprecated: 2026-03-03")]
    public static GameManager Instance { get; private set; }
```

**KEEP JUST THE COMMENT:**
```csharp
    // ─── Singleton ─────────────────────────────────────────────────────────────
    // [REMOVED] Use EventHandler events instead
```

---

### **2. PlayerStats.cs (Line 23-43)**

**REMOVE THESE 21 LINES:**
```csharp
        // Cached singleton (performance optimization)
        private static PlayerStats _instance;
        private static bool _instanceChecked = false;
        
        /// <summary>
        /// [OBSOLETE] Use EventHandler events instead of direct singleton access.
        /// Deprecated: 2026-03-03
        /// </summary>
        [System.Obsolete("Use EventHandler events instead of direct singleton access. Deprecated: 2026-03-03")]
        public static PlayerStats Instance
        {
            get
            {
                if (_instance == null && !_instanceChecked)
                {
                    _instance = FindFirstObjectByType<PlayerStats>();
                    _instanceChecked = true;
                }
                return _instance;
            }
        }
```

**REPLACE WITH:**
```csharp
        // [REMOVED] Use EventHandler events instead
```

---

### **3. SeedManager.cs (Line 33-52)**

**REMOVE THESE 20 LINES:**
```csharp
        private static SeedManager _instance;
        private static bool _instanceChecked = false;
        
        /// <summary>
        /// [OBSOLETE] Use EventHandler events instead of direct singleton access.
        /// Deprecated: 2026-03-03
        /// </summary>
        [System.Obsolete("Use EventHandler events instead of direct singleton access. Deprecated: 2026-03-03")]
        public static SeedManager Instance
        {
            get
            {
                if (_instance == null && !_instanceChecked)
                {
                    _instance = FindFirstObjectByType<SeedManager>();
                    _instanceChecked = true;
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SeedManager");
                        _instance = go.AddComponent<SeedManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
```

**REPLACE WITH:**
```csharp
        // [REMOVED] Use EventHandler events instead
```

---

### **4. CombatSystem.cs (Line 26-48)**

**REMOVE THESE 23 LINES:**
```csharp
        // Cached singleton (performance optimization)
        private static CombatSystem _instance;
        private static bool _instanceChecked = false;
        
        /// <summary>
        /// [OBSOLETE] Use EventHandler events instead of direct singleton access.
        /// Deprecated: 2026-03-03
        /// </summary>
        [System.Obsolete("Use EventHandler events instead of direct singleton access. Deprecated: 2026-03-03")]
        public static CombatSystem Instance
        {
            get
            {
                if (_instance == null && !_instanceChecked)
                {
                    _instance = FindFirstObjectByType<CombatSystem>();
                    _instanceChecked = true;
                }
                return _instance;
            }
        }
```

**REPLACE WITH:**
```csharp
        // [REMOVED] Use EventHandler events instead
```

---

### **5. Inventory.cs (Line 13-35)**

**REMOVE THESE 23 LINES:**
```csharp
        // Cached singleton (performance optimization)
        private static Inventory _instance;
        private static bool _instanceChecked = false;
        
        /// <summary>
        /// [OBSOLETE] Use EventHandler events instead of direct singleton access.
        /// Deprecated: 2026-03-03
        /// </summary>
        [System.Obsolete("Use EventHandler events instead of direct singleton access. Deprecated: 2026-03-03")]
        public static Inventory Instance
        {
            get
            {
                if (_instance == null && !_instanceChecked)
                {
                    _instance = FindFirstObjectByType<Inventory>();
                    _instanceChecked = true;
                }
                return _instance;
            }
        }
```

**REPLACE WITH:**
```csharp
        // [REMOVED] Use EventHandler events instead
```

---

## 📊 **TOTAL SAVINGS**

| File | Lines Removed | Lines Added | Net Savings |
|------|---------------|-------------|-------------|
| `GameManager.cs` | 9 | 1 | **-8** |
| `PlayerStats.cs` | 21 | 1 | **-20** |
| `SeedManager.cs` | 20 | 1 | **-19** |
| `CombatSystem.cs` | 23 | 1 | **-22** |
| `Inventory.cs` | 23 | 1 | **-22** |
| **TOTAL** | **96** | **5** | **-91 LINES** |

---

## ✅ **AFTER CLEANUP**

### **Code Will Be:**
```
✅ 91 lines shorter
✅ No [Obsolete] attributes
✅ No compiler warnings
✅ Clean, minimal code
✅ Forces event-driven architecture
```

### **Developers Must Use:**
```csharp
// ✅ This works (event-driven)
EventHandler.Instance.OnGameStateChanged += Handler;

// ❌ This no longer compiles (Instance removed)
GameManager.Instance.CurrentState
```

---

## 🎯 **QUICK CLEANUP SCRIPT**

**In Rider/Visual Studio:**

1. **Find in Files:** `public static.*Instance.*{.*get;.*private set;.*}`
2. **Replace with:** `// [REMOVED] Use EventHandler events instead`
3. **Find:** `/// <summary>.*Singleton.*</summary>`
4. **Replace with:** (nothing - delete)
5. **Find:** `\[System.Obsolete.*\]`
6. **Replace with:** (nothing - delete)

**Time:** ~5 minutes  
**Result:** Clean, minimal code!

---

## 🎉 **FINAL RESULT**

**After manual cleanup:**
- ✅ **-91 lines** of code removed
- ✅ **0 compiler warnings**
- ✅ **100% event-driven**
- ✅ **Clean, minimal code**

---

**Please perform this manual cleanup in your IDE for best results!**

**Last Updated:** 2026-03-03  
**Estimated Time:** 5 minutes  
**Benefit:** -91 lines, cleaner code
