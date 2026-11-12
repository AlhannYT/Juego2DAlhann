using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controlador del Jugador (Samurai 2D).
/// Soporta movimiento, salto, rebote sobre enemigos (pisotón), ataque con espada e interacción con bloques.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Parámetros de Movimiento")]
    public float moveSpeed = 7f;
    public float jumpForce = 13f;
    public float bounceForce = 10f; // Impulso al caer sobre un enemigo

    [Header("Gravedad y Peso Vertical (Sensación de Salto)")]
    [Tooltip("Multiplicador de gravedad al caer hacia abajo (2.4 a 3.0 hace que caiga rápido con peso y no flote en el aire)")]
    public float fallGravityMultiplier = 2.6f;
    [Tooltip("Multiplicador de gravedad en la subida del salto (1.0 a 1.25 hace que el arco sea firme)")]
    public float upwardGravityMultiplier = 1.15f;
    [Tooltip("Control horizontal mientras está en el aire (1 = control completo, 0.85 = menor maniobra aérea)")]
    [Range(0.4f, 1f)] public float airControl = 0.85f;
    [Tooltip("Velocidad máxima de caída vertical")]
    public float maxFallSpeed = 22f;
    private float defaultGravityScale = 1f;

    [Header("Ataque de Espada")]
    public Transform attackPoint;
    public float attackRange = 0.8f;
    public LayerMask enemyLayers;
    public float attackCooldown = 0.4f;
    private float nextAttackTime = 0f;

    [Header("Efectos de Sonido del Jugador")]
    [Tooltip("Sonido del espadazo (se reproduce respetando el cooldown de ataque)")]
    public AudioClip attackSound;
    [Tooltip("Sonido opcional para el salto")]
    public AudioClip jumpSound;
    [Tooltip("Sonido opcional para la esquiva / dash")]
    public AudioClip dashSound;
    [Range(0f, 1f)] public float soundVolume = 1.0f;
    private AudioSource playerAudioSource;

    [Header("Habilidad de Dash (Esquiva con tecla K)")]
    public float dashSpeed = 22f;
    public float dashDuration = 0.35f;
    public float dashCooldown = 1.5f;
    public float postDashGraceTime = 0.25f; // Tiempo de gracia invulnerable al salir del dash
    public bool isDashing { get; private set; } = false;
    private float nextDashTime = 0f;

    [Header("Chequeo de Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerSpriteAnimator spriteAnimator;

    private float horizontalInput = 0f;
    private bool jumpRequested = false;
    private bool isGrounded = false;
    public bool isAttacking { get; private set; } = false;
    public bool isInvulnerable { get; private set; } = false;
    private Vector3 spawnPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        spriteAnimator = GetComponent<PlayerSpriteAnimator>();

        playerAudioSource = GetComponent<AudioSource>();
        if (playerAudioSource == null)
        {
            playerAudioSource = gameObject.AddComponent<AudioSource>();
            playerAudioSource.playOnAwake = false;
            playerAudioSource.spatialBlend = 0f;
        }

        // Bloquear rotación en Z para que el personaje nunca se tambalee ni se caiga
        if (rb != null)
        {
            defaultGravityScale = rb.gravityScale > 0 ? rb.gravityScale : 1f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        // Material físico sin fricción para evitar que el personaje se quede pegado a las paredes verticales
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            PhysicsMaterial2D noFrictionMat = new PhysicsMaterial2D("PlayerNoFriction")
            {
                friction = 0f,
                bounciness = 0f
            };
            col.sharedMaterial = noFrictionMat;
        }
    }

    private void Update()
    {
        // Si el juego está en pausa, congelar animación/física y no procesar ataques ni movimiento
        if (Time.timeScale == 0f) return;

        if (GameManager.Instance != null && (GameManager.Instance.isGameOver || GameManager.Instance.isLevelComplete))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        ReadInputs();

        // Control de orientación (Flip)
        if (horizontalInput > 0.05f)
        {
            spriteRenderer.flipX = false;
            if (attackPoint != null) attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, 0f);
        }
        else if (horizontalInput < -0.05f)
        {
            spriteRenderer.flipX = true;
            if (attackPoint != null) attackPoint.localPosition = new Vector3(-Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, 0f);
        }

        // Actualizar parámetros de animación
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        }
    }

    private void ReadInputs()
    {
        horizontalInput = 0f;

#if ENABLE_INPUT_SYSTEM
        // Nuevo Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput += 1f;

            if ((Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) && isGrounded)
            {
                jumpRequested = true;
            }

            if (Time.timeScale > 0f && (Keyboard.current.jKey.wasPressedThisFrame || Keyboard.current.zKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame) && Time.time >= nextAttackTime)
            {
                Attack();
            }

            // Dash con la tecla K
            if (Keyboard.current.kKey.wasPressedThisFrame && Time.time >= nextDashTime && !isDashing)
            {
                StartCoroutine(DashRoutine());
            }
        }
#endif

        // Soporte de reserva para Input Manager tradicional si está activo
        try
        {
            float legacyH = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(legacyH) > 0.01f) horizontalInput = legacyH;

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                jumpRequested = true;
            }

            // Ataque con botón Fire1: solo si el cursor NO está sobre ningún botón o elemento de interfaz
            bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (Input.GetButtonDown("Fire1") && !isOverUI && Time.timeScale > 0f && Time.time >= nextAttackTime)
            {
                Attack();
            }

            if (Input.GetKeyDown(KeyCode.K) && Time.time >= nextDashTime && !isDashing)
            {
                StartCoroutine(DashRoutine());
            }
        }
        catch { /* El nuevo Input System puede arrojar advertencia si el legacy está desactivado; se ignora con seguridad */ }
    }

    private void FixedUpdate()
    {
        // Si está ejecutando el Dash, no sobreescribir la velocidad horizontal
        if (isDashing) return;

        // Verificar si está tocando el suelo de forma inteligente y segura
        isGrounded = CheckIsGrounded();

        // Aplicar movimiento horizontal con ajuste en el aire para evitar flotar
        float currentSpeed = isGrounded ? moveSpeed : (moveSpeed * airControl);
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);

        // Aplicar salto
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpRequested = false;

            if (jumpSound != null && playerAudioSource != null)
            {
                playerAudioSource.PlayOneShot(jumpSound, soundVolume);
            }
            else
            {
                AudioManager.Instance?.PlaySFX("Jump");
            }
        }

        // Modulación de gravedad para dar peso vertical real al caer
        ApplyVerticalWeight();

        // Asistencia en esquinas y bordes
        CheckLedgeAssist();
    }

    /// <summary>
    /// Modula dinámicamente la escala de gravedad en el aire.
    /// El salto se eleva normalmente, pero en cuanto el personaje llega al punto más alto
    /// y comienza a caer, la gravedad aumenta para darle peso real hacia abajo y cortar la flotabilidad.
    /// </summary>
    private void ApplyVerticalWeight()
    {
        if (rb == null) return;

        if (isGrounded)
        {
            rb.gravityScale = defaultGravityScale;
        }
        else
        {
            // Si está cayendo o en el ápice del salto (velocidad Y menor o igual a 0.1)
            if (rb.linearVelocity.y <= 0.1f)
            {
                rb.gravityScale = defaultGravityScale * fallGravityMultiplier;
            }
            // Si está ascendiendo durante el salto
            else
            {
                rb.gravityScale = defaultGravityScale * upwardGravityMultiplier;
            }

            // Límite de velocidad terminal de caída por seguridad
            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
            }
        }
    }

    private System.Collections.IEnumerator DashRoutine()
    {
        isDashing = true;
        isInvulnerable = true;
        nextDashTime = Time.time + dashCooldown;

        if (dashSound != null && playerAudioSource != null)
        {
            playerAudioSource.PlayOneShot(dashSound, soundVolume);
        }

        // Dirección hacia donde mira el Samurái
        float dashDir = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;

        // Desactivar colisión física únicamente con los enemigos (para atravesar al jefe y zombies)
        Collider2D playerCollider = GetComponent<Collider2D>();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (playerCollider != null)
        {
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                {
                    Collider2D enemyCol = enemy.GetComponent<Collider2D>();
                    if (enemyCol != null) Physics2D.IgnoreCollision(playerCollider, enemyCol, true);
                }
            }
        }

        // Efecto visual semi-transparente estilo sombra ninja
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.4f, 0.9f, 1f, 0.45f);
        }

        // Desplazamiento rápido manteniendo el contacto firme con el suelo (Ground)
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = new Vector2(dashDir * dashSpeed, rb.linearVelocity.y * 0.1f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ventana de gracia post-dash: mantienes la invulnerabilidad para garantizar que ya cruzaste al jefe por completo
        yield return new WaitForSeconds(postDashGraceTime);

        // Restaurar estado normal y reactivar colisiones con enemigos
        if (playerCollider != null)
        {
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                {
                    Collider2D enemyCol = enemy.GetComponent<Collider2D>();
                    if (enemyCol != null) Physics2D.IgnoreCollision(playerCollider, enemyCol, false);
                }
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        isDashing = false;
        isInvulnerable = false;
    }

    private bool CheckIsGrounded()
    {
        // REGLA CLAVE: Si el personaje va subiendo en el aire, NUNCA está en el suelo.
        // Esto elimina el bug de las esquinas que te impulsaba hacia arriba más de lo debido.
        if (rb != null && rb.linearVelocity.y > 0.1f)
        {
            return false;
        }

        Vector2 checkPos = groundCheck != null 
            ? (Vector2)groundCheck.position 
            : (Vector2)transform.position + Vector2.down * 0.9f;

        // 1. Chequeo por capa configurada (si está asignada)
        if (groundLayer.value != 0 && Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer))
        {
            return true;
        }

        // 2. Fallback universal: detectar cualquier superficie sólida debajo
        Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPos, groundCheckRadius);
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject != gameObject && !col.isTrigger && !col.CompareTag("Player") && !col.CompareTag("Enemy"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Asistencia de borde (Ledge Assist): Si roza la esquina superior por pocos píxeles,
    /// lo acomoda suavemente sobre la plataforma en vez de rebotar o trabarse.
    /// </summary>
    private void CheckLedgeAssist()
    {
        if (rb == null || Mathf.Abs(horizontalInput) < 0.1f) return;

        float facingDir = horizontalInput > 0 ? 1f : -1f;
        Vector2 waistPos = (Vector2)transform.position + new Vector2(0f, 0.1f);
        Vector2 feetPos = (Vector2)transform.position + new Vector2(0f, -0.6f);

        // Raycast al frente a la altura de los pies y a la altura de la cintura
        RaycastHit2D feetHit = Physics2D.Raycast(feetPos, Vector2.right * facingDir, 0.45f, ~LayerMask.GetMask("Player", "Enemy", "Ignore Raycast"));
        RaycastHit2D waistHit = Physics2D.Raycast(waistPos, Vector2.right * facingDir, 0.45f, ~LayerMask.GetMask("Player", "Enemy", "Ignore Raycast"));

        // Si los pies chocan con la esquina pero la cintura está libre, lo subimos suavemente al escalón
        if (feetHit.collider != null && !feetHit.collider.isTrigger && waistHit.collider == null && rb.linearVelocity.y <= 0.1f)
        {
            transform.position = new Vector3(transform.position.x + (facingDir * 0.05f), transform.position.y + 0.12f, transform.position.z);
        }
    }

    public void SetAttackInputDelay(float delay)
    {
        nextAttackTime = Time.time + delay;
    }

    private void Attack()
    {
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        if (spriteAnimator != null)
        {
            spriteAnimator.TriggerAttack();
        }

        if (attackSound != null && playerAudioSource != null)
        {
            playerAudioSource.PlayOneShot(attackSound, soundVolume);
        }
        else
        {
            AudioManager.Instance?.PlaySFX("Attack");
        }

        // Detección automática del ataque con espada en frente del jugador
        Vector2 attackOrigin;
        if (attackPoint != null)
        {
            attackOrigin = attackPoint.position;
        }
        else
        {
            // Si attackPoint no está asignado, calculamos 1 metro al frente según hacia dónde mira el sprite
            float facingDir = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
            attackOrigin = (Vector2)transform.position + new Vector2(facingDir * 0.9f, 0.1f);
        }

        // Buscar todos los objetos dentro del alcance del espadazo (por capas o por Tag)
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin, attackRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // Si es un enemigo (por Tag o componente)
            if (hit.CompareTag("Enemy") || hit.GetComponent<EnemyPatrol>() != null || hit.GetComponent<FlyingEnemy>() != null || hit.GetComponent<BossController>() != null)
            {
                EnemyPatrol patrol = hit.GetComponent<EnemyPatrol>();
                if (patrol != null) patrol.TakeDamage();

                FlyingEnemy flying = hit.GetComponent<FlyingEnemy>();
                if (flying != null) flying.TakeDamage();

                BossController boss = hit.GetComponent<BossController>();
                if (boss != null) boss.TakeDamage(20);
            }
        }

        Invoke(nameof(ResetAttackState), attackCooldown);
    }

    private void ResetAttackState()
    {
        isAttacking = false;
    }

    /// <summary>
    /// Si el jugador está atacando con la espada y colisiona con el enemigo, el enemigo muere primero
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isAttacking && collision.gameObject.CompareTag("Enemy"))
        {
            EnemyPatrol patrol = collision.gameObject.GetComponent<EnemyPatrol>();
            if (patrol != null) patrol.TakeDamage();

            BossController boss = collision.gameObject.GetComponent<BossController>();
            if (boss != null) boss.TakeDamage(20);
        }
    }

    /// <summary>
    /// Rebote automático al pisar a un enemigo estilo Mario.
    /// </summary>
    public void Bounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
        AudioManager.Instance?.PlaySFX("Stomp");
    }

    private void Start()
    {
        spawnPosition = transform.position;
    }

    public void RespawnAtStart()
    {
        transform.position = spawnPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = defaultGravityScale;
        }
        TriggerDamageFeedback();
    }

    public void PlayHurtAnimation()
    {
        if (spriteAnimator != null)
        {
            spriteAnimator.TriggerHurt();
        }
    }

    public void TriggerDamageFeedback()
    {
        if (isInvulnerable) return;

        PlayHurtAnimation();

        // Retroceso hacia atrás (lado contrario de donde está mirando el personaje) sin saltar
        if (rb != null)
        {
            float facingDir = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
            float pushBackForce = 4.5f;
            rb.linearVelocity = new Vector2(-facingDir * pushBackForce, 0f);
        }

        StartCoroutine(InvulnerabilityRoutine());
    }

    private System.Collections.IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        if (spriteRenderer != null)
        {
            for (int i = 0; i < 6; i++)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
                yield return new WaitForSeconds(0.12f);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.12f);
            }
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        isInvulnerable = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Detectar si golpea un bloque desde abajo (cabeceo de bloques estilo Mario)
        if (collision.gameObject.CompareTag("BreakableBlock"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Si el contacto viene desde la cabeza del personaje (normal hacia abajo)
                if (contact.normal.y < -0.5f)
                {
                    BreakableBlock block = collision.gameObject.GetComponent<BreakableBlock>();
                    if (block != null)
                    {
                        block.HitFromBelow();
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
