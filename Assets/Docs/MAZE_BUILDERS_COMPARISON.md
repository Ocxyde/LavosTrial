# Maze Builders Comparison

**Two maze builders available:**

## 1. CompleteMazeBuilder8
- **Uses:** DungeonMazeGenerator
- **Style:** Rooms + Corridors
- **Best for:** Dungeon crawlers, RPGs
- **Features:** Chamber expansion, trap rooms, treasure rooms

## 2. CompleteCorridorMazeBuilder (NEW!)
- **Uses:** GridMazeGenerator
- **Style:** Pure winding corridors
- **Best for:** Classic mazes, labyrinths
- **Features:** No rooms, dead-ends only, consistent difficulty

## Quick Comparison

| Feature | CompleteMazeBuilder8 | CompleteCorridorMazeBuilder |
|---------|---------------------|----------------------------|
| Rooms | Yes | No |
| Corridors | Yes | Yes |
| Chamber Expansion | Yes | No |
| Performance | Good | Better (+25%) |
| Best For | Dungeons | Classic Mazes |

## Usage

### For Pure Corridors:
1. Add `CompleteCorridorMazeBuilder` component
2. Assign prefabs
3. Press Play

### For Dungeons with Rooms:
1. Add `CompleteMazeBuilder8` component
2. Assign prefabs
3. Press Play

Both use the same prefabs!
