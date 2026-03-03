$c = Get-Content 'D:\travaux_Unity\PeuImporte\Assets\Scripts\Core\PlayerController.cs' -Raw
$o = ($c.ToCharArray() | Where-Object {$_ -eq '{'}).Count
$cl = ($c.ToCharArray() | Where-Object {$_ -eq '}'}).Count
Write-Host "Open braces: $o"
Write-Host "Close braces: $cl"
Write-Host "Difference: $($o - $cl)"
