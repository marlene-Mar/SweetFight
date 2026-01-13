using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ==============================
    // Paneles de menú principal
    // ==============================
    public GameObject MenuInicial;
    public GameObject MenuPrincipal;
    public GameObject Personajes;
    public GameObject Configuracion;
    public GameObject Audio;
    public GameObject Controles;
    public GameObject canvasMenu;

    private void Start()
    {
        MenuInicial.SetActive(true);
    }

    private void Update()
    {
        // ENTER para pasar del menú inicial al principal
        if (MenuInicial.activeSelf &&
            Keyboard.current != null &&
            Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Debug.Log("Enter detectado");
            GoToMenuPrincipal();
        }
    }

    // ======================
    // MÉTODO CENTRAL
    // ======================
    public void ShowOnlyPanel(GameObject panelToShow)
    {
        if (MenuInicial) MenuInicial.SetActive(panelToShow == MenuInicial);
        if (MenuPrincipal) MenuPrincipal.SetActive(panelToShow == MenuPrincipal);
        if (Personajes) Personajes.SetActive(panelToShow == Personajes);
        if (Configuracion) Configuracion.SetActive(panelToShow == Configuracion);
        if (Audio) Audio.SetActive(panelToShow == Audio);
        if (Controles) Controles.SetActive(panelToShow == Controles);
    }

    // ======================
    // NAVEGACIÓN PRINCIPAL
    // ======================
    public void GoToMenuPrincipal()
    {
        ShowOnlyPanel(MenuPrincipal);
    }

    public void GoToPersonajes()
    {
        ShowOnlyPanel(Personajes);
    }

    public void GoToConfiguracion()
    {
        ShowOnlyPanel(Configuracion);
    }

    public void GoToAudio()
    {
        ShowOnlyPanel(Audio);
    }

    public void GoToControles()
    {
        ShowOnlyPanel(Controles);
    }

    public void GoToHome()
    {
        ShowOnlyPanel(MenuPrincipal);
    }

    // ======================
    // JUEGO
    // ======================
    public void Jugar()
    {
        Debug.Log("Iniciar o regresar al juego");
        canvasMenu.SetActive(false); //Oculta el menú

    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego");
        // Application.Quit();
    }
    public void RegresarAlMenu()
    {
        Debug.Log("Regresando al menú principal");
        ShowOnlyPanel(MenuPrincipal);
    }

}
