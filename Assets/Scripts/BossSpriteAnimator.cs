using UnityEngine;

/// <summary>
/// Animador por sprites ligero y dedicado para el Jefe Final (Rey Esqueleto / Shogun).
/// Sincronizado automáticamente con los estados de BossController:
/// Reposo (IDLE), Caminar (WALK), Ataque (ATTACK), Daño/Fatiga (HURT) y Muerte (DIE).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BossSpriteAnimator : MonoBehaviour
{
    public enum BossAnimState { Idle, Walk, Attack, Hurt, Die }

    [Header("Colecciones de Fotogramas")]
    public Sprite[] idleFrames;
    public Sprite[] walkFrames;
    public Sprite[] attackFrames;
    public Sprite[] hurtFrames;
    public Sprite[] dieFrames;

    [Header("Velocidad de Animación (FPS)")]
    public float idleFps = 10f;
    public float walkFps = 12f;
    public float attackFps = 14f;
    public float hurtFps = 8f;
    public float dieFps = 10f;

    private SpriteRenderer spriteRenderer;
    private BossAnimState currentState = BossAnimState.Idle;
    private int currentFrameIndex = 0;
    private float timer = 0f;
    private bool isLockedAnimation = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
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
                if (currentState == BossAnimState.Attack || currentState == BossAnimState.Hurt)
                {
                    // Al terminar ataque o daño, vuelve a caminar/reposo
                    isLockedAnimation = false;
                    currentFrameIndex = 0;
                    currentState = BossAnimState.Idle;
                    activeFrames = idleFrames;
                }
                else if (currentState == BossAnimState.Die)
                {
                    // En muerte se queda en el último fotograma
                    currentFrameIndex = activeFrames.Length - 1;
                    return;
                }
                else
                {
                    currentFrameIndex = 0;
                }
            }

            if (activeFrames != null && currentFrameIndex < activeFrames.Length && activeFrames[currentFrameIndex] != null)
            {
                spriteRenderer.sprite = activeFrames[currentFrameIndex];
            }
        }
    }

    public void PlayIdle()
    {
        if (isLockedAnimation || currentState == BossAnimState.Die) return;
        ChangeState(BossAnimState.Idle);
    }

    public void PlayWalk()
    {
        if (isLockedAnimation || currentState == BossAnimState.Die) return;
        ChangeState(BossAnimState.Walk);
    }

    public void PlayAttack()
    {
        if (currentState == BossAnimState.Die) return;
        isLockedAnimation = true;
        ChangeState(BossAnimState.Attack);
    }

    public void PlayHurt()
    {
        if (currentState == BossAnimState.Die) return;
        isLockedAnimation = true;
        ChangeState(BossAnimState.Hurt);
    }

    public void PlayDie()
    {
        isLockedAnimation = true;
        ChangeState(BossAnimState.Die);
    }

    private void ChangeState(BossAnimState newState)
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

    private Sprite[] GetFramesForState(BossAnimState state)
    {
        return state switch
        {
            BossAnimState.Idle => idleFrames,
            BossAnimState.Walk => walkFrames,
            BossAnimState.Attack => attackFrames,
            BossAnimState.Hurt => hurtFrames,
            BossAnimState.Die => dieFrames,
            _ => idleFrames
        };
    }

    private float GetFpsForState(BossAnimState state)
    {
        return state switch
        {
            BossAnimState.Idle => idleFps,
            BossAnimState.Walk => walkFps,
            BossAnimState.Attack => attackFps,
            BossAnimState.Hurt => hurtFps,
            BossAnimState.Die => dieFps,
            _ => idleFps
        };
    }
}
