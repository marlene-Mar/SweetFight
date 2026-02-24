using UnityEngine;

public class EnemyActivator : MonoBehaviour
{
    public GameObject enemy; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemy.SetActive(true);
        }
    }
}