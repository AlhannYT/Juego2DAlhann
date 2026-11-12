using UnityEngine;

/// <summary>
/// Enemigo terrestre tipo patrulla (Zombie / Esqueleto).
/// Camina de lado a lado de forma suave y predecible.
/// Se da la vuelta por tiempo o si choca contra una pared/tope.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Configuración de Patrulla")]
    public float speed = 1.5f;
    public float patrolTime = 3.5f; // Segundos que camina en una dirección antes de girar
    public bool startMovingRight = false; // Los sprites de este pack miran a la izquierda por defecto

    [Header("Modo Persecución (Para esbirros del Boss)")]
    public bool chasePlayer = false;
    private Transform playerTransform;

    [Header("Animación de Muerte")]
    public Sprite[] deathFrames;
    public float deathFps = 10f;

    [Header("Sonidos de Zombie")]
    [Tooltip("Sonido aleatorio del zombie (gemido/gruñido)")]
    public AudioClip ambientSound;
    [Tooltip("Lista opcional de varios sonidos para mayor variedad aleatoria")]
    public AudioClip[] randomSounds;
    [Range(0f, 1f)] public float ambientVolume = 0.8f;
    [Tooltip("Tiempo base de espera entre sonidos (por defecto 15 segundos)")]
    public float minSoundInterval = 15f;
    [Tooltip("Variación aleatoria adicional (0 a randomSoundJitter segundos)")]
    public float randomSoundJitter = 15f;
    [Tooltip("Distancia máxima al jugador para que el zombie haga ruido (22m cubre la pantalla)")]
    public float maxAudibleDistance = 22f;

    [Header("Sonido de Muerte")]
    public AudioClip deathSound;

    private bool movingRight = false;
    private float timer = 0f;
    private float lastFlipTime = 0f;
    private bool isDying = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private float nextAmbientSoundTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    private void Start()
    {
        movingRight = startMovingRight;
        timer = 0f;
        UpdateVisualOrientation();

        // Desfase aleatorio para el primer sonido (entre 1.5s y minSoundInterval)
        // para que no todos los zombies empiecen sonando al unísono
        nextAmbientSoundTime = Time.time + Random.Range(1.5f, minSoundInterval);

        if (chasePlayer)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    private void Update()
    {
        if (isDying) return;

        // Temporizador de sonido aleatorio desfasado
        if (Time.time >= nextAmbientSoundTime)
        {
            PlayAmbientSound();
            // Espera los 15s base + un tiempo aleatorio adicional
            nextAmbientSoundTime = Time.time + minSoundInterval + Random.Range(0f, randomSoundJitter);
        }
    }

    private void PlayAmbientSound()
    {
        if (isDying) return;

        // Comprobar distancia al jugador
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist > maxAudibleDistance) return;
        }

        AudioClip clipToPlay = ambientSound;
        if (randomSounds != null && randomSounds.Length > 0)
        {
            int idx = Random.Range(0, randomSounds.Length);
            if (randomSounds[idx] != null) clipToPlay = randomSounds[idx];
        }

        if (clipToPlay != null && audioSource != null)
        {
            audioSource.PlayOneShot(clipToPlay, ambientVolume);
        }
    }

    private void FixedUpdate()
    {
        if (isDying) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Si está en modo persecución, camina directamente hacia el jugador
        if (chasePlayer)
        {
            if (playerTransform == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTransform = p.transform;
            }

            if (playerTransform != null)
            {
                float dir = Mathf.Sign(playerTransform.position.x - transform.position.x);
                movingRight = dir > 0;
                rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
                UpdateVisualOrientation();
            }
            return;
        }

        // 1. Movimiento constante horizontal manteniendo la gravedad vertical
        rb.linearVelocity = new Vector2(movingRight ? speed : -speed, rb.linearVelocity.y);

        // 2. Temporizador para cambiar de dirección suavemente
        timer += Time.fixedDeltaTime;
        if (timer >= patrolTime)
        {
            timer = 0f;
            FlipDirection();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDying) return;

        // Caso A: Colisión con el Jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            // Si el jugador está atacando con su espada en ese instante
            if (player != null && player.isAttacking)
            {
                Die();
                return;
            }

            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Si el jugador cae desde arriba sobre su cabeza (pisotón)
                if (contact.normal.y < -0.4f)
                {
                    if (player != null) player.Bounce();
                    Die();
                    return;
                }
            }

            // Si lo toca de lado y no está invulnerable, pierde 1 vida
            if (!player.isInvulnerable)
            {
                GameManager.Instance?.TakeDamage(1, "¡TE ALCANZÓ UN ZOMBIE!");
            }
            return;
        }

        // Caso B: Colisión con una pared lateral o tope (evita atascarse)
        if (Time.time > lastFlipTime + 0.5f)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Si el impacto fue horizontal (contra una pared o tope)
                if (Mathf.Abs(contact.normal.x) > 0.6f)
                {
                    timer = 0f;
                    FlipDirection();
                    break;
                }
            }
        }
    }

    public void FlipDirection()
    {
        // Evitar giros múltiples en el mismo instante
        if (Time.time < lastFlipTime + 0.3f) return;
        lastFlipTime = Time.time;

        movingRight = !movingRight;
        UpdateVisualOrientation();
    }

    private void UpdateVisualOrientation()
    {
        if (spriteRenderer != null)
        {
            // El sprite base del Zombie mira hacia la izquierda:
            // Si camina a la derecha, activamos flipX; si va a la izquierda, lo desactivamos.
            spriteRenderer.flipX = movingRight;
        }
    }

    public void TakeDamage()
    {
        Die();
    }

    public void Die()
    {
        if (isDying) return;
        isDying = true;

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound, ambientVolume);
        }
        else
        {
            AudioManager.Instance?.PlaySFX("EnemyHit");
        }

        // Desactivar colisiones para que no dañe al jugador mientras muere
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Detener animación de caminata si existe
        SimpleSpriteAnimator anim = GetComponent<SimpleSpriteAnimator>();
        if (anim != null) anim.Stop();

        if (deathFrames != null && deathFrames.Length > 0)
        {
            StartCoroutine(PlayDeathRoutine());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator PlayDeathRoutine()
    {
        float frameDuration = 1f / Mathf.Max(1f, deathFps);
        for (int i = 0; i < deathFrames.Length; i++)
        {
            if (spriteRenderer != null && deathFrames[i] != null)
            {
                spriteRenderer.sprite = deathFrames[i];
            }
            yield return new WaitForSeconds(frameDuration);
        }

        yield return new WaitForSeconds(0.15f);
        Destroy(gameObject);
    }
}
