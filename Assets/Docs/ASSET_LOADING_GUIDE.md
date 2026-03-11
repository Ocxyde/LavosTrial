# Asset Loading Guide (Cardinal-Only Maze, LavosTrial)

Purpose: document current loading paths, proposed AsyncLoad wrapper, and object pooling approach. Toggle via config.

Background
- Unity 6 era; aim to improve asset loading without breaking existing behavior.
- Non-destructive: introduce scaffolds, not replace existing loads yet.

Current loading patterns
- Synchronous loads on main thread for textures, prefabs, and scene assets.
- JSON-driven config loaded from Config/GameConfig-default.json.

Proposed async via AsyncLoad wrapper
- Introduce AsyncLoad wrapper (toggle with enableAssetAsyncLoading).
- Wrap selected heavy loads behind AsyncLoad; keep existing paths as fallback.
- If enableAssetAsyncLoading is false, behavior remains identical.

Object pooling proposal
- Introduce a minimal ObjectPooler for walls, chests, and enemies.
- Enable via config flag (enableObjectPooling) in a future patch.
- Default state remains instantiate/destroy to avoid gameplay changes.

Config and defaults
- Add a boolean flag enableAssetAsyncLoading to Config/GameConfig-default.json.
- Optional: add enableObjectPooling flag (false by default) for future steps.

Verification steps
- Build in Unity with enableAssetAsyncLoading=false; confirm no regressions.
- When enabling, compare load timings and memory footprint.
- Ensure no runtime exceptions during maze generation.

Notes
- This guide is a living document; update as you iterate on loading changes.
