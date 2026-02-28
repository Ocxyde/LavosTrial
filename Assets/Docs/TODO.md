# TODO List - Game Features

## ðŸ”´ High Priority

### Player Stats (New Values)
- [ ] **Player Stats Update**
  - [ ] HP: 1000 (max)
  - [ ] ManaPoint: 100 (max)
  - [ ] StaminaPoint: 100 (max)
  - [ ] Sprint cost: 1% of current SP per sprint tick

### UI Bars System (Screen-Space Responsive)
- [ ] **Bar Layout Configuration**
  - [ ] Detect screen resolution at runtime
  - [ ] Bars positioned at 75% of screen width/height from center
  - [ ] Bars anchored to middle-center of screen

- [ ] **HealthBar (Left Border)**
  - [ ] Position: Left edge of screen, vertical orientation
  - [ ] Full height: 75% of screen height
  - [ ] Centered vertically on screen
  - [ ] Red color scheme

- [ ] **ManaBar (Right Border)**
  - [ ] Position: Right edge of screen, vertical orientation
  - [ ] Full height: 75% of screen height
  - [ ] Centered vertically on screen
  - [ ] Blue color scheme

- [ ] **StaminaBar (Bottom Border)**
  - [ ] Position: Bottom edge of screen, horizontal orientation
  - [ ] Full width: 75% of screen width
  - [ ] Centered horizontally on screen
  - [ ] Yellow/Green color scheme

- [ ] **Status Effects (Top Center)**
  - [ ] Position: Top center of screen
  - [ ] Horizontal layout for multiple effects
  - [ ] Icon + duration display

### 1. Inventory System
- [x] Create inventory data structure (items, quantities, slots)
- [x] Inventory UI (slots, icons, drag & drop)
- [x] Item pickup system (collectibles)
- [x] Item usage (consumables, equipment)
- [ ] Inventory persistence between scenes

### 2. Player Status System
- [x] **Health System**
  - [x] Max health management (existing PlayerHealth)
  - [x] Damage calculation (existing PlayerHealth)
  - [x] Healing system (existing PlayerHealth)
  - [x] Health bar UI (new PlayerHUD)

- [ ] **Mana System**
  - [x] Max mana management (PlayerStats)
  - [x] Mana consumption for abilities (PlayerStats)
  - [x] Mana regeneration (PlayerStats)
  - [x] Mana bar UI (PlayerHUD)

- [x] **Stamina System**
  - [x] Max stamina management (existing PlayerController)
  - [x] Sprint system (existing PlayerController)
  - [x] Stamina regeneration (existing PlayerController)
  - [x] Stamina bar UI (PlayerHUD)

- [x] **Status Effects**
  - [x] Status effect base class (PlayerStats)
  - [x] Positive effects (buffs): speed boost, damage boost, regeneration
  - [x] Negative effects (debuffs): poison, slow, stun
  - [x] Effect duration management
  - [x] Effect stacking rules
  - [x] Visual indicators for active effects (icons)

### 3. Player HUD UI
- [x] **Health Bar**
  - [x] Health bar prefab
  - [x] Current/Max health display
  - [x] Damage flash effect
  - [x] Regeneration indicator

- [x] **Mana Bar**
  - [x] Mana bar prefab
  - [x] Current/Max mana display
  - [x] Low mana warning
  - [x] Regeneration indicator

- [x] **Stamina Bar**
  - [x] Stamina bar prefab
  - [x] Current/Max stamina display
  - [x] Sprint indicator
  - [x] Regeneration indicator

- [x] **Status Effects Icons**
  - [x] Status effect icon prefab
  - [x] Active effects display (top-right corner)
  - [x] Effect duration timer
  - [x] Effect stack count indicator
  - [x] Buff icons (green)
  - [x] Debuff icons (red)

### 4. UIManager (Centralized UI)
- [x] **Health Bar integration**
- [x] **Mana Bar integration**
- [x] **Stamina Bar integration**
- [x] **Score display**
- [x] **Crosshair**
- [x] **Pause menu**
- [x] **Game Over panel**
- [x] **Victory panel**
- [ ] **Status Effects display integration**

### 5. Chest Interactive System
- [ ] Chest prefab with open/close animation
- [ ] Interaction detection (player proximity)
- [ ] Keyboard input 'E' to open chest
- [ ] Random item generation inside chest
- [ ] Loot distribution to player inventory
- [ ] Chest respawn timer (optional)
- [ ] Audio feedback (open/close sounds)

### 6. Double-Sided Door at Maze Exit
- [ ] Door prefab (double door design)
- [ ] Open animation (swing inward/outward)
- [ ] Player detection trigger
- [ ] Victory condition when door is opened
- [ ] Victory UI screen
- [ ] Optional: Key requirement to open door

### 7. UI Elements On-Screen
- [ ] **Interaction Prompt**
  - [x] "Press E to interact" when near interactable objects
  - [x] Context-sensitive text (e.g., "Press E to open chest", "Press E to pick up")
  
- [ ] **Item Pickup Notification**
  - [ ] Toast notification when item is collected
  - [ ] Item name and icon display
  - [ ] Auto-dismiss after few seconds

- [ ] **Damage Numbers**
  - [ ] Floating damage text on hit
  - [ ] Different colors for damage vs healing
  - [ ] Animation (float up and fade)

- [ ] **Tooltip System**
  - [ ] Item tooltip on hover (inventory)
  - [ ] Enemy stats tooltip
  - [ ] Equipment stat comparison

- [ ] **Mini-Map** (optional)
  - [ ] Player position indicator
  - [ ] Objective marker
  - [ ] Explored vs unexplored areas

- [ ] **Quest Tracker**
  - [ ] Active quest display
  - [ ] Objective progress
  - [ ] Quest completion notification

- [ ] **Loading Screen**
  - [ ] Loading indicator
  - [ ] Tips during loading
  - [ ] Progress bar for scene loads

### 8. Interaction System (New Input System)
- [x] **IInteractable interface** - Define interactable objects
- [x] **PlayerController integration** - Handle 'E' key for interaction
- [x] **Raycast-based detection** - Detect interactable objects in range
- [x] **Interaction prompt UI** - Show context-sensitive text
- [ ] **Interaction feedback** - Visual/audio feedback on interact
- [ ] **Compatible interactables** - Chest, doors, items, NPCs

## ðŸŸ¡ Medium Priority

### 7. Player Animation System
- [ ] Idle animation
- [ ] Walk/Run animation
- [ ] Jump animation
- [ ] Attack animation (if combat implemented)

### 8. Sound System
- [ ] Background music
- [ ] Footstep sounds
- [ ] UI sounds
- [ ] Ambient sounds

### 9. Save/Load System
- [ ] Save player progress
- [ ] Save inventory
- [ ] Save maze state
- [ ] Load saved game

## ðŸŸ¢ Low Priority

### 10. Polish
- [ ] Main menu
- [ ] Pause menu
- [ ] Settings (graphics, audio)
- [ ] Level selection
- [ ] Achievement system

---

## Implementation Notes

### Inventory System Design
```
- Item class: id, name, icon, type, value, description
- Inventory class: slots[], capacity, addItem(), removeItem()
- Item types: consumable, equipment, quest, key
```

### Status Effect Design
```
- Effect class: type, duration, intensity, stacks
- PlayerStatus class: applyEffect(), removeEffect(), update()
- Effect types: poison, burn, freeze, haste, shield, etc.
```

### Chest Design
```
- Chest component: isOpen, items[], respawnTime
- Interaction: OnTriggerEnter/Exit for player detection
- Input: Check for 'E' key press while in range
- Reward: Random item selection from pool
```

### Door Design
```
- Door component: isLocked, isOpen, requiredKey
- Animation: Rotate door panels on open
- Trigger: Detect player approaching
- Event: Trigger victory when fully opened
```

---

## Estimated Timeline
- **Week 1**: Inventory System
- **Week 2**: Player Status (Health/Mana/Stamina/Effects)
- **Week 3**: Chest System + Door System
- **Week 4**: Polish & Testing
