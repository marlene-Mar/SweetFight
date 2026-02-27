using UnityEngine;

// Hace que la textura de la lava se mueva para simular un flujo
public class MoveLava : MonoBehaviour
{
    // Velocidades de movimiento en los ejes X e Y para el offset de la textura
    public float velocidadX = 0.1f;
    public float velocidadY = 0.1f;
    private Renderer rend;

    // Inicializa el Renderer para manipular la textura
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    // Actualiza el offset de la textura para crear el efecto de movimiento
    void Update()
    {
        float offsetXPaso = Time.time * velocidadX;
        float offsetYPaso = Time.time * velocidadY;
        rend.material.mainTextureOffset = new Vector2(offsetXPaso, offsetYPaso);
    }
}