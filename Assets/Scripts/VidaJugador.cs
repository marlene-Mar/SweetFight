using UnityEngine;

public class VidaJugador : MonoBehaviour
{
    public int vidaMaxima = 100;
    public int vidaActual;

    public float curacionPorSegundoEnAgua = 4f;     
    public bool estaEnAgua = false;

    public float curacionFLan = 15f;            
    public float tiempoEntreBocados = 1.2f;          
    private float tiempoUltimoBocado;

    void Start()
    {
        vidaActual = vidaMaxima;
    }

    private void Update()
    {
        // Curación pasiva cuando está en el agua
        if (estaEnAgua && vidaActual < vidaMaxima)
        {
            float curacionEsteFrame = curacionPorSegundoEnAgua * Time.deltaTime;
            Curar(curacionEsteFrame);
        }
    }
    public void RecibirDaño(float cantidad)
    {
        vidaActual -= Mathf.RoundToInt(cantidad);
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    //public void RecibirDaño(int cantidad)
    //{
    //    vidaActual -= cantidad;

    //    if (vidaActual <= 0)
    //    {
    //        Morir();
    //    }
    //}

    public void Curar(float cantidad)
    {
        vidaActual += Mathf.RoundToInt(cantidad);
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);
    }

    // Para usar desde otros scripts (UI, inventario, etc.)
    public bool PuedeCurarseConItem()
    {
        return Time.time >= tiempoUltimoBocado + tiempoEntreBocados
            && vidaActual < vidaMaxima;
    }

    //public void CurarConItem()
    //{
    //    if (!PuedeCurarseConItem()) return;

    //    Curar(curacionFlan);
    //    tiempoUltimoBocado = Time.time;

    //    // Aquí puedes restar 1 del inventario (lo veremos más abajo)
    //    Debug.Log("¡Curado con recurso! +" + curacionFlan);
    //}

    private void Morir()
    {
        Debug.Log("¡Has muerto!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            estaEnAgua = true;
            Debug.Log("Entraste al agua → curación pasiva activada");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(""))
        {
            estaEnAgua = false;
            Debug.Log("Saliste del agua");
        }
    }
}