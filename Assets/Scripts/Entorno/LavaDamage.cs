using UnityEngine;

// Aplica daño al jugador cuando está en contacto con la lava
public class LavaDamage : MonoBehaviour
{
    [Header("Daño por lava")]
    public int dañoPorTick = 5; // Daño aplicado cada intervalo de tiempo
    public float intervaloDaño = 1.2f; // Intervalo de tiempo entre cada aplicación de daño

    private float tiempoSiguienteDaño; // Tiempo para el próximo daño
    private GameManager gameManager; // Referencia al GameManager para aplicar daño al jugador

    // Inicializa el GameManager y establece el tiempo para el próximo daño
    void Start()
    {
        gameManager = GameManager.Instance;  
        if (gameManager == null)
        {
            Debug.LogError("No se encontró GameManager!");
        }
        tiempoSiguienteDaño = Time.time + intervaloDaño;  
    }

    // Aplica daño continuo mientras el jugador esté en contacto con la lava
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && gameManager != null)
        {
            if (Time.time >= tiempoSiguienteDaño)
            {
                gameManager.TakeDamage(dañoPorTick);
                tiempoSiguienteDaño = Time.time + intervaloDaño;
            }
        }
    }

    //Daño inmediato al entrar en contacto con la lava
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gameManager != null)
        {
            gameManager.TakeDamage(8);  
        }
    }
}