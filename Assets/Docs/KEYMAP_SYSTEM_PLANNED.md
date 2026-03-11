# Keymap System - Planned Feature

**Date:** 2026-03-11  
**Status:** ⏳ BACKLOG (Planned for future implementation)  
**Priority:** Medium  

---

## 📋 **OVERVIEW**

Implement a comprehensive key mapping system with configuration files for keyboard, gamepad, and general game settings.

---

## 🎮 **CONFIGURATION FILES**

### **1. Keymap.cfg** (Keyboard Bindings)

```ini
# Keymap.cfg - Keyboard Input Bindings
# Format: Action=KeyCode

# Movement
MoveForward=W
MoveBackward=S
MoveLeft=A
MoveRight=D

# Actions
Interact=F
Sprint=LeftShift
Jump=Space

# Camera
LookMouse=MouseDelta

# System
Pause=Escape
Console=BackQuote
```

### **2. Gamepad.cfg** (Controller Bindings)

```ini
# Gamepad.cfg - Gamepad Input Bindings
# Format: Action=GamepadBinding

# Movement (Left Stick)
MoveLeftStick=Axis0,Axis1

# Actions
Interact=Button0
Sprint=Button1
Jump=Button2

# Camera (Right Stick)
LookRightStick=Axis2,Axis3
```

### **3. Game.cfg** (General Game Settings)

```ini
# Game.cfg - General Game Configuration
# Format: Setting=Value

# Display
Fullscreen=true
Resolution=1920x1080
VSync=true

# Gameplay
MouseSensitivity=2.0
LookInvertY=false
BobbleheadEnabled=true

# Audio
MasterVolume=1.0
MusicVolume=0.7
SFXVolume=0.8
```

---

## 🕹️ **DODGE MECHANIC**

### **Planned Implementation:**

**Controls:**
- **A (AZERTY) / Q (QWERTY):** Slide/strafe left
- **E:** Slide/strafe right
- **Cost:** Stamina points
- **Animation:** Quick lateral dodge roll

**Purpose:**
- Evade enemy attacks
- Quick repositioning in combat
- Add skill-based movement

**AZERTY Consideration:**
```
AZERTY Keyboard (French):
A = Left strafe (where QWERTY has A)
E = Right strafe (same as QWERTY)

QWERTY Keyboard (English):
A = Left strafe
E = Right strafe
```

---

## 🔧 **IMPLEMENTATION PLAN**

### **Phase 1: Configuration System**

1. **Create Config Loader**
   ```csharp
   public class ConfigLoader
   {
       public static KeymapConfig LoadKeymap(string path);
       public static GamepadConfig LoadGamepad(string path);
       public static GameConfig LoadGame(string path);
   }
   ```

2. **Default Config Files**
   - Create `Config/Keymap-default.cfg`
   - Create `Config/Gamepad-default.cfg`
   - Create `Config/Game-default.cfg`

3. **User Overrides**
   - Load defaults first
   - Apply user overrides from `Config/` folder
   - Save user changes on exit

### **Phase 2: Input System Integration**

1. **Update PlayerController**
   ```csharp
   // Instead of hardcoded keys:
   if (_kb.fKey.wasPressedThisFrame) { ... }
   
   // Use config:
   if (_kb[config.InteractKey].wasPressedThisFrame) { ... }
   ```

2. **Update Input Actions**
   - Migrate to Unity's Input System package
   - Support both keyboard and gamepad
   - Allow runtime rebinding

### **Phase 3: Dodge Mechanic**

1. **Player Movement**
   ```csharp
   private void HandleDodge()
   {
       if (_kb[config.DodgeLeftKey].wasPressedThisFrame)
           Dodge(Vector3.left);
       
       if (_kb[config.DodgeRightKey].wasPressedThisFrame)
           Dodge(Vector3.right);
   }
   
   private void Dodge(Vector3 direction)
   {
       if (stats.UseStamina(config.DodgeCost))
       {
           // Quick lateral movement
           characterController.Move(direction * dodgeDistance);
       }
   }
   ```

2. **Stamina Cost**
   - Dodge left: 10 stamina
   - Dodge right: 10 stamina
   - Cooldown: 0.5 seconds

---

## 📊 **FILE STRUCTURE**

```
Assets/
├── Config/
│   ├── Keymap-default.cfg    # Default keyboard bindings
│   ├── Keymap.cfg            # User keyboard bindings (gitignored)
│   ├── Gamepad-default.cfg   # Default gamepad bindings
│   ├── Gamepad.cfg           # User gamepad bindings (gitignored)
│   ├── Game-default.cfg      # Default game settings
│   └── Game.cfg              # User game settings (gitignored)
├── Scripts/
│   └── Core/
│       └── 00_Config/
│           ├── ConfigLoader.cs
│           ├── KeymapConfig.cs
│           ├── GamepadConfig.cs
│           └── GameConfig.cs
```

---

## 🎯 **BENEFITS**

| Feature | Benefit |
|---------|---------|
| **Configurable Keys** | Accessibility, player preference |
| **Gamepad Support** | Console-style gameplay option |
| **Dodge Mechanic** | Skill-based combat, evasion |
| **AZERTY Support** | French keyboard compatibility |
| **Separate Configs** | Easy to update defaults, preserve user settings |

---

## 📝 **NOTES**

- **Current State:** Hardcoded keys (F for interact, Shift for sprint)
- **Future State:** Fully configurable via .cfg files
- **Priority:** Medium (not critical for gameplay, but improves UX)
- **Estimated Time:** 6-8 hours total

---

**Last Updated:** 2026-03-11  
**Status:** ⏳ BACKLOG  
**Next Session:** Review priority after core gameplay testing

*Happy coding, coder friend!*
