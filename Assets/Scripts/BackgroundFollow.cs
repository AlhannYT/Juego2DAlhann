using UnityEngine;

/// <summary>
/// Controla la imagen de fondo (Background) del nivel.
/// Sigue a la cámara/jugador horizontalmente durante todo el recorrido
/// manteniendo una altura vertical fija (sin saltar con el jugador)
/// y asegurando que siempre se dibuje detrás de todo el escenario.
/// </summary>
public class BackgroundFollow : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    [Tooltip("Cámara objetivo (si no se asigna, busca Camera.main automáticamente)")]
    public Camera targetCamera;

    [Tooltip("Altura fija en Y para que el fondo no suba ni baje al saltar")]
    public float fixedY = 3.5f;

    [Tooltip("Profundidad en Z (debe estar detrás de los tiles y personajes)")]
    public float zDepth = 15f;

    [Tooltip("Escala personalizada del fondo")]
    public Vector2 backgroundScale = new Vector2(3.5f, 3.5f);

    [Tooltip("Si está activo, ajusta automáticamente la escala para cubrir la altura de la cámara")]
    public bool autoFitCameraHeight = true;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Capa de ordenamiento negativa para que siempre quede detrás del escenario
            spriteRenderer.sortingOrder = -100;
        }
    }

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null && autoFitCameraHeight && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            float camHeight = targetCamera.orthographicSize * 2f;
            float spriteHeight = spriteRenderer.sprite.bounds.size.y;
            if (spriteHeight > 0.01f)
            {
                // Un margen extra (1.35x) para cubrir con solidez la vista completa
                float scaleFactor = (camHeight / spriteHeight) * 1.35f;
                transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
            }
        }
        else
        {
            transform.localScale = new Vector3(backgroundScale.x, backgroundScale.y, 1f);
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        // Seguir únicamente la coordenada X de la cámara (recorrido horizontal completo)
        transform.position = new Vector3(targetCamera.transform.position.x, fixedY, zDepth);
    }
}
