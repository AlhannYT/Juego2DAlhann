using UnityEngine;

/// <summary>
/// Meta del nivel (Puerta / Torii / Bandera).
/// Al tocarla, completa el nivel y avanza a la siguiente escena.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GoalDoor : MonoBehaviour
{
    private bool levelFinished = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (levelFinished) return;

        if (collision.CompareTag("Player"))
        {
            levelFinished = true;
            GameManager.Instance?.FinishLevel();
        }
    }
}
