using UnityEngine;

public class ZonaActivacion1 : MonoBehaviour
{
    public GameObject[] ratBots; 

    private bool yaActivado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (yaActivado) return;

        if (other.CompareTag("Player"))
        {
            foreach (GameObject ratBots in ratBots)
            {
                ratBots.SetActive(true);
            }

            yaActivado = true;
        }
    }
}
