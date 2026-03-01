========================================
Unity6.LavosTrial - HUD System Patch
========================================

OVERVIEW
--------
This patch updates the HUD system to Unity6 conventions:
- Namespace: Unity6.LavosTrial.HUD
- New Input System with runtime detection + fallback
- Keyboard mapping via JSON config
- Multi-platform build scripts

FILES INCLUDED
--------------
1. Assets/Scripts/HUD/HUDSystem.cs (patched)
   - Namespace: Unity6.LavosTrial.HUD
   - Input System detection at runtime
   - JSON-based keyboard mapping

2. Assets/Input/Unity6.LavosTrial.InputMap.json
   - Default keyboard mapping (US QWERTY)
   - Fallback AZERTY support
   - Extensible via JSON

3. BuildAll.bat / BuildAll.ps1
   - Multi-platform build scripts (Windows/macOS/Linux)
   - Build summary output

4. README.txt (this file)

PATCH APPLICATION
-----------------
The patched HUDSystem.cs has been written directly to:
  Assets/Scripts/HUD/HUDSystem.cs

If you need to apply manually:
1. Backup your original HUDSystem.cs
2. Replace with the patched version
3. Ensure the namespace Unity6.LavosTrial.HUD is used

INPUT CONFIGURATION
-------------------
The input mapping is loaded from:
  Assets/Input/Unity6.LavosTrial.InputMap.json

Default controls (US QWERTY):
- MoveUp: W, UpArrow
- MoveDown: S, DownArrow
- MoveLeft: A, LeftArrow
- MoveRight: D, RightArrow
- Jump: Space
- Fire: LeftCtrl, Mouse0
- Aim: Mouse1
- Reload: R
- Interact: E
- Inventory: I
- Pause: Escape
- Sprint: LeftShift
- Crouch: C

To customize, edit the JSON file or create a new one.

BUILD INSTRUCTIONS
------------------
Using BuildAll.bat (Windows):
  1. Double-click BuildAll.bat
  2. Wait for builds to complete
  3. Check build_summary.log for results

Using BuildAll.ps1 (PowerShell):
  1. Open PowerShell
  2. Run: .\BuildAll.ps1
  3. Wait for builds to complete
  4. Check build_summary.log for results

Build outputs:
  - Build/Windows/LavosTrial.exe
  - Build/Mac/LavosTrial.app
  - Build/Linux/LavosTrial

REQUIREMENTS
------------
- Unity 6 LTS (6000.3.7f1 or later)
- Unity Input System package (optional, for New Input System)
- TextMeshPro package

NOTES
-----
- backup.ps1 was NOT modified (as requested)
- All scripts use Unix line endings (LF) and UTF-8 encoding
- The New Input System is detected at runtime; falls back to legacy if not available
- Keyboard mapping supports both QWERTY and AZERTY layouts via JSON config

SUPPORT
-------
For issues, check:
- build_summary.log for build results
- build_log_<Platform>.log for detailed build logs
- Unity Console for runtime errors
