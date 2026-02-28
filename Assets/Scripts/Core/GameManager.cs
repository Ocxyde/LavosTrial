using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GAMEMANAGER â€” Cerveau central du jeu (Singleton)
/// Attache ce script Ã  un GameObject vide nommÃ© "GameManager" dans ta scÃ¨ne.
/// Coche "Don't Destroy On Load" pour qu'il persiste entre les scÃ¨nes.
/// </summary>
public class GameManager : MonoBehaviour
{
    // â”€â”€â”€ Singleton â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public static GameManager Instance { get; private set; }

    // â”€â”€â”€ Ã‰tats du jeu â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public enum GameState { Playing, Paused, GameOver, Victory }
    public GameState CurrentState { get; private set; }

    // â”€â”€â”€ Score â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public int Score { get; private set; }

    // â”€â”€â”€ Ã‰vÃ©nements (les autres scripts peuvent s'y abonner) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public static event System.Action<int> OnScoreChanged;
    public static event System.Action<GameState> OnGameStateChanged;

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    void Awake()
    {
        // ImplÃ©mentation du Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persiste entre les scÃ¨nes
    }

    void Start()
    {
        SetGameState(GameState.Playing);
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  MÃ‰THODES PUBLIQUES
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Ajoute des points au score.</summary>
    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score); // Notifie l'UI
        Debug.Log($"[GameManager] Score : {Score}");
    }

    /// <summary>Change l'Ã©tat global du jeu.</summary>
    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        // Gestion du temps selon l'Ã©tat
        Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;

        Debug.Log($"[GameManager] Ã‰tat du jeu : {newState}");
    }

    /// <summary>Active / dÃ©sactive la pause.</summary>
    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            SetGameState(GameState.Paused);
        else if (CurrentState == GameState.Paused)
            SetGameState(GameState.Playing);
    }

    /// <summary>DÃ©clenche le Game Over.</summary>
    public void TriggerGameOver()
    {
        SetGameState(GameState.GameOver);
    }

    /// <summary>Recharge la scÃ¨ne courante (recommencer).</summary>
    public void RestartGame()
    {
        Score = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Charge une scÃ¨ne par son nom.</summary>
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
