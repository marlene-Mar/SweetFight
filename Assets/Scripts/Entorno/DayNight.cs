using UnityEngine;

// Hace que la luz gire alrededor del eje X para simular el ciclo día-noche
public class DayNight : MonoBehaviour
{
    public float velocidad = 3.0f;

    void Update()
    {
        transform.Rotate(Vector3.right * velocidad * Time.deltaTime);
    }
}
