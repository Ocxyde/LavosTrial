# Holy Boss Room - Design Document

**Version:** 1.0
**Date:** 2026-03-05
**Status:** 📝 PLANNED (Not Implemented)

---

## 🏛️ **ROOM CONCEPT**

A special **holy/sacred boss room** that serves as the **final challenge** of the maze.

---

## 📐 **ROOM LAYOUT**

```
┌─────────────────────────────────────────┐
│         MAZE (Player comes from here)   │
├─────────────────────────────────────────┤
│           🚪🚪🚪                         │
│      ════════════════  ← Saloon Doors   │
│      (LOCK AFTER ENTRY - No retreat!)   │
│                                         │
│   ┌─────────────────────────────────┐   │
│   │                                 │   │
│   │     ⛧ HOLY BOSS ROOM ⛧          │   │
│   │                                 │   │
│   │   [CHEST]         💀 [BOSS]     │   │
│   │   (Loot)        (Final Challenge)│   │
│   │                                 │   │
│   │         🕯️ 🕯️ 🕯️ 🕯️             │   │
│   │      (Holy ambiance lighting)   │   │
│   │                                 │   │
│   └─────────────────────────────────┘   │
│                                         │
│        🚪      🚪      🚪               │
│     (LOCKED)(LOCKED)(LOCKED)            │
│     (Exit 1)(Exit 2)(Exit 3)            │
└─────────────────────────────────────────┘
              SOUTH (outside world)
```

---

## 🚪 **DOOR CONFIGURATION**

### **Entrance (North Wall - ONE-WAY):**
- **Type:** 3 Saloon-style double-sided doors
- **Behavior:** 
  - Open when player approaches (from maze side)
  - **LOCK PERMANENTLY after player enters** (NO EXIT!)
  - Visible from both sides
  - Swing both ways (push/pull)
  - **ONE-WAY ENTRANCE ONLY** (cannot exit back through entrance)

### **Exits (South Wall - 3 DOORS):**
- **Type:** 3 Simple single doors
- **Behavior:**
  - **LOCKED initially** (cannot open)
  - Unlock after boss defeat OR challenge completion
  - Manual operation (press E)
  - All 3 doors unlock simultaneously
  - **ONLY WAY OUT** (saloon entrance stays locked!)

---

## ⛧ **HOLY AMBiance**

### **Lighting:**
- **4 Candle holders** (corners of room)
- **Golden/yellow light** (holy atmosphere)
- **Dynamic flickering** (living flame)
- **God rays** from ceiling (divine light)

### **Visual Elements:**
- **Holy symbols** on walls (runes, sigils)
- **Altar** at far end (behind boss)
- **Stained glass** effect (if possible)
- **Particle effects** (holy dust, motes of light)

### **Audio:**
- **Choral ambient music** (holy choir)
- **Echo effect** (large sacred space)
- **Boss intro sound** (when player enters)
- **Door lock sound** (saloon doors close behind)

---

## 💀 **BOSS FIGHT**

### **Boss Placement:**
- **Position:** Center-back of room (facing entrance)
- **Behavior:** Awakens when player enters
- **Health:** High (final challenge)
- **Attacks:** Multiple patterns

### **Boss Types (Options):**
1. **Holy Knight** (armored, sword)
2. **Temple Guardian** (construct, magic)
3. **Fallen Priest** (magic, curses)
4. **Demon** (fire, darkness vs holy theme)

---

## 🎁 **CHEST LOOT**

### **Chest Placement:**
- **Position:** Side of room (not in direct line of sight)
- **Type:** Special/ornate chest (different from normal chests)
- **Lock:** Unlocks after boss defeat

### **Loot Contents:**
- **Legendary weapon** (holy sword, blessed staff)
- **Holy armor** (blessed protection)
- **Special item** (key, artifact, McGuffin)
- **Consumables** (holy potions, blessings)

---

## 🎮 **GAMEPLAY FLOW**

### **Phase 1: Entry**
```
1. Player approaches saloon doors (from maze)
2. Doors auto-open (player can enter)
3. Player steps into room
4. Saloon doors SLAM SHUT behind player
5. Lock sound effect (no retreat!)
6. Boss awakens (roar, animation)
7. Holy ambiance activates (lighting, music)
```

### **Phase 2: Boss Fight**
```
1. Boss engages player
2. Player fights boss
3. 3 exit doors remain locked (visible reminder)
4. Chest remains locked (temptation!)
5. Player must defeat boss (no shortcuts)
```

### **Phase 3: Victory**
```
1. Boss defeated (death animation)
2. Lock sound effect (exit doors unlock)
3. Chest unlocks (click sound)
4. Holy light intensifies (victory ambiance)
5. Player can:
   - Loot chest (reward)
   - Exit through any of 3 doors (freedom!)
```

---

## 🔧 **IMPLEMENTATION REQUIREMENTS**

### **Scripts Needed:**
- [ ] `BossRoomController.cs` (room state management)
- [ ] `BossAI.cs` (boss behavior)
- [ ] `HolyAmbiance.cs` (lighting, audio)
- [ ] `BossRoomDoor.cs` (saloon door lock mechanic)
- [ ] `BossRoomChest.cs` (special chest logic)

### **Prefabs Needed:**
- [ ] Boss prefab (with AI controller)
- [ ] Saloon door set (3 doors, linked)
- [ ] Holy candle holders (4x, with light)
- [ ] Ornate chest (special model)
- [ ] Altar (decorative)
- [ ] Holy symbols (wall decorations)

### **Audio Needed:**
- [ ] Boss intro roar
- [ ] Door lock/unlock sounds
- [ ] Holy ambient music
- [ ] Boss fight music
- [ ] Victory fanfare
- [ ] Chest open sound

### **Visual Effects:**
- [ ] God rays (ceiling light)
- [ ] Holy dust particles
- [ ] Boss death effect
- [ ] Door slam effect
- [ ] Unlock glow effect

---

## 📝 **IMPLEMENTATION PRIORITY**

### **Phase 1 (Core):**
- [ ] Room geometry (floor, ceiling, walls)
- [ ] 3 saloon doors (entrance)
- [ ] 3 simple doors (exit, locked)
- [ ] Boss spawn point
- [ ] Chest spawn point

### **Phase 2 (Functionality):**
- [ ] Saloon door lock mechanic
- [ ] Exit door unlock mechanic
- [ ] Boss AI (basic)
- [ ] Chest unlock mechanic

### **Phase 3 (Polish):**
- [ ] Holy ambiance (lighting)
- [ ] Audio (music, SFX)
- [ ] Visual effects (particles)
- [ ] Boss intro/outro animations

### **Phase 4 (Extra):**
- [ ] Multiple boss variants
- [ ] Multiple loot tables
- [ ] Secret alternate exit
- [ ] Achievement for completion

---

## 🎯 **PLAYER CHOICE**

The player has **ONE CHOICE**:

> **DEFEAT THE BOSS OR DIE TRYING**

No retreat, no shortcuts, no escape.

**Victory = Freedom + Loot**

**Defeat = Restart from checkpoint**

---

## 🤫 **SECRETS (Optional)**

### **Secret Alternate Exit:**
- Hidden behind altar
- Opens only if player finds clue in maze
- Shortcut to exit (skips boss?)
- Or leads to TRUE final boss?

### **Secret Boss Weakness:**
- Holy symbol on wall (shoot it?)
- Reduces boss health by 50%
- Easter egg for observant players

### **Secret Loot:**
- Hidden compartment in chest
- Extra rare item (1% chance)
- Only for lucky players

---

## 📊 **BALANCING**

### **Room Size:**
- **Width:** 2-3 cells (20-30m)
- **Depth:** 2-3 cells (20-30m)
- **Height:** Standard (4m)

### **Boss Difficulty:**
- **Health:** 3x normal enemy
- **Damage:** 2x normal enemy
- **Speed:** Equal to player
- **Attacks:** 3-4 patterns

### **Loot Quality:**
- **Weapon:** Legendary tier
- **Armor:** Epic tier
- **Consumables:** 3-5 items
- **Special:** 1 unique item

---

## 🎮 **TESTING CHECKLIST**

- [ ] Saloon doors open on approach
- [ ] Saloon doors lock after entry
- [ ] No retreat possible (doors locked)
- [ ] Boss awakens on entry
- [ ] Boss fights correctly
- [ ] Exit doors unlock on boss death
- [ ] Chest unlocks on boss death
- [ ] Loot is appropriate quality
- [ ] Holy ambiance works (lighting, audio)
- [ ] No exploits (can't skip boss)

---

**Design Document Created:** 2026-03-05
**Status:** 📝 PLANNED (Ready for Implementation)
**Priority:** 🔥 HIGH (Final content milestone)

---

*This is the FINAL CHALLENGE. Make it EPIC!* ⛧💀⚔️
