using UnityEngine;

public class LavaDamage : MonoBehaviour
{
    [Header("Daño por lava")]
    public int dañoPorTick = 5;
    public float intervaloDaño = 1.2f; 

    private float tiempoSiguienteDaño;
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;  
        if (gameManager == null)
        {
            Debug.LogError("No se encontró GameManager!");
        }
        tiempoSiguienteDaño = Time.time + intervaloDaño;  
    }

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

    // Opcional: daño inicial fuerte
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gameManager != null)
        {
            gameManager.TakeDamage(8);  
        }
    }
}