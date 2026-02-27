using UnityEngine;

// Hace que el flan gire lentamente para darle un efecto visual
public class RotarFlan : MonoBehaviour
{
    public Vector3 velocidadRotacion = new Vector3(0, 50, 0);

    void Update()
    {
        transform.Rotate(velocidadRotacion * Time.deltaTime);
    }
}
