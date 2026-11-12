using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controlador de la ventana / panel de Pausa en el juego.
/// Gestiona la congelación del tiempo (Time.timeScale), el botón CONTINUAR,
/// el botón MENU PRINCIPAL con transición suave y cooldown para evitar clics accidentales.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Botones Principales")]
    [Tooltip("Botón para reanudar la partida")]
    public Button resumeButton;

    [Tooltip("Botón para volver al menú principal")]
    public Button mainMenuButton;

    // Nombres alternativos por compatibilidad con prefab del menú
    public Button playButton;
    public Button quitButton;

    [Header("Referencias Adicionales")]
    [Tooltip("Botón de pausa superior (||)")]
    public Button pauseButton;

    [Tooltip("Panel de pausa (por defecto este mismo GameObject)")]
    public GameObject pausePanel;

    [Tooltip("Imagen de fondo para la transición de oscurecimiento")]
    public Image transitionOverlay;

    [Header("Protección de Clics")]
    [Tooltip("Tiempo mínimo entre pulsaciones de botones")]
    public float buttonCooldown = 0.4f;

    private float menuOpenTime = 0f;
    private float lastButtonPressTime = 0f;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (pausePanel == null) pausePanel = gameObject;

        // Asignación de respaldo si venían serializados con nombres del menú principal
        if (resumeButton == null && playButton != null) resumeButton = playButton;
        if (mainMenuButton == null && quitButton != null) mainMenuButton = quitButton;

        // Auto-detección por jerarquía si no están conectados
        if (resumeButton == null)
        {
            Transform t = transform.Find("PlayButton") ?? transform.Find("ContinuarButton") ?? transform.Find("ResumeButton");
            if (t != null) resumeButton = t.GetComponent<Button>();
        }

        if (mainMenuButton == null)
        {
            Transform t = transform.Find("QuitButton") ?? transform.Find("MainMenuButton") ?? transform.Find("MenuPrincipalButton");
            if (t != null) mainMenuButton = t.GetComponent<Button>();
        }

        if (pauseButton == null && transform.parent != null)
        {
            Transform pb = transform.parent.Find("PauseButton");
            if (pb != null) pauseButton = pb.GetComponent<Button>();
        }

        if (transitionOverlay == null)
        {
            Transform bg = transform.Find("BackgroundOverlay");
            if (bg != null) transitionOverlay = bg.GetComponent<Image>();
        }

        // Configurar eventos
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }

    private void OnEnable()
    {
        menuOpenTime = Time.unscaledTime;
        lastButtonPressTime = Time.unscaledTime;
        isTransitioning = false;

        // Restaurar interactividad de botones
        if (resumeButton != null) resumeButton.interactable = true;
        if (mainMenuButton != null) mainMenuButton.interactable = true;

        // Al activarse el menú de pausa, congelamos el tiempo y pausamos SFX con música al 50%
        Time.timeScale = 0f;
        AudioManager.Instance?.OnGamePaused();

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isTransitioning) return;

        // Evitar que el mismo fotograma que abrió el menú lo cierre accidentalmente
        if (Time.unscaledTime < menuOpenTime + 0.25f) return;

        // 1. Detección directa de clics del mouse con coordenadas de pantalla
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;

            if (resumeButton != null && RectTransformUtility.RectangleContainsScreenPoint(resumeButton.GetComponent<RectTransform>(), mousePos))
            {
                ResumeGame();
                return;
            }

            if (mainMenuButton != null && RectTransformUtility.RectangleContainsScreenPoint(mainMenuButton.GetComponent<RectTransform>(), mousePos))
            {
                GoToMainMenu();
                return;
            }
        }

        // 2. Atajos de teclado: Escape o P para reanudar, Enter / Barra espaciadora para continuar
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
            {
                ResumeGame();
                return;
            }
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ResumeGame();
                return;
            }
        }
        #endif

        try
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                ResumeGame();
                return;
            }
        }
        catch { }
    }

    /// <summary>
    /// Reanuda la partida, restaura el tiempo a 1 y oculta este panel.
    /// </summary>
    public void ResumeGame()
    {
        if (isTransitioning) return;
        if (Time.unscaledTime < lastButtonPressTime + buttonCooldown) return;
        lastButtonPressTime = Time.unscaledTime;

        Time.timeScale = 1f;

        AudioManager.Instance?.OnGameResumed();
        AudioManager.Instance?.PlaySFX("Attack");

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Inicia la transición suave de regreso al Menú Principal.
    /// </summary>
    public void GoToMainMenu()
    {
        if (isTransitioning) return;
        if (Time.unscaledTime < lastButtonPressTime + buttonCooldown) return;
        lastButtonPressTime = Time.unscaledTime;
        isTransitioning = true;

        StartCoroutine(TransitionToMainMenuRoutine());
    }

    private IEnumerator TransitionToMainMenuRoutine()
    {
        // Bloquear ambos botones de inmediato para evitar cualquier doble clic
        if (resumeButton != null) resumeButton.interactable = false;
        if (mainMenuButton != null) mainMenuButton.interactable = false;

        AudioManager.Instance?.PlaySFX("Attack");

        // Transición de oscurecimiento suave
        Image overlay = transitionOverlay;
        if (overlay == null)
        {
            Transform bg = transform.Find("BackgroundOverlay");
            if (bg != null) overlay = bg.GetComponent<Image>();
        }

        float duration = 0.35f;
        float elapsed = 0f;
        Color startColor = overlay != null ? overlay.color : new Color(0, 0, 0, 0.7f);
        Color targetColor = new Color(0, 0, 0, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (overlay != null)
            {
                overlay.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            }
            yield return null;
        }

        // Restaurar estado antes de cargar la escena
        Time.timeScale = 1f;
        AudioManager.Instance?.OnGameResumed();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetFullGame();
        }

        SceneManager.LoadScene("MainMenu");
    }
}
