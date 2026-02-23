using UnityEngine;

public class WaterHealingZone : MonoBehaviour
{
    [Header("Curación por agua")]
    public int curacionPorTick = 4;
    public float intervaloCuracion = 1.2f;

    private float tiempoSiguienteCuracion;
    private VidaJugador vidaJugador;

    void Start()
    {
        tiempoSiguienteCuracion = Time.time + intervaloCuracion;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        vidaJugador = other.GetComponent<VidaJugador>();

        if (vidaJugador != null)
        {
            vidaJugador.Curar(6); // curación inicial
            Debug.Log("Jugador entró al agua → curación inicial");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (vidaJugador == null)
            vidaJugador = other.GetComponent<VidaJugador>();

        if (vidaJugador == null) return;

        if (Time.time >= tiempoSiguienteCuracion)
        {
            vidaJugador.Curar(curacionPorTick);
            tiempoSiguienteCuracion = Time.time + intervaloCuracion;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        vidaJugador = null;
        Debug.Log("Jugador salió del agua");
    }
}