using UnityEngine;
using System;

// Clase que se encarga de manejar la vida del jugador, recibir daño, curarse y notificar cambios
public class VidaJugador : MonoBehaviour
{
    public int vidaMaxima = 100;
    public int vidaActual;

    // Referencia al controlador de efectos de golpe para activar el efecto visual al recibir daño
    private HitEffectController efectoGolpe;

    // Eventos para notificar cambios en la vida del jugador
    public Action<int, int> OnVidaChanged;
    public Action OnPlayerDead;
    public Action OnDamageTaken;


    void Start()
    {
        vidaActual = vidaMaxima;
        efectoGolpe = GetComponent<HitEffectController>();
        NotificarCambio();
    }

    // Método para recibir daño, actualizar la vida actual, activar efectos y notificar cambios
    public void RecibirDaño(int cantidad)
    {
        vidaActual -= cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);

        Debug.Log("Daño recibido: " + cantidad);
        if (efectoGolpe != null) 
        {
            efectoGolpe.TriggerHit(); 
        }
        Debug.Log("Vida actual del jugador: " + vidaActual);

        NotificarCambio();

        if (vidaActual <= 0)
            Morir();
    }

    // Método para curar al jugador, actualizar la vida actual y notificar cambios
    public void Curar(int cantidad)
    {
        vidaActual += cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);

        Debug.Log("Curación: " + cantidad);

        NotificarCambio();
    }

    // Método para notificar cambios en la vida del jugador a través del evento OnVidaChanged
    public void NotificarCambio()
    {
        OnVidaChanged?.Invoke(vidaActual, vidaMaxima);
    }

    // Método para manejar la muerte del jugador, activar el evento OnPlayerDead y realizar acciones adicionales si es necesario
    void Morir()
    {
        Debug.Log("Jugador murió");
        OnPlayerDead?.Invoke();
    }
}