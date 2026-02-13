using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public Transform player;

    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.y = transform.position.y; // Mantiene la altura de la cámara
            transform.position = newPosition;
        }
    }
}
