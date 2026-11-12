using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador del Menú Principal del videojuego.
/// Gestiona los botones de JUGAR y SALIR, así como el panel de controles y música.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Botones Principales")]
    public Button playButton;
    public Button quitButton;
    public Button instructionsButton;
    public Button closeInstructionsButton;

    [Header("Paneles Opcionales")]
    public GameObject instructionsPanel;

    [Header("Música de Fondo del Juego")]
    [Tooltip("Asigna aquí la pista de música de fondo (ej: BGMusic) para que arranque desde el menú y continúe durante todo el juego.")]
    public AudioClip backgroundMusic;

    [Header("Protección contra Clics Accidentales")]
    [Tooltip("Tiempo en segundos que se bloquean los clics al entrar al menú para evitar activar botones por accidente")]
    public float loadInputCooldown = 0.5f;

    private float canInteractTime = 0f;
    private bool isStartingGame = false;

    private void Awake()
    {
        // Auto-detección inteligente de botones en caso de no estar asignados manualmente
        if (playButton == null)
        {
            playButton = FindButtonByName("PlayButton", "BotonJugar", "BtnPlay", "Jugar");
        }
        if (quitButton == null)
        {
            quitButton = FindButtonByName("QuitButton", "BotonSalir", "BtnQuit", "Salir");
        }
        if (instructionsButton == null)
        {
            instructionsButton = FindButtonByName("InstructionsButton", "BotonControles", "BtnControles", "Controles");
        }

        // Vincular eventos OnClick a cada botón
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }

        if (instructionsButton != null)
        {
            instructionsButton.onClick.RemoveAllListeners();
            instructionsButton.onClick.AddListener(() => ToggleInstructions(true));
        }

        if (closeInstructionsButton != null)
        {
            closeInstructionsButton.onClick.RemoveAllListeners();
            closeInstructionsButton.onClick.AddListener(() => ToggleInstructions(false));
        }
    }

    private void Start()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);

        // Activar período de gracia para ignorar clics residuales de la escena previa
        canInteractTime = Time.unscaledTime + loadInputCooldown;
        isStartingGame = false;

        // Si se configuró la música de fondo aquí, iniciarla de inmediato en bucle continuo
        if (backgroundMusic != null)
        {
            AudioManager.Instance?.PlayBackgroundMusic(backgroundMusic);
        }
        else
        {
            // Música de respaldo
            AudioManager.Instance?.PlayMusic("MenuMusic");
        }
    }

    private void Update()
    {
        // Si aún está en tiempo de gracia inicial o ya se inició la transición, bloquear clicks y teclas
        if (Time.unscaledTime < canInteractTime || isStartingGame)
        {
            return;
        }

        // 1. Respaldo directo de Clic del Mouse (infalible en cualquier configuración de Input)
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;

            if (playButton != null)
            {
                RectTransform rt = playButton.GetComponent<RectTransform>();
                if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos))
                {
                    Debug.Log("[MainMenuController] Clic en JUGAR detectado por pantalla.");
                    StartGame();
                    return;
                }
            }

            if (quitButton != null)
            {
                RectTransform rt = quitButton.GetComponent<RectTransform>();
                if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos))
                {
                    Debug.Log("[MainMenuController] Clic en SALIR detectado por pantalla.");
                    QuitGame();
                    return;
                }
            }
        }

        // 2. Atajos rápidos de teclado (Enter / Espacio para jugar, Escape para salir)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            if (instructionsPanel != null && instructionsPanel.activeSelf)
            {
                ToggleInstructions(false);
            }
            else
            {
                Debug.Log("[MainMenuController] Tecla Enter/Espacio detectada para Jugar.");
                StartGame();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (instructionsPanel != null && instructionsPanel.activeSelf)
            {
                ToggleInstructions(false);
            }
            else
            {
                Debug.Log("[MainMenuController] Tecla Escape detectada para Salir.");
                QuitGame();
            }
        }
    }

    public void StartGame()
    {
        if (isStartingGame) return;
        isStartingGame = true;

        Debug.Log("[MainMenuController] ¡Iniciando el juego en Level1!");
        AudioManager.Instance?.PlaySFX("Attack");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetFullGame();
        }

        SceneManager.LoadScene("Level1");
    }

    public void ToggleInstructions(bool show)
    {
        AudioManager.Instance?.PlaySFX("Coin");
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(show);
        }
    }

    public void QuitGame()
    {
        if (isStartingGame) return;
        isStartingGame = true;

        Debug.Log("[MainMenuController] Saliendo del juego...");
        AudioManager.Instance?.PlaySFX("EnemyHit");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private Button FindButtonByName(params string[] possibleNames)
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (var b in allButtons)
        {
            foreach (var name in possibleNames)
            {
                if (b.gameObject.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return b;
                }
            }
        }
        return null;
    }
}
