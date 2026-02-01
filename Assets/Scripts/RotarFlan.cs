using UnityEngine;

public class RotarFlan : MonoBehaviour
{
    public Vector3 velocidadRotacion = new Vector3(0, 50, 0);

    void Update()
    {
        transform.Rotate(velocidadRotacion * Time.deltaTime);
    }
}
