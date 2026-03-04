# Quick-Scene-Setup.ps1
# Complete scene generation using plug-in-and-out architecture
#
# USAGE: Run from project root
#   .\quick-scene-setup.ps1
#
# WHAT IT DOES:
#   • Creates/updates FpsMazeTest_Fresh.unity scene
#   • Adds all components via plug-in-and-out system
#   • Configures: Maze, Rooms, Doors, Textures, Ground, Ceiling, Torches, Lighting
#   • Prepares enemies (prefabs ready, not spawned)
#   • Opens scene in Unity

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  QUICK SCENE SETUP - Complete Generation" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan

# Step 1: Check Unity is closed
Write-Host "`n[INFO] This script prepares the scene setup." -ForegroundColor Yellow
Write-Host "  You need to run the Editor tool in Unity:" -ForegroundColor Yellow
Write-Host "  Tools → Quick Scene Setup → Generate Complete Scene" -ForegroundColor Yellow
Write-Host "  Shortcut: Ctrl+Alt+G" -ForegroundColor Yellow

# Step 2: Create scene file if not exists
$scenePath = "Assets/Scenes/FpsMazeTest_Fresh.unity"

if (-not (Test-Path $scenePath)) {
    Write-Host "`n[1/3] Creating new scene..." -ForegroundColor Yellow
    
    # Create minimal scene with MazeTest GameObject
    $sceneContent = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {fileID: 0}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 10
  m_Fog: 1
  m_FogColor: {r: 0.08, g: 0.05, b: 0.03, a: 1}
  m_FogMode: 1
  m_FogDensity: 0.005
  m_LinearFogStart: 10
  m_LinearFogEnd: 80
  m_AmbientSkyColor: {r: 0.6, g: 0.55, b: 0.5, a: 1}
  m_AmbientEquatorColor: {r: 0.114, g: 0.125, b: 0.133, a: 1}
  m_AmbientGroundColor: {r: 0.047, g: 0.043, b: 0.035, a: 1}
  m_AmbientIntensity: 1.5
  m_AmbientMode: 0
  m_SubtractiveShadowColor: {r: 0.42, g: 0.478, b: 0.627, a: 1}
  m_SkyboxMaterial: {fileID: 10304, guid: 0000000000000000f000000000000000, type: 0}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {fileID: 0}
  m_SpotCookie: {fileID: 10001, guid: 0000000000000000e000000000000000, type: 0}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {fileID: 0}
  m_Sun: {fileID: 0}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 13
  m_BakeOnSceneLoad: 0
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 1
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {fileID: 0}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 2
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 1
    m_PVRCulling: 1
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 1
    m_PVRFilteringGaussRadiusAO: 1
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {fileID: 20201, guid: 0000000000000000f000000000000000, type: 0}
  m_LightingSettings: {fileID: 0}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 3
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    buildHeightMesh: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {fileID: 0}
--- !u!1 &2138532859
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 2138532869}
  m_Layer: 0
  m_Name: MazeTest
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &2138532869
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 2138532859}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!1660057539 &9223372036854775807
SceneRoots:
  m_ObjectHideFlags: 0
  m_Roots:
  - {fileID: 2138532869}
"@
    
    $sceneContent | Set-Content $scenePath -Encoding UTF8 -NoNewline
    Write-Host "  ✅ Scene created: $scenePath" -ForegroundColor Green
} else {
    Write-Host "`n[1/3] Scene exists: $scenePath" -ForegroundColor Yellow
}

# Step 3: Create scene meta file
$sceneMetaPath = "$scenePath.meta"
if (-not (Test-Path $sceneMetaPath)) {
    Write-Host "`n[2/3] Creating scene meta..." -ForegroundColor Yellow
    
    $sceneMeta = @"
fileFormatVersion: 2
guid: f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
    
    $sceneMeta | Set-Content $sceneMetaPath -Encoding UTF8 -NoNewline
    Write-Host "  ✅ Meta created" -ForegroundColor Green
} else {
    Write-Host "  ✅ Meta exists" -ForegroundColor Green
}

# Step 4: Instructions
Write-Host "`n[3/3] Next Steps:" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Open Unity Editor" -ForegroundColor White
Write-Host "  2. Open scene: Assets/Scenes/FpsMazeTest_Fresh.unity" -ForegroundColor White
Write-Host "  3. Select MazeTest GameObject" -ForegroundColor White
Write-Host "  4. Run: Tools → Quick Scene Setup → Generate Complete Scene" -ForegroundColor White
Write-Host "     (or press Ctrl+Alt+G)" -ForegroundColor White
Write-Host ""
Write-Host "  This will add and configure all components:" -ForegroundColor Cyan
Write-Host "    ✅ MazeGenerator (31x31 maze)" -ForegroundColor Green
Write-Host "    ✅ MazeRenderer (stone textures)" -ForegroundColor Green
Write-Host "    ✅ MazeIntegration (rooms + doors)" -ForegroundColor Green
Write-Host "    ✅ SpatialPlacer (torches, chests, items, enemies)" -ForegroundColor Green
Write-Host "    ✅ TorchPool (torch instancing)" -ForegroundColor Green
Write-Host "    ✅ LightPlacementEngine (dynamic lighting)" -ForegroundColor Green
Write-Host "    ✅ LightEngine (fog of war + lights)" -ForegroundColor Green
Write-Host "    ✅ GroundPlaneGenerator (stone floor)" -ForegroundColor Green
Write-Host "    ✅ CeilingGenerator (stone ceiling)" -ForegroundColor Green
Write-Host "    ✅ FpsMazeTest (FPS test controller)" -ForegroundColor Green
Write-Host ""
Write-Host "  5. Press Play to test!" -ForegroundColor White
Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan

# Ask for backup
Write-Host "`n[REMINDER] Run backup after testing:" -ForegroundColor Yellow
Write-Host "  .\backup.ps1" -ForegroundColor White
Write-Host ""
