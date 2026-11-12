using UnityEngine;

/// <summary>
/// Animador por sprites ligero y automático para el Samurai.
/// Controla transiciones entre Reposo (IDLE), Correr (RUN), Ataque (ATTACK) y Daño (HURT)
/// sin necesidad de configurar Animator Controllers complejos en Unity.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    public enum PlayerAnimState { Idle, Run, Attack, Hurt }

    [Header("Colecciones de Fotogramas")]
    public Sprite[] idleFrames;
    public Sprite[] runFrames;
    public Sprite[] attackFrames;
    public Sprite[] hurtFrames;

    [Header("Velocidad de Animación (FPS)")]
    public float idleFps = 10f;
    public float runFps = 14f;
    public float attackFps = 16f;
    public float hurtFps = 8f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private PlayerController playerController;

    private PlayerAnimState currentState = PlayerAnimState.Idle;
    private int currentFrameIndex = 0;
    private float timer = 0f;
    private bool isLockedAnimation = false; // Bloquea estados mientras dura el ataque o daño

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // 1. Determinar el estado si no hay animación prioritaria (Ataque o Daño)
        if (!isLockedAnimation)
        {
            float speed = Mathf.Abs(rb.linearVelocity.x);

            if (speed > 0.2f)
            {
                ChangeState(PlayerAnimState.Run);
            }
            else
            {
                ChangeState(PlayerAnimState.Idle);
            }
        }

        // 2. Reproducir fotogramas del estado actual
        Sprite[] activeFrames = GetFramesForState(currentState);
        if (activeFrames == null || activeFrames.Length == 0) return;

        float activeFps = GetFpsForState(currentState);
        timer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, activeFps);

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            currentFrameIndex++;

            if (currentFrameIndex >= activeFrames.Length)
            {
                if (currentState == PlayerAnimState.Attack || currentState == PlayerAnimState.Hurt)
                {
                    // Al terminar ataque o daño, volvemos a la normalidad
                    isLockedAnimation = false;
                    currentFrameIndex = 0;
                    ChangeState(PlayerAnimState.Idle);
                    activeFrames = idleFrames;
                }
                else
                {
                    // En Idle o Run hacemos bucle
                    currentFrameIndex = 0;
                }
            }

            if (activeFrames != null && currentFrameIndex < activeFrames.Length && activeFrames[currentFrameIndex] != null)
            {
                spriteRenderer.sprite = activeFrames[currentFrameIndex];
            }
        }
    }

    public void TriggerAttack()
    {
        if (attackFrames != null && attackFrames.Length > 0)
        {
            isLockedAnimation = true;
            ChangeState(PlayerAnimState.Attack);
        }
    }

    public void TriggerHurt()
    {
        if (hurtFrames != null && hurtFrames.Length > 0)
        {
            isLockedAnimation = true;
            ChangeState(PlayerAnimState.Hurt);
        }
    }

    private void ChangeState(PlayerAnimState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        currentFrameIndex = 0;
        timer = 0f;

        Sprite[] frames = GetFramesForState(newState);
        if (frames != null && frames.Length > 0 && frames[0] != null)
        {
            spriteRenderer.sprite = frames[0];
        }
    }

    private Sprite[] GetFramesForState(PlayerAnimState state)
    {
        return state switch
        {
            PlayerAnimState.Idle => idleFrames,
            PlayerAnimState.Run => runFrames,
            PlayerAnimState.Attack => attackFrames,
            PlayerAnimState.Hurt => hurtFrames,
            _ => idleFrames
        };
    }

    private float GetFpsForState(PlayerAnimState state)
    {
        return state switch
        {
            PlayerAnimState.Idle => idleFps,
            PlayerAnimState.Run => runFps,
            PlayerAnimState.Attack => attackFps,
            PlayerAnimState.Hurt => hurtFps,
            _ => idleFps
        };
    }
}
