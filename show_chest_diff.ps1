# show_chest_diff.ps1
# Show diff for enhanced chest pixel art
# Unity 6 compatible - UTF-8 encoding - Unix line endings

Write-Host "=============================" -ForegroundColor Cyan
Write-Host "📦 CHEST PIXEL ART ENHANCEMENT" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "FILE: Assets/Scripts/Core/ChestBehavior.cs" -ForegroundColor Yellow
Write-Host "CHANGE: Enhanced 8-bit pixel art texture with cool cube design" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host ""

Write-Host "🎨 NEW 8-BIT COLOR PALETTE:" -ForegroundColor Green
Write-Host "  • woodDark:    (52, 28, 12)   - Dark brown" -ForegroundColor White
Write-Host "  • woodMid:     (88, 50, 20)   - Medium brown" -ForegroundColor White
Write-Host "  • woodLight:   (120, 75, 35)  - Light brown" -ForegroundColor White
Write-Host "  • goldBright:  (255, 220, 60) - Bright gold trim" -ForegroundColor White
Write-Host "  • goldDark:    (200, 160, 40) - Dark gold accents" -ForegroundColor White
Write-Host "  • iron:        (70, 75, 85)   - Steel bands" -ForegroundColor White
Write-Host "  • ironHighlight: (120, 130, 145) - Metal highlights" -ForegroundColor White
Write-Host "  • gemRed:      (220, 40, 40)  - Ruby gem" -ForegroundColor White
Write-Host "  • gemGlow:     (255, 100, 100) - Gem aura" -ForegroundColor White
Write-Host ""

Write-Host "✨ NEW FEATURES:" -ForegroundColor Green
Write-Host "  ✓ Lid section with curved top design" -ForegroundColor White
Write-Host "  ✓ Gold trim border with checkered pattern" -ForegroundColor White
Write-Host "  ✓ Decorative gold patterns on lid" -ForegroundColor White
Write-Host "  ✓ Center ruby gem with glow effect" -ForegroundColor White
Write-Host "  ✓ Body with horizontal wood planks" -ForegroundColor White
Write-Host "  ✓ Metal bands with rivet details" -ForegroundColor White
Write-Host "  ✓ Vertical metal straps on sides" -ForegroundColor White
Write-Host "  ✓ Ornate lock plate with keyhole" -ForegroundColor White
Write-Host "  ✓ Gold studs around lock" -ForegroundColor White
Write-Host "  ✓ 3D edge highlighting" -ForegroundColor White
Write-Host ""

Write-Host "📋 DIFF SUMMARY:" -ForegroundColor Cyan
Write-Host "--- a/Assets/Scripts/Core/ChestBehavior.cs" -ForegroundColor Gray
Write-Host "+++ b/Assets/Scripts/Core/ChestBehavior.cs" -ForegroundColor Gray
Write-Host ""
Write-Host "@@ -115,40 +115,120 @@" -ForegroundColor Cyan
Write-Host "     private Texture2D GenerateChestTexture()" -ForegroundColor Green
Write-Host "     {" -ForegroundColor Green
Write-Host "-        // Basic wood grain pattern" -ForegroundColor Red
Write-Host "-        var woodDark = new Color32(60, 40, 20, 255);" -ForegroundColor Red
Write-Host "-        var woodMid = new Color32(80, 55, 30, 255);" -ForegroundColor Red
Write-Host "-        var woodLight = new Color32(100, 70, 40, 255);" -ForegroundColor Red
Write-Host "-        var gold = new Color32(255, 215, 0, 255);" -ForegroundColor Red
Write-Host "-        var iron = new Color32(80, 85, 90, 255);" -ForegroundColor Red
Write-Host "+" -ForegroundColor Green
Write-Host "+        // 8-bit pixel art color palette (vibrant, classic style)" -ForegroundColor Green
Write-Host "+        var woodDark = new Color32(52, 28, 12, 255);" -ForegroundColor Green
Write-Host "+        var woodMid = new Color32(88, 50, 20, 255);" -ForegroundColor Green
Write-Host "+        var woodLight = new Color32(120, 75, 35, 255);" -ForegroundColor Green
Write-Host "+        var goldBright = new Color32(255, 220, 60, 255);" -ForegroundColor Green
Write-Host "+        var goldDark = new Color32(200, 160, 40, 255);" -ForegroundColor Green
Write-Host "+        var iron = new Color32(70, 75, 85, 255);" -ForegroundColor Green
Write-Host "+        var ironHighlight = new Color32(120, 130, 145, 255);" -ForegroundColor Green
Write-Host "+        var gemRed = new Color32(220, 40, 40, 255);" -ForegroundColor Green
Write-Host "+        var gemGlow = new Color32(255, 100, 100, 255);" -ForegroundColor Green
Write-Host "+" -ForegroundColor Green
Write-Host "+        // === LID SECTION (top 40%) ===" -ForegroundColor Green
Write-Host "+        // - Gold trim border" -ForegroundColor Green
Write-Host "+        // - Decorative patterns" -ForegroundColor Green
Write-Host "+        // - Center ruby gem" -ForegroundColor Green
Write-Host "+" -ForegroundColor Green
Write-Host "+        // === BODY SECTION (bottom 60%) ===" -ForegroundColor Green
Write-Host "+        // - Wood plank grain" -ForegroundColor Green
Write-Host "+        // - Metal bands with rivets" -ForegroundColor Green
Write-Host "+        // - Vertical straps" -ForegroundColor Green
Write-Host "+        // - Ornate lock plate" -ForegroundColor Green
Write-Host "+" -ForegroundColor Green
Write-Host "+        // 3D edge highlighting" -ForegroundColor Green
Write-Host ""
Write-Host "+    private static Color32 LightenColor(Color32 c, int amount) { ... }" -ForegroundColor Green
Write-Host "+    private static Color32 DarkenColor(Color32 c, int amount) { ... }" -ForegroundColor Green
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "VISUAL DESIGN (32x32 pixel grid)" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  ┌────────────────────────────┐" -ForegroundColor Yellow
Write-Host "  │ ══════════════════════════ │  ← Gold trim (checkered)" -ForegroundColor White
Write-Host "  │   ✚  DECORATIVE LID     ✚  │  ← Gold patterns" -ForegroundColor White
Write-Host "  │        ◆ RUBY ◆           │  ← Center gem with glow" -ForegroundColor White
Write-Host "  │══════════════════════════│" -ForegroundColor Yellow
Write-Host "  │▓▓▓ CHEST BODY ▓▓▓        │" -ForegroundColor White
Write-Host "  │▓▓▓ [LOCK] ▓▓▓            │  ← Ornate lock plate" -ForegroundColor White
Write-Host "  │▓▓▓   ⊕    ▓▓▓            │  ← Keyhole" -ForegroundColor White
Write-Host "  │▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓           │" -ForegroundColor White
Write-Host "  └────────────────────────────┘" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Legend:" -ForegroundColor Cyan
Write-Host "    ═ Gold trim       ✚ Gold studs" -ForegroundColor White
Write-Host "    ◆ Ruby gem        ▓ Wood planks" -ForegroundColor White
Write-Host "    [LOCK] Lock plate ⊕ Keyhole" -ForegroundColor White
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  File changed: 1" -ForegroundColor White
Write-Host "  Lines added:   ~120" -ForegroundColor White
Write-Host "  Lines removed: ~40" -ForegroundColor White
Write-Host ""
Write-Host "  Benefits:" -ForegroundColor Green
Write-Host "    ✓ Vibrant 8-bit pixel art style" -ForegroundColor Green
Write-Host "    ✓ Distinctive lid and body sections" -ForegroundColor Green
Write-Host "    ✓ Decorative elements (gem, trim, patterns)" -ForegroundColor Green
Write-Host "    ✓ 3D visual depth with edge highlighting" -ForegroundColor Green
Write-Host "    ✓ Classic treasure chest appearance" -ForegroundColor Green
Write-Host ""
