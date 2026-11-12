using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// GameManager central: administra vidas, monedas, tiempo límite y transiciones entre niveles.
/// Implementa la regla: al morir en nivel 2 o 3, muestra mensaje y reinicia al Nivel 1.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración de Vidas y Puntuación")]
    public int maxLives = 3;
    public int currentLives;
    public int coins = 0;

    [Header("Temporizador de Nivel")]
    public float levelTimeLimit = 100f; // Tiempo en segundos por nivel
    public float currentTime;
    public bool isGameOver = false;
    public bool isLevelComplete = false;

    [Header("Música de Fondo Global")]
    [Tooltip("Música de fondo que se mantiene continua durante todo el juego")]
    public AudioClip backgroundMusic;

    // Eventos para actualizar la UI en tiempo real
    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnCoinsChanged;
    public static event Action<float> OnTimerTick;
    public static event Action<string> OnGameOverEvent;
    public static event Action OnLevelCompletedEvent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ResetFullGame();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Al cargar un nivel jugable, reanudamos el timer y restablecemos las 3 vidas para ese nivel
        if (scene.name.StartsWith("Level"))
        {
            isGameOver = false;
            isLevelComplete = false;
            currentTime = levelTimeLimit;
            currentLives = maxLives;
            Time.timeScale = 1f;

            // Actualizar UI
            OnLivesChanged?.Invoke(currentLives);
            OnCoinsChanged?.Invoke(coins);
            OnTimerTick?.Invoke(currentTime);

            // Audio: si hay música global asignada, asegurar continuidad
            if (backgroundMusic != null)
            {
                AudioManager.Instance?.PlayBackgroundMusic(backgroundMusic);
            }

            // Audio: si es el nivel 3 (Boss), sonar música de Boss
            if (scene.name.Contains("3") || scene.name.ToLower().Contains("boss"))
            {
                AudioManager.Instance?.PlayMusic("BossMusic");
            }
            else
            {
                AudioManager.Instance?.PlayMusic("LevelMusic");
            }
        }
        else if (scene.name == "MainMenu")
        {
            if (backgroundMusic != null)
            {
                AudioManager.Instance?.PlayBackgroundMusic(backgroundMusic);
            }
            else
            {
                AudioManager.Instance?.PlayMusic("MenuMusic");
            }
        }
    }

    private void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (isGameOver || isLevelComplete || sceneName == "MainMenu") return;

        currentTime -= Time.deltaTime;
        OnTimerTick?.Invoke(currentTime);

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            TriggerPlayerDeath("¡SE AGOTÓ EL TIEMPO!");
        }
    }

    /// <summary>
    /// Restablece todo el juego a estado inicial (Vidas a 3, monedas a 0).
    /// </summary>
    public void ResetFullGame()
    {
        currentLives = maxLives;
        coins = 0;
        currentTime = levelTimeLimit;
        isGameOver = false;
        isLevelComplete = false;
        Time.timeScale = 1f;

        OnLivesChanged?.Invoke(currentLives);
        OnCoinsChanged?.Invoke(coins);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
        AudioManager.Instance?.PlaySFX("Coin");
    }

    public void TakeDamage(int amount = 1, string reason = "¡TE HICIERON DAÑO!")
    {
        if (isGameOver || isLevelComplete) return;

        currentLives -= amount;
        OnLivesChanged?.Invoke(currentLives);

        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (currentLives <= 0)
        {
            // Sin vidas restantes: Game Over
            isGameOver = true;
            AudioManager.Instance?.PlaySFX("Death");
            if (player != null) player.PlayHurtAnimation();

            string currentScene = SceneManager.GetActiveScene().name;
            string deathMsg = currentScene == "Level1"
                ? $"{reason}\nTe has quedado sin vidas.\nReiniciando Nivel 1..."
                : $"{reason}\nTe has quedado sin vidas en este nivel.\nRegresando al Nivel 1...";

            OnGameOverEvent?.Invoke(deathMsg);
            Invoke(nameof(ReturnToLevel1), 2.5f);
        }
        else
        {
            // Aún le quedan vidas: continúa jugando con retroalimentación y parpadeo de invulnerabilidad
            AudioManager.Instance?.PlaySFX("BossHurt");
            if (player != null) player.TriggerDamageFeedback();
        }
    }

    /// <summary>
    /// Llamado cuando el jugador cae al abismo / pozo sin fondo.
    /// </summary>
    public void HandleFallIntoAbyss()
    {
        if (isGameOver || isLevelComplete) return;

        currentLives--;
        OnLivesChanged?.Invoke(currentLives);
        AudioManager.Instance?.PlaySFX("Death");

        if (currentLives <= 0)
        {
            isGameOver = true;
            string currentScene = SceneManager.GetActiveScene().name;
            string deathMsg = currentScene == "Level1"
                ? "¡CAÍSTE AL VACÍO!\nTe has quedado sin vidas.\nReiniciando Nivel 1..."
                : "¡CAÍSTE AL VACÍO!\nTe has quedado sin vidas.\nRegresando al Nivel 1...";

            OnGameOverEvent?.Invoke(deathMsg);
            Invoke(nameof(ReturnToLevel1), 2.5f);
        }
        else
        {
            // Reaparecer al jugador en el inicio de este nivel sin resetear el juego
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null) player.RespawnAtStart();
        }
    }

    /// <summary>
    /// Llamado si el tiempo se agota.
    /// </summary>
    public void TriggerPlayerDeath(string reason = "¡HAS MUERTO!")
    {
        TakeDamage(1, reason);
    }

    private void ReloadCurrentLevel()
    {
        currentTime = levelTimeLimit;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ReturnToLevel1()
    {
        ResetFullGame();
        SceneManager.LoadScene("Level1");
    }

    /// <summary>
    /// Completa el nivel y avanza a la siguiente escena.
    /// </summary>
    public void FinishLevel()
    {
        if (isGameOver || isLevelComplete) return;

        isLevelComplete = true;

        // Restablecer todas las vidas al completar un nivel para entrar al siguiente con vida completa
        currentLives = maxLives;
        OnLivesChanged?.Invoke(currentLives);

        AudioManager.Instance?.PlaySFX("LevelWin");
        OnLevelCompletedEvent?.Invoke();

        Invoke(nameof(LoadNextScene), 2f);
    }

    private void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            // Completó el juego (tras el Boss)
            SceneManager.LoadScene("MainMenu");
        }
    }
}
