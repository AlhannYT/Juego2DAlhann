using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Muestra la pantalla explicativa con las mecánicas del Rey Esqueleto al iniciar el Nivel 3:
/// - Muestra la imagen del jefe.
/// - Explica que el jefe tiene un tiempo de recuperación tras atacar, donde se le debe golpear para bajarle vida.
/// - Explica que al recuperarse, aparecen 2 ayudantes (zombies) para atacarte.
/// - Botón de OK interactivo que también responde al presionar la tecla 'J' (o Enter / Espacio).
/// - Al confirmar, comienza oficialmente la batalla del jefe.
/// </summary>
public class BossIntroController : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelRoot;
    public Button okButton;

    [Header("Configuración")]
    public bool autoShowOnStart = true;

    private bool isIntroOpen = false;
    private float openTime = 0f;
    private BossController cachedBoss;

    private void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;
    }

    private void Start()
    {
        cachedBoss = FindFirstObjectByType<BossController>();

        if (autoShowOnStart)
        {
            ShowIntro();
        }
    }

    public void ShowIntro()
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        isIntroOpen = true;
        openTime = Time.unscaledTime;

        // Pausar el juego para que el jugador lea sin peligro
        Time.timeScale = 0f;

        if (okButton != null)
        {
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(CloseIntro);
        }

        // Si el jefe ya existe, asegurar que no camine ni ataque antes de tiempo
        if (cachedBoss == null) cachedBoss = FindFirstObjectByType<BossController>();
        if (cachedBoss != null)
        {
            cachedBoss.StopWalkingSound();
        }
    }

    private void Update()
    {
        if (!isIntroOpen) return;

        // Pequeño retardo de 0.2s para evitar saltarse la pantalla por pulsar una tecla inmediatamente
        if (Time.unscaledTime < openTime + 0.2f) return;

        bool keyPressed = false;

        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.jKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                keyPressed = true;
            }
        }
        #endif

        try
        {
            if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                keyPressed = true;
            }
        }
        catch { }

        if (keyPressed)
        {
            CloseIntro();
        }
    }

    public void CloseIntro()
    {
        if (!isIntroOpen) return;
        isIntroOpen = false;

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        // Reanudar el tiempo del juego
        Time.timeScale = 1f;

        // Pequeño margen para que el press de J no ejecute un espadazo en falso
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetAttackInputDelay(0.25f);
        }

        // Efecto de sonido al confirmar
        AudioManager.Instance?.PlaySFX("Attack");

        // Iniciar la batalla del jefe
        if (cachedBoss == null) cachedBoss = FindFirstObjectByType<BossController>();
        if (cachedBoss != null)
        {
            cachedBoss.StartBattleFromIntro();
        }

        Debug.Log("[BossIntroController] ¡Pantalla de mecánicas cerrada! ¡Empieza el combate contra el Rey Esqueleto!");
    }
}
