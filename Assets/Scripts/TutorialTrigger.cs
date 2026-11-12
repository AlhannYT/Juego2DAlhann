using UnityEngine;

/// <summary>
/// Zona de activación invisible (Trigger) para alternar tutoriales o avisos en pantalla.
/// Al cruzar este punto, oculta el texto anterior (ej. Controles1) y muestra el nuevo (ej. Controles2).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("Objetos de la Interfaz a Alternar")]
    public GameObject objectToHide; // Arrastra aquí: Controles1
    public GameObject objectToShow; // Arrastra aquí: Controles2

    [Header("Configuración")]
    public bool triggerOnce = true;
    public float autoHideAfterSeconds = 0f; // Pon 0 si quieres que se quede visible, o ej. 4 para que desaparezca solo

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered && triggerOnce) return;

        if (collision.CompareTag("Player"))
        {
            hasTriggered = true;

            if (objectToHide != null)
            {
                objectToHide.SetActive(false);
            }

            if (objectToShow != null)
            {
                objectToShow.SetActive(true);
            }

            if (autoHideAfterSeconds > 0f)
            {
                Invoke(nameof(HideAfterTime), autoHideAfterSeconds);
            }
        }
    }

    private void HideAfterTime()
    {
        if (objectToShow != null)
        {
            objectToShow.SetActive(false);
        }
    }
}
