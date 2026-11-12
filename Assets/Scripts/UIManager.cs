using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controla la interfaz de usuario durante el juego:
/// Vidas, Monedas, Temporizador, cartel de Game Over y Sistema de Pausa.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Textos de Información")]
    public TMP_Text livesText;
    public TMP_Text coinsText;
    public TMP_Text timerText;

    [Header("Pantalla / Label de Muerte y Game Over")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverLabel;

    [Header("Pantalla de Nivel Completado")]
    public GameObject levelCompletePanel;

    [Header("Sistema de Pausa")]
    [Tooltip("Panel de Pausa (PauseMenu)")]
    public GameObject pauseMenu;
    [Tooltip("Botón de Pausa superior (PauseButton)")]
    public Button pauseButton;

    private void OnEnable()
    {
        GameManager.OnLivesChanged += UpdateLivesUI;
        GameManager.OnCoinsChanged += UpdateCoinsUI;
        GameManager.OnTimerTick += UpdateTimerUI;
        GameManager.OnGameOverEvent += ShowGameOverUI;
        GameManager.OnLevelCompletedEvent += ShowLevelCompletedUI;
    }

    private void OnDisable()
    {
        GameManager.OnLivesChanged -= UpdateLivesUI;
        GameManager.OnCoinsChanged -= UpdateCoinsUI;
        GameManager.OnTimerTick -= UpdateTimerUI;
        GameManager.OnGameOverEvent -= ShowGameOverUI;
        GameManager.OnLevelCompletedEvent -= ShowLevelCompletedUI;
    }

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        // Auto-vincular elementos de pausa si no se asignaron en el inspector
        if (pauseMenu == null)
        {
            Transform pm = transform.Find("PauseMenu");
            if (pm != null) pauseMenu = pm.gameObject;
        }

        if (pauseButton == null)
        {
            Transform pb = transform.Find("PauseButton");
            if (pb != null) pauseButton = pb.GetComponent<Button>();
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(OpenPause);
        }

        // El menú de pausa siempre arranca cerrado al iniciar el nivel
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            UpdateLivesUI(GameManager.Instance.currentLives);
            UpdateCoinsUI(GameManager.Instance.coins);
            UpdateTimerUI(GameManager.Instance.currentTime);
        }
    }

    private void Update()
    {
        // 1. Respaldo directo de clic en el botón de pausa ||
        if (Input.GetMouseButtonDown(0))
        {
            if (pauseButton != null && pauseButton.gameObject.activeInHierarchy)
            {
                RectTransform rt = pauseButton.GetComponent<RectTransform>();
                if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition))
                {
                    OpenPause();
                    return;
                }
            }
        }

        // 2. Atajos de teclado: Escape o P para abrir/cerrar pausa
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
            {
                TogglePause();
                return;
            }
        }
        #endif

        try
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
                return;
            }
        }
        catch { }
    }

    private float lastToggleTime = 0f;

    public void OpenPause()
    {
        if (pauseMenu != null && pauseMenu.activeSelf) return;

        Time.timeScale = 0f;
        AudioManager.Instance?.OnGamePaused();

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(false);
        }
    }

    public void ClosePause()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.OnGameResumed();

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
        }
    }

    public void TogglePause()
    {
        // Evitar rebote del teclado en el mismo fotograma o intervalo muy corto
        if (Time.unscaledTime < lastToggleTime + 0.25f) return;
        lastToggleTime = Time.unscaledTime;

        if (pauseMenu != null && pauseMenu.activeSelf)
        {
            ClosePause();
        }
        else
        {
            OpenPause();
        }
    }

    private void UpdateLivesUI(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Vidas: {lives}";
        }
    }

    private void UpdateCoinsUI(int coins)
    {
        if (coinsText != null)
        {
            coinsText.text = $"Monedas: {coins}";
        }
    }

    private void UpdateTimerUI(float secondsRemaining)
    {
        if (timerText != null)
        {
            timerText.text = $"Tiempo: {Mathf.CeilToInt(secondsRemaining)}s";
        }
    }

    private void ShowGameOverUI(string message)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverLabel != null)
            {
                gameOverLabel.text = message;
            }
        }
    }

    private void ShowLevelCompletedUI()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
    }
}
