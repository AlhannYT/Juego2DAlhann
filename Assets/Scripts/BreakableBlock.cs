using UnityEngine;
using System.Collections;

/// <summary>
/// Bloque rompible o de sorpresa estilo Mario.
/// Reacciona cuando el jugador salta y lo golpea desde abajo con la cabeza.
/// </summary>
public class BreakableBlock : MonoBehaviour
{
    [Header("Configuración del Bloque")]
    public bool isQuestionBlock = true; // Si da monedas o si se destruye como ladrillo
    public int coinsInside = 1;
    public Sprite emptyBlockSprite; // Sprite apagado/usado

    private bool isHit = false;
    private Vector3 originalPosition;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void HitFromBelow()
    {
        if (isHit && isQuestionBlock) return;

        StartCoroutine(BounceAnimation());

        if (isQuestionBlock)
        {
            if (coinsInside > 0)
            {
                coinsInside--;
                GameManager.Instance?.AddCoins(1);
            }

            if (coinsInside <= 0)
            {
                isHit = true;
                if (emptyBlockSprite != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = emptyBlockSprite;
                }
            }
        }
        else
        {
            // Bloque de ladrillo destructible
            AudioManager.Instance?.PlaySFX("BlockBreak");
            Destroy(gameObject, 0.15f);
        }
    }

    private IEnumerator BounceAnimation()
    {
        AudioManager.Instance?.PlaySFX("BlockHit");

        float elapsed = 0f;
        float duration = 0.1f;
        Vector3 peakPos = originalPosition + new Vector3(0f, 0.25f, 0f);

        // Subir
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(originalPosition, peakPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Bajar
        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(peakPos, originalPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }
}
