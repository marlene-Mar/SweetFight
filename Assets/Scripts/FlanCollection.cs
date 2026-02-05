using UnityEngine;
using Unity.Collections;

public class FlanCollection : MonoBehaviour
{
    private int flanCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Flan"))
        {
            flanCount++;
            Destroy(other.gameObject);
            Debug.Log($"Flanes recogidos: {flanCount}");
        }
    }
}
