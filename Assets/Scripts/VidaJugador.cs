using UnityEngine;
using System;

public class VidaJugador : MonoBehaviour
{
    public int vidaMaxima = 100;
    public int vidaActual;

    public Action<int, int> OnVidaChanged;
    public Action OnPlayerDead;

    public HitEffectController hitEffect;

    void Start()
    {
        vidaActual = vidaMaxima;
        NotificarCambio();
    }

    public void RecibirDaño(int cantidad)
    {
        vidaActual -= cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);

        Debug.Log("Daño recibido: " + cantidad);

        hitEffect?.TriggerHit();

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

    void NotificarCambio()
    {
        OnVidaChanged?.Invoke(vidaActual, vidaMaxima);
    }

    void Morir()
    {
        Debug.Log("Jugador murió");
        OnPlayerDead?.Invoke();
    }
}