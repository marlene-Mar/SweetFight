using UnityEngine;

// Aplica curación al jugador cuando está en contacto con el agua
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

    // Aplica curación inicial al entrar en contacto con el agua y curación continua mientras el jugador permanezca en el agua
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

    // Aplica curación continua mientras el jugador esté en contacto con el agua
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

    // Limpia la referencia a VidaJugador cuando el jugador salga del agua
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        vidaJugador = null;
        Debug.Log("Jugador salió del agua");
    }
}