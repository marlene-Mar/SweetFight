using UnityEngine;

public class CandyCollection : MonoBehaviour
{
    private GameManager gameManager ;

    void Start()
    {
        gameManager= FindObjectOfType<GameManager>();
        if (gameManager == null)
            Debug.LogError("NO se encontró CandyManager en la escena");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Candy"))
        {
            Debug.Log("Detecté candy");
            gameManager.AddCandy(1);
            Destroy(other.gameObject);
        }
    }
}