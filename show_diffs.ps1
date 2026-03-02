# show_diffs.ps1
# Show diffs for changed files
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# Usage: .\show_diffs.ps1

Write-Host "=============================" -ForegroundColor Cyan
Write-Host "📋 DIFFS FOR CHANGED FILES" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host ""

# Diff 1: ItemData.cs
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "FILE: Assets/Scripts/Core/ItemData.cs" -ForegroundColor Yellow
Write-Host "CHANGE: Fix reflection usage - use direct component references" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host ""
Write-Host "--- a/Assets/Scripts/Core/ItemData.cs" -ForegroundColor Gray
Write-Host "+++ b/Assets/Scripts/Core/ItemData.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "@@ -53,25 +53,22 @@" -ForegroundColor Cyan
Write-Host "         public virtual void OnUse(GameObject user)" -ForegroundColor Green
Write-Host "         {" -ForegroundColor Green
Write-Host "             Debug.Log($`"[Item] Using {itemName}`");" -ForegroundColor Green
Write-Host "" -ForegroundColor Green
Write-Host "             if (itemType != InventoryItemType.Consumable) return;" -ForegroundColor Green
Write-Host "" -ForegroundColor Green
Write-Host "-            // Use dynamic lookup to avoid assembly dependency" -ForegroundColor Red
Write-Host "-            var stats = user.GetComponent(`"PlayerStats`") as MonoBehaviour;" -ForegroundColor Red
Write-Host "-            if (stats == null) return;" -ForegroundColor Red
Write-Host "-" -ForegroundColor Red
Write-Host "-            // Use reflection to call heal/restore methods" -ForegroundColor Red
Write-Host "+            // Use direct component reference for better performance" -ForegroundColor Green
Write-Host "+            var playerStats = user.GetComponent<PlayerStats>();" -ForegroundColor Green
Write-Host "+            if (playerStats == null) return;" -ForegroundColor Green
Write-Host "+" -ForegroundColor Green
Write-Host "+            // Direct method calls - no reflection overhead" -ForegroundColor Green
Write-Host "             if (healthRestore > 0f)" -ForegroundColor Green
Write-Host "             {" -ForegroundColor Green
Write-Host "-                var healMethod = stats.GetType().GetMethod(`"Heal`");" -ForegroundColor Red
Write-Host "-                healMethod?.Invoke(stats, new object[] { healthRestore });" -ForegroundColor Red
Write-Host "+                playerStats.Heal(healthRestore);" -ForegroundColor Green
Write-Host "             }" -ForegroundColor Green
Write-Host "             if (manaRestore > 0f)" -ForegroundColor Green
Write-Host "             {" -ForegroundColor Green
Write-Host "-                var restoreManaMethod = stats.GetType().GetMethod(`"RestoreMana`");" -ForegroundColor Red
Write-Host "-                restoreManaMethod?.Invoke(stats, new object[] { manaRestore });" -ForegroundColor Red
Write-Host "+                playerStats.RestoreMana(manaRestore);" -ForegroundColor Green
Write-Host "             }" -ForegroundColor Green
Write-Host "             if (staminaRestore > 0f)" -ForegroundColor Green
Write-Host "             {" -ForegroundColor Green
Write-Host "-                var restoreStaminaMethod = stats.GetType().GetMethod(`"RestoreStamina`");" -ForegroundColor Red
Write-Host "-                restoreStaminaMethod?.Invoke(stats, new object[] { staminaRestore });" -ForegroundColor Red
Write-Host "+                playerStats.RestoreStamina(staminaRestore);" -ForegroundColor Green
Write-Host "             }" -ForegroundColor Green
Write-Host "         }" -ForegroundColor Green
Write-Host ""

# Diff 2: AddDoorSystemToScene.cs
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "FILE: Assets/Scripts/Editor/AddDoorSystemToScene.cs" -ForegroundColor Yellow
Write-Host "CHANGE: Update to Unity 6 API - remove deprecated warning suppression" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host ""
Write-Host "--- a/Assets/Scripts/Editor/AddDoorSystemToScene.cs" -ForegroundColor Gray
Write-Host "+++ b/Assets/Scripts/Editor/AddDoorSystemToScene.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "@@ -1,25 +1,22 @@" -ForegroundColor Cyan
Write-Host " #if UNITY_EDITOR" -ForegroundColor Green
Write-Host " using UnityEngine;" -ForegroundColor Green
Write-Host " using UnityEditor;" -ForegroundColor Green
Write-Host " using System.Linq;" -ForegroundColor Green
Write-Host "" -ForegroundColor Green
Write-Host "-#pragma warning disable CS0618 // Disable warnings for deprecated Unity API" -ForegroundColor Red
Write-Host "-" -ForegroundColor Red
Write-Host " namespace Code.Lavos.Core" -ForegroundColor Green
Write-Host " {" -ForegroundColor Green
Write-Host "     public class AddDoorSystemToScene : MonoBehaviour" -ForegroundColor Green
Write-Host "     {" -ForegroundColor Green
Write-Host "         [MenuItem(`"Tools/PeuImporte/Add Door System to Maze`")]" -ForegroundColor Green
Write-Host "         public static void AddDoorSystemToMaze()" -ForegroundColor Green
Write-Host "         {" -ForegroundColor Green
Write-Host "-            // Find MazeGenerator in scene" -ForegroundColor Red
Write-Host "-            var mazeGenerators = FindObjectsOfType<MazeGenerator>();" -ForegroundColor Red
Write-Host "+            // Find MazeGenerator in scene using Unity 6 API" -ForegroundColor Green
Write-Host "+            var mazeGenerators = Object.FindObjectsByType<MazeGenerator>(FindObjectsSortMode.None);" -ForegroundColor Green
Write-Host ""

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Files changed: 2" -ForegroundColor White
Write-Host "  Lines added:   ~10" -ForegroundColor White
Write-Host "  Lines removed: ~15" -ForegroundColor White
Write-Host ""
Write-Host "  Benefits:" -ForegroundColor Green
Write-Host "    ✓ Better performance (no reflection overhead)" -ForegroundColor Green
Write-Host "    ✓ Type safety (compile-time checking)" -ForegroundColor Green
Write-Host "    ✓ Unity 6 API compliance" -ForegroundColor Green
Write-Host "    ✓ Cleaner, more maintainable code" -ForegroundColor Green
Write-Host ""
