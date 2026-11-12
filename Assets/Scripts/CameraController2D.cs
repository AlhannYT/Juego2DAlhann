using UnityEngine;

/// <summary>
/// Cámara 2D suave que sigue al jugador en el plano horizontal y vertical,
/// y acompaña al GameObject de fondo ubicado directamente en la escena.
/// Permite al desarrollador mover, posicionar y escalar el fondo libremente desde el editor.
/// </summary>
public class CameraController2D : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.5f, -10f);
    public float smoothSpeed = 0.15f;

    [Header("Límites del Nivel")]
    public bool enableBounds = false;
    public float minX = -10f;
    public float maxX = 50f;
    public float minY = -2f;
    public float maxY = 15f;

    [Header("Fondo en Escena (Editable en el Editor)")]
    [Tooltip("Arrastra aquí el GameObject 'Background' de la escena. Si se deja vacío, busca uno llamado 'Background'.")]
    public Transform backgroundTransform;

    [Tooltip("Si está activo, el fondo acompaña a la cámara horizontalmente durante el recorrido.")]
    public bool followCameraX = true;

    [Tooltip("Si está activo, el fondo también acompaña a la cámara verticalmente.")]
    public bool followCameraY = false;

    [Header("Respaldo Dinámico (Opcional)")]
    [Tooltip("Sprite opcional por si la escena no cuenta con un GameObject de fondo.")]
    public Sprite backgroundSprite;
    public float backgroundFixedY = 3.5f;
    public float backgroundZDepth = 15f;
    public float backgroundScaleMultiplier = 1.35f;

    private float bgOffsetX = 0f;
    private float bgOffsetY = 0f;
    private float bgFixedY = 3.5f;
    private float bgZ = 15f;

    private void Start()
    {
        // 1. Buscar el fondo de la escena si no se asignó en el Inspector
        if (backgroundTransform == null)
        {
            GameObject[] sceneObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var go in sceneObjects)
            {
                if ((go.name == "Background" || go.name == "LevelBackground") && go.GetComponent<SpriteRenderer>() != null)
                {
                    backgroundTransform = go.transform;
                    break;
                }
            }
        }

        // 2. Si existe un fondo en la escena, registrar su posición configurada por el usuario
        if (backgroundTransform != null)
        {
            bgOffsetX = backgroundTransform.position.x - transform.position.x;
            bgOffsetY = backgroundTransform.position.y - transform.position.y;
            bgFixedY = backgroundTransform.position.y;
            bgZ = backgroundTransform.position.z;
        }
        else if (backgroundSprite != null)
        {
            // Solo si no existe en la escena, crearlo dinámicamente como respaldo
            SetupFallbackBackground();
        }
    }

    private void SetupFallbackBackground()
    {
        GameObject bgObj = new GameObject("Background");
        backgroundTransform = bgObj.transform;
        bgFixedY = backgroundFixedY;
        bgZ = backgroundZDepth;
        backgroundTransform.position = new Vector3(transform.position.x, bgFixedY, bgZ);

        SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sprite = backgroundSprite;
        sr.sortingOrder = -100;

        Camera cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        if (cam != null && backgroundSprite != null)
        {
            float camHeight = cam.orthographicSize * 2f;
            float spriteHeight = backgroundSprite.bounds.size.y;
            if (spriteHeight > 0.01f)
            {
                float scale = (camHeight / spriteHeight) * backgroundScaleMultiplier;
                backgroundTransform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        bgOffsetX = 0f;
        bgOffsetY = 0f;
    }

    public void SetBackground(Sprite newSprite)
    {
        backgroundSprite = newSprite;
        if (backgroundTransform != null)
        {
            SpriteRenderer sr = backgroundTransform.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = newSprite;
        }
        else if (newSprite != null)
        {
            SetupFallbackBackground();
        }
    }

    private void LateUpdate()
    {
        // 1. Seguimiento suave del jugador
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }

        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;

            if (enableBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            }

            desiredPosition.z = offset.z; // Mantener la profundidad en Z (-10)
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        }

        // 2. Acompañar el fondo en el eje horizontal (manteniendo la altura Y y escala elegida por el usuario)
        if (backgroundTransform != null)
        {
            float newX = followCameraX ? (transform.position.x + bgOffsetX) : backgroundTransform.position.x;
            float newY = followCameraY ? (transform.position.y + bgOffsetY) : bgFixedY;
            backgroundTransform.position = new Vector3(newX, newY, bgZ);
        }
    }
}
