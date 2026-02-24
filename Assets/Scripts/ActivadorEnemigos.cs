using UnityEngine;
using UnityEngine.AI;

public class ActivadorEnemigos : MonoBehaviour
{
    public GameObject[] enemigos;
    private bool yaActivado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (yaActivado) return;

        if (other.CompareTag("Player"))
        {
            foreach (GameObject enemigo in enemigos)
            {
                enemigo.SetActive(true);

                NavMeshAgent agent = enemigo.GetComponent<NavMeshAgent>();
                agent.Warp(enemigo.transform.position); // 🔥 esto es la clave
            }

            yaActivado = true;
        }
    }
}