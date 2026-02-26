using UnityEngine;
using System;

public class VidaJugador : MonoBehaviour
{
    public int vidaMaxima = 100;
    public int vidaActual;

    private HitEffectController efectoGolpe;

    public Action<int, int> OnVidaChanged;
    public Action OnPlayerDead;

    void Start()
    {
        vidaActual = vidaMaxima;
        efectoGolpe = GetComponent<HitEffectController>();
        NotificarCambio();
    }

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

    public void Curar(int cantidad)
    {
        vidaActual += cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);

        Debug.Log("Curación: " + cantidad);

        NotificarCambio();
    }

    public void NotificarCambio()
    {
        OnVidaChanged?.Invoke(vidaActual, vidaMaxima);
    }

    void Morir()
    {
        Debug.Log("Jugador murió");
        OnPlayerDead?.Invoke();
    }
}