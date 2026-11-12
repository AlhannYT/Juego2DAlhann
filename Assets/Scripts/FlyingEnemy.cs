using UnityEngine;
using System.Collections;

/// <summary>
/// Enemigo Volador (Murciélago / Espectro / Shuriken flotante).
/// Oscila en el aire vertical u horizontalmente mediante una función senoidal.
/// Reproduce sonidos ambientales aleatorios desfasados y animación de muerte al destruirse.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FlyingEnemy : MonoBehaviour
{
    [Header("Comportamiento")]
    public float moveSpeed = 2f;
    public float amplitude = 1.5f; // Distancia de oscilación
    public float frequency = 2f;  // Velocidad de oscilación
    public bool oscillateVertical = true;

    [Header("Animación de Muerte")]
    public Sprite[] deathFrames;
    public float deathFps = 12f;

    [Header("Sonidos de Murciélago")]
    [Tooltip("Sonido aleatorio del murciélago (chillido/aleteo)")]
    public AudioClip ambientSound;
    [Tooltip("Lista opcional de varios sonidos para mayor variedad aleatoria")]
    public AudioClip[] randomSounds;
    [Range(0f, 1f)] public float ambientVolume = 0.8f;
    [Tooltip("Tiempo base de espera entre sonidos (por defecto 15 segundos)")]
    public float minSoundInterval = 15f;
    [Tooltip("Variación aleatoria adicional (0 a randomSoundJitter segundos)")]
    public float randomSoundJitter = 15f;
    [Tooltip("Distancia máxima al jugador para que el murciélago se escuche")]
    public float maxAudibleDistance = 22f;

    [Header("Sonido de Muerte")]
    public AudioClip deathSound;

    private Vector3 startPosition;
    private bool isDying = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private float nextAmbientSoundTime = 0f;
    private Transform playerTransform;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    private void Start()
    {
        startPosition = transform.position;

        // Desfase aleatorio para el primer sonido (entre 1.5s y minSoundInterval)
        // para que no todos los murciélagos comiencen a chillar al mismo tiempo
        nextAmbientSoundTime = Time.time + Random.Range(1.5f, minSoundInterval);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    private void Update()
    {
        if (isDying) return;

        // Movimiento de oscilación senoidal
        float offset = Mathf.Sin(Time.time * frequency) * amplitude;

        if (oscillateVertical)
        {
            transform.position = startPosition + new Vector3(0f, offset, 0f);
        }
        else
        {
            transform.position = startPosition + new Vector3(offset, 0f, 0f);
        }

        // Temporizador de sonido aleatorio desfasado
        if (Time.time >= nextAmbientSoundTime)
        {
            PlayAmbientSound();
            // Espera los 15s base + tiempo aleatorio adicional
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDying) return;

        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            // Si el jugador está atacando con la espada
            if (player != null && player.isAttacking)
            {
                Die();
                return;
            }

            // Si el jugador viene cayendo sobre él (pisotón)
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.linearVelocity.y < -0.1f && collision.transform.position.y > transform.position.y)
            {
                player?.Bounce();
                Die();
            }
            else if (!player.isInvulnerable)
            {
                GameManager.Instance?.TakeDamage(1, "¡UN ENEMIGO VOLADOR TE ALCANZÓ!");
            }
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

        // Desactivar colisiones para que no dañe al jugador al morir
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Detener animación de vuelo
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

    private IEnumerator PlayDeathRoutine()
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
