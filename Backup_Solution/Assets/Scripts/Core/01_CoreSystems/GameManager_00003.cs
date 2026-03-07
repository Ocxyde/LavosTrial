// GameManager.cs
// Central game state manager
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Main pivot point for plug-in-and-out system

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GAMEMANAGER — Cerveau central du jeu
    /// Attache ce script à un GameObject vide nommé "GameManager" dans ta scène.
    /// Coche "Don't Destroy On Load" pour qu'il persiste entre les scènes.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
    // ─── États du jeu ─────────────────────────────────────────────────────────────
    public enum GameState { Playing, Paused, GameOver, Victory }
    public GameState CurrentState { get; private set; }

    // ─── Score ─────────────────────────────────────────────────────────────
    public int Score { get; private set; }

    // ─── Événements (les autres scripts peuvent s'y abonner) ─────────────────────
    public static event System.Action<int> OnScoreChanged;
    public static event System.Action<GameState> OnGameStateChanged;

    // ─────────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        // Ensure this persists between scenes
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetGameState(GameState.Playing);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    //  MÉTHODES PUBLIQUES
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>Ajoute des points au score.</summary>
    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
        Debug.Log($"[GameManager] Score : {Score}");
    }

    /// <summary>Change l'état global du jeu.</summary>
    public void SetGameState(GameState newState)
    {
        CurrentState = newState;

        // PLUG-IN-AND-OUT: Invoke via EventHandler
        if (EventHandler.Instance != null)
        {
            EventHandler.Instance.InvokeGameStateChanged(newState);

            if (newState == GameState.Paused)
                EventHandler.Instance.InvokeGamePaused();
            else if (newState == GameState.Playing)
                EventHandler.Instance.InvokeGameResumed();
            else if (newState == GameState.GameOver)
                EventHandler.Instance.InvokeGameOver();
            else if (newState == GameState.Victory)
                EventHandler.Instance.InvokeGameVictory();
        }

        OnGameStateChanged?.Invoke(newState);
        Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;
        Debug.Log($"[GameManager] État du jeu : {newState}");
    }

    /// <summary>Active / désactive la pause.</summary>
    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            SetGameState(GameState.Paused);
        else if (CurrentState == GameState.Paused)
            SetGameState(GameState.Playing);
    }

    /// <summary>Déclenche le Game Over.</summary>
    public void TriggerGameOver()
    {
        SetGameState(GameState.GameOver);
    }

    /// <summary>Recharge la scène courante (recommencer).</summary>
    public void RestartGame()
    {
        Score = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Déclenche la victoire.</summary>
    public void TriggerVictory()
    {
        SetGameState(GameState.Victory);
    }
}
