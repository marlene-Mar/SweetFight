using UnityEngine;

public class MoveLava : MonoBehaviour
{
    public float velocidadX = 0.1f;
    public float velocidadY = 0.1f;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float offsetXPaso = Time.time * velocidadX;
        float offsetYPaso = Time.time * velocidadY;
        rend.material.mainTextureOffset = new Vector2(offsetXPaso, offsetYPaso);
    }
}