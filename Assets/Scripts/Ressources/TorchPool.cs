using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TORCHPOOL â€” Pool d'objets pour les torches.
///
/// FONCTIONNEMENT :
///  - Les GameObjects de torche sont crÃ©Ã©s une fois puis dÃ©sactivÃ©s (pas dÃ©truits).
///  - Ã€ la rÃ©gÃ©nÃ©ration, ils sont rÃ©activÃ©s et repositionnÃ©s â†’ zÃ©ro allocation.
///  - Si le nouveau labyrinthe a besoin de plus de torches que le pool,
///    il crÃ©e les manquantes et les ajoute au pool.
///  - ReleaseAll() remet toutes les torches en rÃ©serve sans les dÃ©truire.
///
/// SETUP : Attache sur le mÃªme GameObject que MazeRenderer.
/// </summary>
public class TorchPool : MonoBehaviour
{
    // â”€â”€â”€ Pool â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private readonly List<GameObject>      _pool   = new List<GameObject>();
    private readonly List<TorchController> _active = new List<TorchController>();
    private Transform                      _poolRoot;

    // â”€â”€â”€ MatÃ©riaux partagÃ©s (Ã©vite les allocations Ã  chaque torche) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private Material _sharedFlameMat;
    private Material _sharedHandleMat;

    // â”€â”€â”€ Brasero Flame â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    [SerializeField] private bool useBraseroFlame = true;

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    void Awake()
    {
        _poolRoot = new GameObject("TorchPool_Inactive").transform;
        _poolRoot.SetParent(transform);
        _poolRoot.gameObject.SetActive(false); // Cache tout le conteneur
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  RÃ‰CUPÃ‰RER UNE TORCHE (depuis le pool ou nouvelle crÃ©ation)
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Retourne une torche prÃªte Ã  l'emploi, repositionnÃ©e et rÃ©initialisÃ©e.
    /// </summary>
    public TorchController Get(Vector3 position, Quaternion rotation,
                               Transform activeParent, Texture2D[] flameFrames,
                               Material flameMat, Material handleMat)
    {
        // Stocke les matÃ©riaux partagÃ©s (crÃ©e une seule instance)
        if (flameMat != null && _sharedFlameMat == null)
            _sharedFlameMat = new Material(flameMat);
        else if (_sharedFlameMat == null && flameFrames != null && flameFrames.Length > 0 && flameFrames[0] != null)
        {
            // Fallback: crÃ©er un matÃ©riau basique si flameMat est null
            Debug.LogWarning("[TorchPool] Flame material is null, creating fallback material");
            var fallbackShader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Transparent");
            if (fallbackShader != null)
            {
                _sharedFlameMat = new Material(fallbackShader);
                _sharedFlameMat.mainTexture = flameFrames[0];
            }
        }
        
        if (handleMat != null && _sharedHandleMat == null)
            _sharedHandleMat = new Material(handleMat);

        GameObject go;

        if (_pool.Count > 0)
        {
            // RÃ©utilise depuis le pool
            go = _pool[_pool.Count - 1];
            _pool.RemoveAt(_pool.Count - 1);
        }
        else
        {
            // CrÃ©e une nouvelle torche et l'ajoute au pool pour la prochaine fois
            go = BuildTorchObject();
        }

        // Repositionne et rattache au parent actif
        go.transform.SetParent(activeParent);
        go.transform.position = position;
        go.transform.rotation = rotation;
        go.SetActive(true);

        // RÃ©initialise le controller
        var ctrl = go.GetComponent<TorchController>();
        var flameMR = go.transform.Find("Flame")?.GetComponent<Renderer>();
        var light = go.transform.Find("FlameLight")?.GetComponent<Light>();

        // Defensive: if components are missing, fall back to sprite mode to avoid runtime errors
        if (ctrl == null)
        {
            Debug.LogWarning("[TorchPool] TorchController missing on rebuilt torch. Skipping initialization.");
        }

        if (useBraseroFlame)
        {
            var braseroFlame = go.transform.Find("BraseroFlame")?.GetComponent<BraseroFlame>();
            if (braseroFlame != null && ctrl != null && light != null)
            {
                ctrl.InitializeBrasero(light, braseroFlame);
            }
            else
            {
                Debug.LogWarning("[TorchPool] BraseroFlame setup incomplete, falling back to sprite mode");
                SetupSpriteMode(go, flameFrames, flameMR, light, ctrl);
            }
        }
        else
        {
            SetupSpriteMode(go, flameFrames, flameMR, light, ctrl);
        }

        _active.Add(ctrl);
        return ctrl;
    }

    private void SetupSpriteMode(GameObject go, Texture2D[] flameFrames, Renderer flameMR, Light light, TorchController ctrl)
    {
        if (ctrl == null)
        {
            Debug.LogError("[TorchPool] TorchController component missing!");
            return;
        }

        ctrl.Initialize(flameFrames, flameMR, light);
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  LIBÃ‰RER TOUTES LES TORCHES ACTIVES â†’ retour dans le pool
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// DÃ©sactive toutes les torches actives et les remet en rÃ©serve.
    /// Appeler avant chaque rÃ©gÃ©nÃ©ration de labyrinthe.
    /// </summary>
    public void ReleaseAll()
    {
        foreach (var ctrl in _active)
        {
            if (ctrl == null) continue;
            var go = ctrl.gameObject;
            go.SetActive(false);
            go.transform.SetParent(_poolRoot);
            _pool.Add(go);
        }
        _active.Clear();
        Debug.Log($"[TorchPool] {_pool.Count} torche(s) remise(s) en rÃ©serve.");
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  DESTRUCTION COMPLÃˆTE (fin de session)
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>DÃ©truit toutes les torches (pool + actives). Appeler Ã  la fin du jeu.</summary>
    public void DestroyAll()
    {
        ReleaseAll();
        foreach (var go in _pool)
            if (go != null) Destroy(go);
        _pool.Clear();

        if (_sharedFlameMat != null) { Destroy(_sharedFlameMat); _sharedFlameMat = null; }
        if (_sharedHandleMat != null) { Destroy(_sharedHandleMat); _sharedHandleMat = null; }
    }

    void OnDestroy() => DestroyAll();

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  CONSTRUCTION D'UNE TORCHE (interne)
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private GameObject BuildTorchObject()
    {
        var torchGO = new GameObject("Torch");

        // â”€â”€ Support (bÃ¢ton) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        // CrÃ©er un cube Ã©troit pour le bois
        var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "Handle";
        handle.transform.SetParent(torchGO.transform);
        handle.transform.localPosition = new Vector3(0f, 0f, 0f);
        // Inclinaison de 25Â° vers l'intÃ©rieur (l'avant du mur)
        handle.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
        handle.transform.localScale    = new Vector3(0.08f, 0.35f, 0.08f);
        handle.GetComponent<MeshRenderer>().sharedMaterial = _sharedHandleMat;
        Destroy(handle.GetComponent<BoxCollider>());

        if (useBraseroFlame)
        {
            // â”€â”€ Brasero Flame (Particle System) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var flame = new GameObject("BraseroFlame");
            flame.transform.SetParent(torchGO.transform);
            flame.transform.localPosition = new Vector3(0f, 0.25f, 0.05f);
            flame.transform.localRotation = Quaternion.identity;
            
            var braseroFlame = flame.AddComponent<BraseroFlame>();
            
            // â”€â”€ LumiÃ¨re â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var lightGO = new GameObject("FlameLight");
            lightGO.transform.SetParent(torchGO.transform);
            lightGO.transform.localPosition = new Vector3(0f, 0.4f, 0.08f);
            
            var pointLight = lightGO.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.range = 10f;
            pointLight.intensity = 2.5f;
            pointLight.color = new Color(1f, 0.6f, 0.3f);
            pointLight.shadows = LightShadows.None;
            
            // Add light flicker to the torch controller
            var ctrl = torchGO.AddComponent<TorchController>();
            ctrl.InitializeBrasero(lightGO.GetComponent<Light>(), braseroFlame);
        }
        else
        {
            // â”€â”€ Flamme (billboard 2D pixel art) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var flame = GameObject.CreatePrimitive(PrimitiveType.Quad);
            flame.name = "Flame";
            flame.transform.SetParent(torchGO.transform);
            // Position au bout du bois, penchÃ©e aussi
            flame.transform.localPosition = new Vector3(0f, 0.22f, 0.06f);
            flame.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
            flame.transform.localScale    = new Vector3(0.3f, 0.45f, 1f);
            flame.GetComponent<MeshRenderer>().sharedMaterial = _sharedFlameMat;
            Destroy(flame.GetComponent<MeshCollider>());

            // â”€â”€ LumiÃ¨re â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var lightGO = new GameObject("FlameLight");
            lightGO.transform.SetParent(torchGO.transform);
            lightGO.transform.localPosition = new Vector3(0f, 0.3f, 0.08f);
            
            var pointLight = lightGO.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.range = 10f;
            pointLight.intensity = 2.5f;
            pointLight.color = new Color(1f, 0.6f, 0.3f); // Orange chaud
            pointLight.shadows = LightShadows.None; // DÃ©sactive les ombres pour Ã©viter le warning

            // â”€â”€ Controller â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            torchGO.AddComponent<TorchController>();
        }

        return torchGO;
    }

    // â”€â”€â”€ Stats (debug) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public int ActiveCount => _active.Count;
    public int PooledCount => _pool.Count;
}
