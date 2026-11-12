using UnityEngine;

/// <summary>
/// Obstáculo dañino: Pinchos, fuego o zona de muerte por caída (abismo sin fondo).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    public enum HazardType { Spikes, FallZone, Fire }
    public HazardType type = HazardType.Spikes;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (type == HazardType.FallZone)
            {
                // Caída al abismo: pierde 1 vida y reaparece al inicio
                GameManager.Instance?.HandleFallIntoAbyss();
            }
            else
            {
                // Espinas o fuego: pierde 1 vida si no está en invulnerabilidad temporal
                if (player != null && !player.isInvulnerable)
                {
                    string msg = type == HazardType.Spikes ? "¡CAÍSTE EN LAS ESPINAS!" : "¡TE QUEMASTE CON FUEGO!";
                    GameManager.Instance?.TakeDamage(1, msg);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && !player.isInvulnerable)
            {
                GameManager.Instance?.TakeDamage(1, "¡CHOCASTE CON UN OBSTÁCULO!");
            }
        }
    }
}
