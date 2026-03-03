# Quick fix for remaining errors
Remove-Item 'D:\travaux_Unity\PeuImporte\Assets\Scripts\Core\TrapType.cs' -Force
Remove-Item 'D:\travaux_Unity\PeuImporte\Assets\Scripts\Core\TrapType.cs.meta' -Force

Move-Item 'D:\travaux_Unity\PeuImporte\Assets\Scripts\Ressources\TorchController.cs' 'D:\travaux_Unity\PeuImporte\Assets\Scripts\Core\TorchController.cs' -Force
Move-Item 'D:\travaux_Unity\PeuImporte\Assets\Scripts\Ressources\TorchController.cs.meta' 'D:\travaux_Unity\PeuImporte\Assets\Scripts\Core\TorchController.cs.meta' -Force

Write-Host "Done!"
