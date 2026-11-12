using UnityEngine;

/// <summary>
/// Reproductor simple de animación por sprites para 2D.
/// Permite animar cualquier objeto (Murciélago, Zombie, Monedas, etc.)
/// sin necesidad de configurar Animator Controllers complejos.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSpriteAnimator : MonoBehaviour
{
    [Header("Fotogramas de la Animación")]
    public Sprite[] frames;

    [Header("Configuración")]
    public float framesPerSecond = 10f;
    public bool loop = true;
    public bool playOnAwake = true;

    private SpriteRenderer spriteRenderer;
    private int currentFrameIndex = 0;
    private float timer = 0f;
    private bool isPlaying = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        isPlaying = playOnAwake;
    }

    private void Update()
    {
        if (!isPlaying || frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        float timePerFrame = 1f / Mathf.Max(0.1f, framesPerSecond);

        if (timer >= timePerFrame)
        {
            timer -= timePerFrame;
            currentFrameIndex++;

            if (currentFrameIndex >= frames.Length)
            {
                if (loop)
                {
                    currentFrameIndex = 0;
                }
                else
                {
                    currentFrameIndex = frames.Length - 1;
                    isPlaying = false;
                }
            }

            if (spriteRenderer != null && frames[currentFrameIndex] != null)
            {
                spriteRenderer.sprite = frames[currentFrameIndex];
            }
        }
    }

    public void Play()
    {
        isPlaying = true;
        currentFrameIndex = 0;
        timer = 0f;
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void SetFrames(Sprite[] newFrames, float newFps = 10f)
    {
        frames = newFrames;
        framesPerSecond = newFps;
        currentFrameIndex = 0;
        timer = 0f;
        isPlaying = true;
    }
}
