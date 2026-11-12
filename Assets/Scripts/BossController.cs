using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador del Jefe Final (Nivel 3).
/// Mecánicas:
/// - 100 HP con barra de vida deslizante (Slider) suave.
/// - Ciclo de combate: Acechar (caminar) -> Telegrafiar -> Embestir (atacar) -> Descanso vulnerable (3s).
/// - Conexión directa con BossSpriteAnimator para todas las animaciones.
/// - Secuencia de victoria final al derrotarlo.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BossController : MonoBehaviour
{
    public enum BossState { Stalking, Telegraphing, Dashing, Resting, Dead }

    [Header("Estadísticas de Vida")]
    public int maxHealth = 100;
    public int currentHealth;
    public int damagePerHit = 20;

    [Header("Barra de Vida (Slider UI)")]
    public Slider healthSlider;
    public TMP_Text healthText;

    [Header("Pantalla de Victoria")]
    public GameObject victoryPanel;
    public TMP_Text victoryText;
    public TMP_Text coinsSummaryText;

    [Header("Recompensa Final (Yaroa)")]
    public GameObject yaroaPrefab;

    [Header("Invocación de Esbirros (Zombies)")]
    public GameObject zombiePrefab;
    public float minionSpeed = 3.2f; // Velocidad del zombie perseguidor (más rápido que el Boss)
    public int maxMinionsAtOnce = 2;
    private System.Collections.Generic.List<GameObject> activeMinions = new System.Collections.Generic.List<GameObject>();

    [Header("Tiempos del Ciclo de Combate")]
    public float attackInterval = 7.5f;
    public float telegraphDuration = 0.8f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.6f;
    public float restDuration = 3.0f;

    [Header("Velocidad Normal")]
    public float walkSpeed = 2.2f;

    [Header("Efectos de Sonido del Rey Esqueleto")]
    [Tooltip("Sonido de pasos o movimiento que se reproduce mientras el jefe camina")]
    public AudioClip walkSound;
    [Tooltip("Intervalo entre pasos si es un clip corto de pisada (en segundos)")]
    public float stepInterval = 0.45f;

    [Tooltip("Sonido al invocar a los zombies esbirros")]
    public AudioClip minionSpawnSound;

    [Tooltip("Sonido de advertencia cuando se prepara para atacar (telegrafiado)")]
    public AudioClip attackTelegraphSound;

    [Tooltip("Sonido de embestida / ataque")]
    public AudioClip attackDashSound;

    [Tooltip("Sonido al recibir daño")]
    public AudioClip hurtSound;

    [Tooltip("Sonido de derrota")]
    public AudioClip deathSound;

    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    private BossState currentState = BossState.Stalking;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BossSpriteAnimator bossAnimator;
    private Transform playerTransform;
    private AudioSource audioSource;
    private AudioSource walkAudioSource;
    private float nextStepTime = 0f;

    private bool isVulnerable = false;
    private bool wasHitInThisRest = false;
    private float targetHealthValue;
    private bool battleStarted = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossAnimator = GetComponent<BossSpriteAnimator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        walkAudioSource = gameObject.AddComponent<AudioSource>();
        walkAudioSource.playOnAwake = false;
        walkAudioSource.spatialBlend = 0f;
        walkAudioSource.loop = true;

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        targetHealthValue = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        UpdateHealthUI();

        if (victoryPanel != null) victoryPanel.SetActive(false);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        // Si existe pantalla de introducción de mecánicas, esperar a que el jugador confirme
        BossIntroController intro = FindFirstObjectByType<BossIntroController>();
        if (intro == null)
        {
            StartBattleFromIntro();
        }
        else
        {
            bossAnimator?.PlayIdle();
            StopWalkingSound();
        }
    }

    public void StartBattleFromIntro()
    {
        if (battleStarted) return;
        battleStarted = true;

        AudioManager.Instance?.PlayMusic("BossMusic");
        StartCoroutine(BossBattleRoutine());
    }

    private void Update()
    {
        // Si el juego está en pausa, silenciar pasos y congelar lógica
        if (Time.timeScale == 0f)
        {
            StopWalkingSound();
            return;
        }

        // Vaciado suave de la barra de vida
        if (healthSlider != null && Mathf.Abs(healthSlider.value - targetHealthValue) > 0.05f)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealthValue, Time.deltaTime * 5f);
        }

        if (currentState == BossState.Dead || (GameManager.Instance != null && GameManager.Instance.isGameOver))
        {
            rb.linearVelocity = Vector2.zero;
            StopWalkingSound();
            return;
        }

        // Sonido de pasos continuo mientras camina hacia el jugador
        if (currentState == BossState.Stalking)
        {
            UpdateWalkingSound();
        }
        else
        {
            StopWalkingSound();
        }

        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            return;
        }

        // Orientar el sprite hacia el jugador
        if (currentState != BossState.Dashing && spriteRenderer != null)
        {
            float dirToPlayer = playerTransform.position.x - transform.position.x;
            spriteRenderer.flipX = dirToPlayer < 0;
        }
    }

    private void StartWalkingSound()
    {
        if (walkSound == null || walkAudioSource == null) return;

        if (walkSound.length >= 0.9f)
        {
            walkAudioSource.clip = walkSound;
            walkAudioSource.loop = true;
            walkAudioSource.volume = sfxVolume;
            if (!walkAudioSource.isPlaying) walkAudioSource.Play();
        }
        else
        {
            walkAudioSource.loop = false;
            walkAudioSource.PlayOneShot(walkSound, sfxVolume);
            nextStepTime = Time.time + stepInterval;
        }
    }

    private void UpdateWalkingSound()
    {
        if (currentState != BossState.Stalking || walkSound == null || walkAudioSource == null)
        {
            StopWalkingSound();
            return;
        }

        if (walkSound.length < 0.9f)
        {
            if (Time.time >= nextStepTime)
            {
                walkAudioSource.PlayOneShot(walkSound, sfxVolume);
                nextStepTime = Time.time + stepInterval;
            }
        }
        else
        {
            if (!walkAudioSource.isPlaying)
            {
                walkAudioSource.clip = walkSound;
                walkAudioSource.loop = true;
                walkAudioSource.volume = sfxVolume;
                walkAudioSource.Play();
            }
        }
    }

    public void StopWalkingSound()
    {
        if (walkAudioSource != null && walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }
    }

    private IEnumerator BossBattleRoutine()
    {
        while (currentHealth > 0)
        {
            // FASE 1: Acechar / Caminar hacia el jugador (7.5s)
            currentState = BossState.Stalking;
            isVulnerable = false;
            wasHitInThisRest = false;
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            bossAnimator?.PlayWalk();
            StartWalkingSound();

            // Invocar 2 zombies perseguidores mientras se acerca al jugador
            SpawnMinions();

            float stalkTimer = 0f;
            while (stalkTimer < attackInterval)
            {
                if (currentHealth <= 0) yield break;

                if (playerTransform != null)
                {
                    float dir = Mathf.Sign(playerTransform.position.x - transform.position.x);
                    rb.linearVelocity = new Vector2(dir * walkSpeed, rb.linearVelocity.y);
                }

                stalkTimer += Time.deltaTime;
                yield return null;
            }

            // FASE 2: Telegrafiar / Advertencia (parpadeo de aviso y sonido de alerta)
            StopWalkingSound();
            currentState = BossState.Telegraphing;
            rb.linearVelocity = Vector2.zero;
            bossAnimator?.PlayIdle();

            if (attackTelegraphSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackTelegraphSound, sfxVolume);
            }

            for (int i = 0; i < 4; i++)
            {
                if (spriteRenderer != null) spriteRenderer.color = Color.yellow;
                yield return new WaitForSeconds(telegraphDuration / 8f);
                if (spriteRenderer != null) spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(telegraphDuration / 8f);
            }

            // FASE 3: Embestida de Ataque
            currentState = BossState.Dashing;
            bossAnimator?.PlayAttack();

            if (attackDashSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackDashSound, sfxVolume);
            }
            else
            {
                AudioManager.Instance?.PlaySFX("Attack");
            }

            float dashDir = playerTransform != null ? Mathf.Sign(playerTransform.position.x - transform.position.x) : 1f;
            rb.linearVelocity = new Vector2(dashDir * dashSpeed, rb.linearVelocity.y);
            yield return new WaitForSeconds(dashDuration);

            // Frenar tras la embestida
            rb.linearVelocity = Vector2.zero;

            // FASE 4: Descanso / Estado Vulnerable (3 segundos)
            currentState = BossState.Resting;
            isVulnerable = true;
            wasHitInThisRest = false;
            bossAnimator?.PlayIdle();

            float restTimer = 0f;
            while (restTimer < restDuration)
            {
                if (currentHealth <= 0) yield break;

                // Tinte azul cian pulsante indicando fatiga y vulnerabilidad
                if (!wasHitInThisRest && spriteRenderer != null)
                {
                    float pulse = Mathf.PingPong(Time.time * 4f, 0.4f) + 0.6f;
                    spriteRenderer.color = new Color(0.4f, 0.8f, 1f, pulse);
                }

                restTimer += Time.deltaTime;
                yield return null;
            }

            isVulnerable = false;
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == BossState.Dead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            // Si el jugador está ejecutando el Dash o está invulnerable, ignorar por completo el daño
            if (player != null && (player.isDashing || player.isInvulnerable))
            {
                return;
            }

            // Pisotón en la cabeza durante el descanso
            if (isVulnerable && !wasHitInThisRest)
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y < -0.4f)
                    {
                        player?.Bounce();
                        TakeDamage(damagePerHit);
                        return;
                    }
                }
            }

            // REGLA CLAVE: Durante el descanso de 3 segundos (guardia baja), el Jefe NUNCA hace daño al jugador
            if (isVulnerable || currentState == BossState.Resting)
            {
                return;
            }

            // Daño al jugador únicamente si el Jefe está activo (acechando o embistiendo)
            if (player != null && !player.isInvulnerable && !player.isDashing)
            {
                GameManager.Instance?.TakeDamage(1, "¡EL JEFE FINAL TE HA GOLPEADO!");
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == BossState.Dead || isVulnerable || currentState == BossState.Resting) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && !player.isInvulnerable && !player.isDashing)
            {
                GameManager.Instance?.TakeDamage(1, "¡EL JEFE FINAL TE HA GOLPEADO!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentState == BossState.Dead) return;

        // Si no está vulnerable, el golpe es bloqueado
        if (!isVulnerable)
        {
            AudioManager.Instance?.PlaySFX("BlockHit");
            return;
        }

        if (wasHitInThisRest) return;
        wasHitInThisRest = true;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        targetHealthValue = currentHealth;

        UpdateHealthUI();
        bossAnimator?.PlayHurt();

        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound, sfxVolume);
        }
        else
        {
            AudioManager.Instance?.PlaySFX("BossHurt");
        }

        StartCoroutine(HitFeedbackRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFeedbackRoutine()
    {
        if (spriteRenderer != null)
        {
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"JEFE: {currentHealth} / {maxHealth} HP";
        }
    }

    private void SpawnMinions()
    {
        if (zombiePrefab == null) return;

        // Limpiar esbirros ya derrotados o destruidos
        activeMinions.RemoveAll(item => item == null);

        // Si ya hay 2 zombies activos en la arena, no generar más para mantener balanceada la pelea
        if (activeMinions.Count >= maxMinionsAtOnce) return;

        if (minionSpawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(minionSpawnSound, sfxVolume);
        }

        // 1. Obtener la altura del suelo de forma sólida y exacta buscando ÚNICAMENTE la capa Ground
        int groundMask = LayerMask.GetMask("Ground");
        if (groundMask == 0) groundMask = ~LayerMask.GetMask("Enemy", "Ignore Raycast");

        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + 1f), Vector2.down, 15f, groundMask);

        Collider2D[] bossColliders = GetComponentsInChildren<Collider2D>();
        float floorY = 0f;
        if (hit.collider != null)
        {
            floorY = hit.point.y;
        }
        else if (bossColliders.Length > 0 && bossColliders[0] != null)
        {
            floorY = bossColliders[0].bounds.min.y;
        }

        // El BoxCollider2D del Zombie mide 1.24m de alto (escala 4).
        // Al sumar 0.62m (su mitad exacta), los pies quedan perfectamente apoyados sobre el suelo:
        // NUNCA flotará a la altura del hombro ni se hundirá en el piso.
        float spawnY = floorY + 0.62f;

        // Dirección hacia donde está el jugador respecto al jefe
        float dirToPlayer = playerTransform != null ? Mathf.Sign(playerTransform.position.x - transform.position.x) : -1f;

        int toSpawn = maxMinionsAtOnce - activeMinions.Count;

        for (int i = 0; i < toSpawn; i++)
        {
            // Aparecen detrás del jefe (o a su espalda):
            // 1. Evita que aparezcan sorpresivamente en la cara del jugador mientras esquiva.
            // 2. Corren desde atrás pasando al esqueleto hacia el jugador.
            float offsetDistance = (i == 0) ? 1.6f : 2.8f;
            float spawnX = transform.position.x - (dirToPlayer * offsetDistance);

            // Mantener dentro de los límites de la arena
            spawnX = Mathf.Clamp(spawnX, -7f, 45f);

            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            GameObject minion = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);

            // REGLA CLAVE: El zombie y el Jefe NUNCA colisionan entre sí bajo ninguna circunstancia
            Collider2D[] minionColliders = minion.GetComponentsInChildren<Collider2D>();
            foreach (var bCol in bossColliders)
            {
                foreach (var mCol in minionColliders)
                {
                    if (bCol != null && mCol != null)
                    {
                        Physics2D.IgnoreCollision(mCol, bCol, true);
                    }
                }
            }

            EnemyPatrol patrol = minion.GetComponent<EnemyPatrol>();
            if (patrol != null)
            {
                patrol.chasePlayer = true;
                patrol.speed = minionSpeed; // 3.2f
                StartCoroutine(MinionSpawnGraceRoutine(minion));
            }

            activeMinions.Add(minion);
        }
    }

    private IEnumerator MinionSpawnGraceRoutine(GameObject minion)
    {
        if (minion == null) yield break;
        Collider2D[] minionCols = minion.GetComponentsInChildren<Collider2D>();
        SpriteRenderer sr = minion.GetComponent<SpriteRenderer>();

        // Ignorar colisión con el jugador durante los primeros instantes para dar tiempo de reacción
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Collider2D playerCol = playerObj != null ? playerObj.GetComponent<Collider2D>() : null;

        if (playerCol != null)
        {
            foreach (var mCol in minionCols)
            {
                if (mCol != null) Physics2D.IgnoreCollision(mCol, playerCol, true);
            }
        }

        // Tinte semi-transparente como advertencia visual de invocación
        if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.45f);

        yield return new WaitForSeconds(0.7f);

        if (minion != null)
        {
            if (playerCol != null)
            {
                foreach (var mCol in minionCols)
                {
                    if (mCol != null) Physics2D.IgnoreCollision(mCol, playerCol, false);
                }
            }
            if (sr != null) sr.color = Color.white;
        }
    }

    private void Die()
    {
        currentState = BossState.Dead;
        StopAllCoroutines();
        StopWalkingSound();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Destruir automáticamente a todos los esbirros invocados cuando el jefe muere
        foreach (GameObject minion in activeMinions)
        {
            if (minion != null) Destroy(minion);
        }
        activeMinions.Clear();

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        bossAnimator?.PlayDie();

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound, sfxVolume);
        }
        else
        {
            AudioManager.Instance?.PlaySFX("BossDefeated");
        }

        // Ocultar la barra de vida del jefe al caer
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);

        // Desovar la Yaroa como trofeo final para que el jugador la recoja
        SpawnYaroaGoal();
    }

    private void SpawnYaroaGoal()
    {
        // 1. Obtener la posición del jugador y la dirección hacia donde mira
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector3 playerPos = playerObj != null ? playerObj.transform.position : transform.position;
        SpriteRenderer playerSr = playerObj != null ? playerObj.GetComponent<SpriteRenderer>() : null;

        // En PlayerController: flipX == true es Izquierda (-1), flipX == false es Derecha (+1)
        float playerFacingDir = (playerSr != null && playerSr.flipX) ? -1f : 1f;

        // 2. Destino de la Yaroa: 4.5 metros AL FRENTE de hacia donde mira el jugador
        float targetX = playerPos.x + (playerFacingDir * 4.5f);

        // Si el jugador está mirando hacia el muro del límite de la arena, reorientar hacia el centro
        if (targetX < -5f) targetX = playerPos.x + 4.5f;
        if (targetX > 43f) targetX = playerPos.x - 4.5f;
        targetX = Mathf.Clamp(targetX, -5f, 43f);

        // 3. Detectar la altura del suelo en el punto de aterrizaje
        int groundMask = LayerMask.GetMask("Ground");
        if (groundMask == 0) groundMask = ~LayerMask.GetMask("Enemy", "Ignore Raycast");

        RaycastHit2D hit = Physics2D.Raycast(new Vector2(targetX, playerPos.y + 2f), Vector2.down, 15f, groundMask);
        float floorY = (hit.collider != null) ? hit.point.y : 0f;
        Vector3 targetLandingPos = new Vector3(targetX, floorY + 0.9f, 0f);

        // Punto de origen del lanzamiento: desde el pecho del jefe
        Vector3 launchOrigin = new Vector3(transform.position.x, transform.position.y + 0.8f, 0f);

        GameObject yaroaObj = null;

        if (yaroaPrefab != null)
        {
            yaroaObj = Instantiate(yaroaPrefab, launchOrigin, Quaternion.identity);
        }
        else
        {
            yaroaObj = new GameObject("YaroaGoal");
            yaroaObj.transform.position = launchOrigin;
            yaroaObj.transform.localScale = new Vector3(3.5f, 3.5f, 1f);

            SpriteRenderer sr = yaroaObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (var s in sprites)
            {
                if (s != null && s.name.Equals("Yaroa_0", System.StringComparison.OrdinalIgnoreCase))
                {
                    sr.sprite = s;
                    break;
                }
            }

            CircleCollider2D circleCol = yaroaObj.AddComponent<CircleCollider2D>();
            circleCol.isTrigger = true;
            circleCol.radius = 0.5f;

            yaroaObj.AddComponent<YaroaGoal>();
        }

        YaroaGoal yGoal = yaroaObj.GetComponent<YaroaGoal>();
        if (yGoal != null)
        {
            yGoal.victoryPanel = victoryPanel;
            yGoal.victoryText = victoryText;
            yGoal.coinsSummaryText = coinsSummaryText;
            // Lanzar la Yaroa en un arco hacia adelante para que aterrice frente al jugador
            yGoal.LaunchTowards(launchOrigin, targetLandingPos);
        }

        Debug.Log($"[BossController] ¡Jefe derrotado! Lanzando la Yaroa Sagrada hacia X={targetX}");
    }
}
