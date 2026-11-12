using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// La Yaroa Sagrada: El objetivo y trofeo final tras derrotar al Jefe en el Nivel 3.
/// Sale expulsada en un arco hacia adelante, cae al suelo con rebote y espera a que el jugador
/// camine hacia ella para recogerla y consagrar su victoria.
/// Al recogerla, despliega la pantalla final con la victoria a la izquierda y el resumen de monedas a la derecha.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class YaroaGoal : MonoBehaviour
{
    [Header("Configuración Visual")]
    public float floatAmplitude = 0.18f;
    public float floatSpeed = 3f;
    public float scalePulseSpeed = 2f;
    public float scalePulseAmount = 0.12f;

    [Header("Referencias de Victoria")]
    public GameObject victoryPanel;
    [Tooltip("Texto de felicitación y lore (lado izquierdo)")]
    public TMP_Text victoryText;
    [Tooltip("Texto de resumen de monedas (lado derecho)")]
    public TMP_Text coinsSummaryText;

    [Header("Configuración de Monedas")]
    public int totalLevelCoins = 6;

    private Vector3 startPosition;
    private Vector3 baseScale;
    private bool isCollected = false;
    private bool canBeCollected = false;
    private bool isLaunching = false;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        startPosition = transform.position;
        baseScale = transform.localScale;

        // Si no se inició por LaunchTowards, activar recolección con retraso de seguridad
        if (!isLaunching)
        {
            StartCoroutine(SafetyUnlockRoutine());
        }

        // Buscar panel de victoria si no está asignado
        if (victoryPanel == null)
        {
            GameObject vp = GameObject.Find("VictoryPanel");
            if (vp != null)
            {
                victoryPanel = vp;
                TMP_Text[] texts = vp.GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length > 0 && victoryText == null) victoryText = texts[0];
                if (texts.Length > 1 && coinsSummaryText == null) coinsSummaryText = texts[1];
            }
        }
    }

    private IEnumerator SafetyUnlockRoutine()
    {
        canBeCollected = false;
        yield return new WaitForSeconds(1.2f);
        canBeCollected = true;
    }

    public void LaunchTowards(Vector3 fromPos, Vector3 targetPos)
    {
        isLaunching = true;
        StartCoroutine(LaunchArcRoutine(fromPos, targetPos));
    }

    private IEnumerator LaunchArcRoutine(Vector3 from, Vector3 target)
    {
        canBeCollected = false;
        transform.position = from;

        float duration = 0.85f;
        float elapsed = 0f;

        // Animación en arco parabólico (vuela desde el jefe hacia el suelo frente al jugador)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float x = Mathf.Lerp(from.x, target.x, t);
            float arc = Mathf.Sin(t * Mathf.PI) * 2.2f;
            float y = Mathf.Lerp(from.y, target.y, t) + arc;

            transform.position = new Vector3(x, y, 0f);

            // Ligero giro visual durante el vuelo
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 2f) * 15f);

            yield return null;
        }

        transform.rotation = Quaternion.identity;
        transform.position = target;
        startPosition = target;

        // Sonido de caída/rebote de la Yaroa al tocar el suelo
        AudioManager.Instance?.PlaySFX("Coin");

        // Pequeño retraso adicional tras aterrizar para que el jugador la aprecie antes de poder tomarla
        yield return new WaitForSeconds(0.5f);
        canBeCollected = true;
        isLaunching = false;
    }

    private void Update()
    {
        if (isCollected || isLaunching) return;

        // 1. Animación de flotación suave arriba y abajo
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 2. Pulsación sutil de escala como un artefacto legendario
        float pulse = 1f + Mathf.PingPong(Time.time * scalePulseSpeed, scalePulseAmount);
        transform.localScale = baseScale * pulse;

        // 3. Brillo dorado sutil
        if (spriteRenderer != null)
        {
            float glow = Mathf.PingPong(Time.time * 3f, 0.25f) + 0.75f;
            spriteRenderer.color = new Color(1f, 1f, glow, 1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected || !canBeCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;
            StartCoroutine(CollectYaroaRoutine(other.gameObject));
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isCollected || !canBeCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;
            StartCoroutine(CollectYaroaRoutine(other.gameObject));
        }
    }

    private IEnumerator CollectYaroaRoutine(GameObject player)
    {
        // Efectos de sonido épicos
        AudioManager.Instance?.PlaySFX("Coin");
        AudioManager.Instance?.PlaySFX("Attack");

        // Ocultar la Yaroa al recogerla
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Salto festivo del jugador al recogerla
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = new Vector2(0f, 6f);
        }

        // Mostrar pantalla de felicitación y victoria
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            // Obtener conteo de monedas del jugador
            int collectedCoins = (GameManager.Instance != null) ? GameManager.Instance.coins : 0;
            int missedCoins = Mathf.Max(0, totalLevelCoins - collectedCoins);

            // 1. Mensaje de victoria a la izquierda
            if (victoryText != null)
            {
                victoryText.text = "<b><size=32><color=#FFD700>¡VICTORIA ÉPICA, SAMURAI!</color></size></b>\n\n" +
                                   "Has derrotado al Rey Esqueleto y conseguido la legendaria <b>YAROA</b>.\n\n" +
                                   "¡La orden del honor y del buen comer está a salvo!";
            }

            // Buscar componente de resumen de monedas a la derecha si no estaba asignado directamente
            if (coinsSummaryText == null)
            {
                TMP_Text[] allTexts = victoryPanel.GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in allTexts)
                {
                    if (t != victoryText)
                    {
                        coinsSummaryText = t;
                        break;
                    }
                }
            }

            // 2. Resumen de monedas a la derecha
            if (coinsSummaryText != null)
            {
                string statusMsg;
                if (missedCoins == 0)
                {
                    statusMsg = "<color=#22C55E>★ ¡PERFECTO! ★\n¡Conseguiste todas las monedas del nivel!</color>";
                }
                else
                {
                    statusMsg = $"<color=#F87171>Te faltaron {missedCoins} moneda{(missedCoins > 1 ? "s" : "")} por recolectar.</color>";
                }

                coinsSummaryText.text = $"<b><size=30><color=#FBBF24>RESUMEN DE RECOMPENSAS</color></size></b>\n\n" +
                                        $"Monedas conseguidas:\n" +
                                        $"<b><size=42><color=#FFD700>{collectedCoins}</color> / <color=#E2E8F0>{totalLevelCoins}</color></size></b>\n\n" +
                                        $"{statusMsg}";
            }
        }

        // Esperar unos segundos para disfrutar la victoria antes de ir al menú
        yield return new WaitForSeconds(6.0f);

        SceneManager.LoadScene("MainMenu");
    }
}
