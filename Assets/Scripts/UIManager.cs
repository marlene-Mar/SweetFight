using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ==============================
    // Menú principal
    // ==============================
    public GameObject MenuInicial;
    public GameObject MenuPrincipal;
    public GameObject MenuPrincipalBase;
    public GameObject Personajes;
    public GameObject Configuracion;
    public GameObject Audio;
    public GameObject Controles;
    public GameObject canvasMenu;

    // ==============================
    // HUD
    // ==============================
    public GameObject HudPanel;
    public GameObject PausaPanel;
    private bool isPaused = false;

    private ConfigSource configSource;

    public enum ConfigSource
    {
        Menu,
        Game
    }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Estado inicial
        MenuInicial.SetActive(true);

        MenuPrincipalBase.SetActive(false);
        MenuPrincipal.SetActive(false);
        Personajes.SetActive(false);
        Configuracion.SetActive(false);
        Audio.SetActive(false);
        Controles.SetActive(false);

        canvasMenu.SetActive(true);

        HudPanel.SetActive(false);
        PausaPanel.SetActive(false);

        Time.timeScale = 0f; 
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
    public void ShowMenuPanel(GameObject panelToShow)
    {
        MenuPrincipal.SetActive(false);
        Personajes.SetActive(false);
        Configuracion.SetActive(false);
        Audio.SetActive(false);
        Controles.SetActive(false);

        panelToShow.SetActive(true);
    }

    // ======================
    // NAVEGACIÓN PRINCIPAL
    // ======================
    public void GoToMenuPrincipal()
    {
        MenuInicial.SetActive(false);
        MenuPrincipalBase.SetActive(true);
        ShowMenuPanel(MenuPrincipal);
    }

    public void GoToPersonajes()
    {
        ShowMenuPanel(Personajes);
    }

    public void GoToConfiguracion()
    {
        configSource = ConfigSource.Menu;
        ShowMenuPanel(Configuracion);
    }

    public void GoToAudio()
    {
        //ShowMenuPanel(Audio);
        Audio.SetActive(true);
        Controles.SetActive(false);
        Configuracion.SetActive(false);
    }

    public void GoToControles()
    {
        //ShowMenuPanel(Controles);
        Controles.SetActive(true);
        Audio.SetActive(false);
        Configuracion.SetActive(false);
    }

    public void GoToHome()
    {
        ShowMenuPanel(MenuPrincipal);
    }

    // ======================
    // JUEGO
    // ======================
    public void Jugar()
    {
        Debug.Log("Iniciar o regresar al juego");
        MenuPrincipal.SetActive(false); //Oculta el menú
        MenuPrincipalBase.SetActive(false); //Oculta el menú
        HudPanel.SetActive(true); //Muestra el HUD
        PausaPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego");
        // Application.Quit();
    }

    public void RegresarAlMenu()
    {
        Debug.Log("Regresando al menú principal");
        ShowMenuPanel(MenuPrincipal);
    }

    public void AbrirConfiguracionDesdeJuego()
    {
        configSource = ConfigSource.Game;

        Configuracion.SetActive(true);
        PausaPanel.SetActive(false);
    }

    public void CerrarConfiguracion()
    {
        if (configSource == ConfigSource.Menu)
        {
            // Regresa al menú principal
            ShowMenuPanel(MenuPrincipal);
        }
        else if (configSource == ConfigSource.Game)
        {
            // Regresa al juego
            Configuracion.SetActive(false);
            PausaPanel.SetActive(true);
            HudPanel.SetActive(true);

            Time.timeScale = 1f;
        }
    }

    public void VolverAConfiguracion()
    {
        Audio.SetActive(false);
        Controles.SetActive(false);
        Configuracion.SetActive(true);
    }


    public void PausarJuego()
    {
        if (PausaPanel.activeSelf) return;

        PausaPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ReanudarJuego()
    {
        PausaPanel.SetActive(false);
        HudPanel.SetActive(true);
        Time.timeScale = 1f;
    }

    public void GuardarYSalir()
    {
        Debug.Log("Guardado");
        PausaPanel.SetActive(false); 
        HudPanel.SetActive(false);
        MenuPrincipal.SetActive(true);
        Salir();
    }

}
