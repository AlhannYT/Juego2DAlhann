using UnityEngine;

/// <summary>
/// Objeto coleccionable (Monedas, gemas o pergaminos).
/// Suma puntos/monedas al jugador al entrar en contacto.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CollectibleCoin : MonoBehaviour
{
    public int value = 1;
    public float bobSpeed = 3f;
    public float bobHeight = 0.15f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Efecto flotante suave arriba y abajo
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance?.AddCoins(value);
            Destroy(gameObject);
        }
    }
}
