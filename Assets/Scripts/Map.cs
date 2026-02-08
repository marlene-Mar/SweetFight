using UnityEngine;

public class Map : MonoBehaviour
{
    public float velocidad = 3.0f;

    void Update()
    {
        transform.Rotate(Vector3.right * velocidad * Time.deltaTime);
    }

}
