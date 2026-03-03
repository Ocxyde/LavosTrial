# Simple Pixel Art Door Preview Generator

$tempDir = "D:\travaux_Unity\PeuImporte\diff_tmp\door_previews"
if (-not (Test-Path $tempDir)) {
    New-Item -ItemType Directory -Path $tempDir | Out-Null
}

Add-Type -AssemblyName System.Drawing

# 8-bit color palette
$cBlack = [System.Drawing.Color]::FromArgb(0, 0, 0)
$cBrown = [System.Drawing.Color]::FromArgb(139, 90, 43)
$cLightBrown = [System.Drawing.Color]::FromArgb(181, 137, 84)
$cDarkBrown = [System.Drawing.Color]::FromArgb(101, 67, 33)
$cGray = [System.Drawing.Color]::FromArgb(128, 128, 128)
$cLightGray = [System.Drawing.Color]::FromArgb(192, 192, 192)
$cGold = [System.Drawing.Color]::FromArgb(255, 215, 0)
$cWhite = [System.Drawing.Color]::FromArgb(255, 255, 255)

Write-Host "Creating door sprite sheet..." -ForegroundColor Cyan

# Create sprite sheet: 5 frames x 64x96 pixels each
$sheet = New-Object System.Drawing.Bitmap(320, 96)
$g = [System.Drawing.Graphics]::FromImage($sheet)
$g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))

# Draw 5 animation frames
for ($f = 0; $f -lt 5; $f++) {
    $x = $f * 64
    $angle = $f * 22.5
    $offset = [int](56 * ($angle / 90.0))
    
    # Door frame
    $framePen = New-Object System.Drawing.Pen($cGray, 2)
    $g.DrawRectangle($framePen, $x, 0, 63, 95)
    
    # Door panel (wood)
    $doorBrush = New-Object System.Drawing.SolidBrush($cBrown)
    $g.FillRectangle($doorBrush, $x + 4 + $offset, 4, 56 - $offset, 88)
    
    # Wood planks
    $plankPen = New-Object System.Drawing.Pen($cDarkBrown, 1)
    for ($i = 1; $i -lt 5; $i++) {
        $py = 4 + ($i * 22)
        $g.DrawLine($plankPen, $x + 4 + $offset, $py, $x + 60 - $offset, $py)
    }
    
    # Door handle (gold)
    $handleBrush = New-Object System.Drawing.SolidBrush($cGold)
    $g.FillEllipse($handleBrush, $x + 52 - $offset, 46, 8, 8)
    
    # Highlight edge
    $highlightPen = New-Object System.Drawing.Pen($cLightBrown, 1)
    $g.DrawLine($highlightPen, $x + 6 + $offset, 4, $x + 6 + $offset, 92)
    
    Write-Host "  Frame $f : ${angle}°" -ForegroundColor Gray
}

# Save sprite sheet
$spritePath = "$tempDir\door_sprite_sheet.png"
$sheet.Save($spritePath, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host "`nSaved: $spritePath" -ForegroundColor Green

# Create individual frames
Write-Host "`nCreating individual frames..." -ForegroundColor Cyan
for ($f = 0; $f -lt 5; $f++) {
    $frame = New-Object System.Drawing.Bitmap(64, 96)
    $fg = [System.Drawing.Graphics]::FromImage($frame)
    $fg.Clear([System.Drawing.Color]::Transparent)
    
    $srcX = $f * 64
    $srcRect = New-Object System.Drawing.Rectangle($srcX, 0, 64, 96)
    $fg.DrawImage($sheet, 0, 0, $srcRect, [System.Drawing.GraphicsUnit]::Pixel)
    
    $framePath = "$tempDir\door_frame_$f.png"
    $frame.Save($framePath, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Host "  $framePath" -ForegroundColor Gray
}

# Create color palette
Write-Host "`nCreating color palette..." -ForegroundColor Cyan
$pal = New-Object System.Drawing.Bitmap(240, 80)
$pg = [System.Drawing.Graphics]::FromImage($pal)
$pg.Clear([System.Drawing.Color]::FromArgb(30, 30, 30))

$colors = @($cBlack, $cDarkBrown, $cBrown, $cLightBrown, $cGray, $cLightGray, $cGold, $cWhite)
$names = @("Black", "Dark Brown", "Brown", "Light Brown", "Gray", "Light Gray", "Gold", "White")

for ($i = 0; $i -lt $colors.Length; $i++) {
    $swatchBrush = New-Object System.Drawing.SolidBrush($colors[$i])
    $pg.FillRectangle($swatchBrush, ($i * 30) + 2, 5, 26, 50)
    
    $font = New-Object System.Drawing.Font("Consolas", 6)
    $labelBrush = New-Object System.Drawing.SolidBrush($cWhite)
    $pg.DrawString($names[$i], $font, $labelBrush, ($i * 30) + 2, 58)
}

$palPath = "$tempDir\color_palette.png"
$pal.Save($palPath, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host "  $palPath" -ForegroundColor Green

Write-Host "`nDone! Files saved to: $tempDir" -ForegroundColor Cyan
