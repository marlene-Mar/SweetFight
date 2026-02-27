using UnityEngine;

// Clase que se encarga de detectar la colision con los caramelos y sumar puntos al jugador
public class CandyCollection : MonoBehaviour
{
    private GameManager gameManager ;

    void Start()
    {
        gameManager= FindObjectOfType<GameManager>();
        if (gameManager == null)
            Debug.LogError("NO se encontro CandyManager en la escena");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Candy"))
        {
            Debug.Log("Detecta candy");
            gameManager.AddCandy(1);
            Destroy(other.gameObject);
        }
    }
}