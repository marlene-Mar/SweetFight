using UnityEngine;

public class DayNight : MonoBehaviour
{
    public float velocidad = 3.0f;

    void Update()
    {
        transform.Rotate(Vector3.right * velocidad * Time.deltaTime);
    }
}
